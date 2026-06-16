# MapTool Plan — Stage Map Editor (Unity EditorWindow 기반)

작성일: 2026-06-16 (개정: EditorWindow 모델로 전환)
대상 프로젝트: `DefenceGame/` (Unity 6, runtime-driven JSON 스테이지)
범위: 스테이지 맵을 (a) 손으로 칠하거나 (b) 시드/규칙으로 자동 생성하고 (c) 둘을 섞어 다듬을 수 있는 **Unity EditorWindow 기반 전용 툴** 설계.

---

## 0. TL;DR

기존 `StageEditor` (Assets/Scripts/Gameplay/Systems/StageEditor.cs) 는 *런타임 오버레이 + F1/F2 키 두 모드*만 있고, terrain은 직사각형 한 종류만 그릴 수 있다. 첫 단계로 런타임 오버레이를 떼어내고 `Assets/Editor/StageEditorWindow.cs` (UnityEditor.EditorWindow, 메뉴: **Tools → Stage Editor**, 단축키 `Ctrl+Alt+E`) 로 옮겼다. 이제 **본 계획은 그 window 를 "MapTool" 로 확장**해서 다음 두 워크플로를 한 도킹 패널에서 굴린다.

1. **Manual workflow** — 셀 단위 페인팅 (붓 크기 / 도형 / 레이어 / undo) + 자원·exit·spawn·encounter 마커 배치.
2. **Procedural workflow** — `WorldGenerator`/`ResourceScatter`/`PathPlanner` 가 만들 결과를 *비파괴* 미리보기로 띄우고, 시드·노이즈·임계값·클러스터 weight 를 슬라이더로 조절 → "Commit" 시 결과를 `terrain.regions` 또는 `terrain.overrides` 또는 `generator` 키로 굳힌다.

두 워크플로의 출력은 같은 `StageDef` JSON 으로 합쳐진다. **레이어 우선순위는 기존 `StageWorld.Build` 와 동일** (Generator → Regions → Path → Overrides) — 새 툴은 어떤 레이어에 쓸지 사용자가 선택만 한다.

**Editor-only 보장**: `Assets/Editor/Game.Editor.asmdef` 의 `includePlatforms: ["Editor"]` + 모든 파일 `#if UNITY_EDITOR` 가드. 플레이어 빌드에 한 줄도 안 섞인다.

M0(완료) → M1 → M6 단계로 분할. 각 단계는 독립적으로 verifiable.

---

## 1. 현재 상태 스냅샷 (2026-06-16 갱신)

| 영역 | 상태 |
|---|---|
| `StageEditorWindow` (NEW) | `Assets/Editor/StageEditorWindow.cs` — IMGUI EditorWindow. 메뉴 `Tools/Stage Editor`. Play 모드일 때만 활성. 모드 토글 + 팔레트 + Save + Status. |
| `Game.Editor` asmdef (NEW) | `includePlatforms: ["Editor"]`. 빌드에서 자동 제외. references: Game.Data / Game.Gameplay / Game.Loaders. |
| `StageEditor` (runtime) | F1 = Resource (5 슬롯), F2 = Terrain (9 슬롯, rect drag only). Ctrl+S 가 `resources.authored` 또는 `terrain.regions` 한쪽만 덮어쓴다. `#if UNITY_EDITOR` 가드. **EditorWindow 의 brain — UI 만 떼어내고 input/ghost/IO 는 그대로**. |
| `StageEditorView` (deprecated) | 삭제됨. 런타임 오버레이는 더 이상 존재하지 않는다. |
| `StageLoader.SaveAuthored / SaveRegions` | JSON 부분 패치 가능. 다른 키 (`exits`, `encounters`, `resources.procedural`, `enemy_spawn_zones`, `generator`) 는 패치 helper 없음. |
| `StageWorld.Build` | Generator → Regions → Path → Overrides → tall_grass sprinkle. 결정론적, 순수 함수. → **미리보기 재계산이 안전**. |
| `WorldGenerator` | biome noise + thresholds 기반. 시드는 `stage.Seed`. |
| `ResourceScatter` | Poisson disc + blob, biome weights fallback. `IsHabitable` 으로 water/cliff 제외. |
| `PathPlanner.Apply` | spawn/core/exits 사이 자동 경로. 현재 미사용 stage 가 다수. |
| `Session` / `StageRuntime` | runtime stage swap 지원 (`Load(stageId)`). 툴이 stage 전환을 트리거하기 좋다. |

요약: **파이프라인은 결정론적·순수해서 미리보기 친화적**. window 스캐폴드는 깔렸고, 이제 *기능을 채워 넣는* 단계.

---

## 2. 디자인 비전

> "한 EditorWindow 안에서 시드 돌려보고, 마음에 들면 굳히고, 안 들면 손으로 덮는다."

핵심 원칙:

1. **Editor-only by construction** — 모든 신규 파일은 `Assets/Editor/` 아래 + `Game.Editor` asmdef. 플레이어 빌드에 코드 한 줄도 안 들어간다.
2. **Play-mode 작업 + Edit-mode 메타 작업 분리**
   - *Play mode* — 실제 페인팅, ghost 미리보기 (Camera.main, StageRuntime 필요).
   - *Edit mode* — stage 메타데이터 편집 (bounds, exits, encounters, generator params). JSON 만 만진다. Play 진입 안 해도 됨.
   - Window 가 두 상태를 구분해서 가능한 작업만 활성화.
3. **비파괴 미리보기** — Procedural 결과는 항상 *별도 레이어* 로 그린다 (Tilemap secondary layer 또는 SceneView overlay sprite, Edit mode 에서는 `SceneView.duringSceneGui` Gizmos). Commit 전엔 JSON 을 건드리지 않는다.
4. **레이어 = JSON 섹션 1:1** — 사용자가 "어디에 쓸지" 명시:
   - Generator (auto/hybrid) → `stage.generator`
   - Regions → `stage.terrain.regions`
   - Overrides → `stage.terrain.overrides`
   - Authored resources → `stage.resources.authored`
   - Procedural clusters → `stage.resources.procedural.clusters`
   - Markers (spawn/core/exits/encounters/enemy_spawn_zones) → 각자
5. **Single source of truth = JSON on disk** — 툴은 in-memory `StageDef` scratchpad 를 편집, Ctrl+S 로 JSON 부분 패치. Play mode 면 `StageRuntime.NotifyChanged()` 로 즉시 화면 반영, Edit mode 면 다음 Play 진입 때 반영.
6. **Undo/redo 는 JSON diff 기준** — 모든 commit 은 `StageDef` 의 immutable 스냅샷 (JObject deep clone) 을 stack 에 push. 50 step. EditorWindow 의 Undo 시스템과 연동 (`Undo.RegisterCompleteObjectUndo` 호환).

---

## 3. 핵심 워크플로 시나리오

### 시나리오 A — "맨손 으로 작은 stage 그리기"
1. **Edit mode** — Tools → Stage Editor → New Stage → bounds 입력 (60×40), biome 선택. JSON 파일이 디스크에 생성됨.
2. **Edit mode** — Marker 탭 → spawn_point, core_point 클릭 배치. ExitDef 입력. EncounterDef 설정.
3. **Play 진입** — Window 가 자동으로 painting UI 활성.
4. **Play mode** — Terrain 탭 → land 페인트 (붓 5×5) → water rectangle → path freehand.
5. **Play mode** — Resource 탭 → Wood 클러스터 페인트 (자동 노드 채움).
6. **Ctrl+S** → JSON 저장 → 같은 Play 세션에서 즉시 검증.

### 시나리오 B — "시드 돌려 베이스 만들고 손으로 다듬기"
1. **Edit mode** — Window 열기 → 기존 stage 선택. Procedural 탭 → seed 입력 / scrub → SceneView 에 gizmo 미리보기 자동 갱신 (Edit mode 에서도 `WorldGenerator.GenerateGrid` 는 순수 함수).
2. **Edit mode** — biome.noise/thresholds 슬라이더 조정 → 미리보기 즉시 갱신.
3. **Edit mode** — "Commit baseline → Generator" → `stage.generator` 키만 JSON 에 저장. 그리드 자체는 안 굳힘.
4. **Play 진입** → 게임 안 결과 확인.
5. **Play mode** — Terrain 탭 → 추가 paint 는 `overrides` 로 → generator 위에 덮어쓰기.
6. **Ctrl+S** → 저장.

### 시나리오 C — "기존 stage 자원만 다시 뿌리기"
1. **Edit mode** 또는 **Play mode** — Window 에서 stage 로드.
2. Resource 탭 → 모든 `procedural.clusters` 표시.
3. cluster row 선택 → "Regenerate" → 시드 변경 → 미리보기.
4. "Apply" → 해당 cluster 의 결과를 `authored` 로 굳히거나, 그대로 `procedural` 유지.

---

## 4. Phase 별 작업

각 phase 는 1~2일 이내에 끝낼 수 있는 단위. 이전 phase 의 결과 위에 점진적으로 쌓는다.

### M0 — EditorWindow 스캐폴드 ✅ (완료, 2026-06-16)

- ✅ `Assets/Editor/Game.Editor.asmdef` (`includePlatforms: ["Editor"]`).
- ✅ `Assets/Editor/StageEditorWindow.cs` — IMGUI EditorWindow, 메뉴 `Tools/Stage Editor` (`Ctrl+Alt+E`).
- ✅ 모드 토글 (Resource/Terrain) + 팔레트 + Active 토글 + Save 버튼 + Status + Hotkeys cheat sheet.
- ✅ 런타임 오버레이 `StageEditorView` 삭제. `GameUI.cs` / `Game.UI.csproj` 정리.
- ✅ Play 모드 외엔 `HelpBox` 로 안내.

### M1 — Foundation: 탭 구조 + 미리보기 인프라 + Scratchpad ✅ (완료, 2026-06-16)

| # | 작업 | 상태 |
|---|---|---|
| M1.1 | `StageEditorWindow` UIElements (UI Toolkit) EditorWindow 마이그레이션. 코드형 UIElements (기존 GameUI/HudView 와 동일한 스타일 — UXML 없이 VisualElement 직접 조립). | ✅ |
| M1.2 | 탭 5개 (`Terrain` / `Resource` / `Marker` / `Procedural` / `Stage`). `IMapTab` 인터페이스 + `Title` / `Root` / `Bind` / `Refresh` 계약. 각각 `Assets/Editor/Tabs/*.cs`. | ✅ |
| M1.3 | `Assets/Editor/Preview/EditorPreviewLayer.cs` — `SceneView.duringSceneGui` + `Handles.DrawSolidRectangleWithOutline`. Play-mode tilemap overlay 는 M3+ 로 이연. | ✅ (Edit 측만) |
| M1.4 | `Assets/Editor/Core/StageScratchpad.cs` — JObject Root + `StageDef` view + undo/redo stack (50, top-N 유지) + dirty flag + SessionState 로 마지막 stage 자동 로드. Ctrl+Z/Ctrl+Shift+Z/Ctrl+Y/Ctrl+S `KeyDownEvent` shortcut. | ✅ |
| M1.5 | Window 하단 status bar: mode + 선택 팔레트 · SceneView 마우스 tile · cell purpose (Play mode 에선 `TerrainPainter.LastGrid` 조회) · undo/redo count. 10Hz 폴링. | ✅ |
| M1.6 | EditorWindow 가 기존 `StageEditor` 싱글톤을 직접 제어 (`ToggleMode` / `Select` / `Save`). Play scene 의 MonoBehaviour 가 필요한 부분 전부 그쪽으로 위임. | ✅ |

**검증** (수동, Play/Edit 양쪽): 탭 5개 토글 OK, Stage 탭의 Rescan/Load 로 stage1_1~1_3 로드 OK, scratchpad Undo stack push/pop OK, Ctrl+S 저장 시 dirty flag 클리어 OK, SceneView 마우스 위치 → tile status 갱신 OK. **Play-mode 미리보기 (M1.3 두 번째 갈래) + Tilemap overlay 는 M3 와 묶어서 진행.**

### M2 — Terrain 편집 강화 ✅ (1차 완료, 2026-06-16 / deferred 항목 표시)

| # | 작업 | 상태 |
|---|---|---|
| M2.1 | 붓 크기 (1/3/5/9), 도형 (rect / disc). `line`/`freehand` 는 M2.1b 로 분리, 추후 진행. | ✅ (rect/disc 만) |
| M2.2 | 레이어 토글: Regions / Overrides 버튼 → 페인트가 어느 list 에 들어갈지 결정. | ✅ |
| M2.3 | Region 인스펙터: 현재 stage 의 `terrain.regions` + `terrain.overrides` 목록 → 항목별 ✕ 버튼으로 삭제. 우클릭 컨텍스트(복제/하이라이트)는 M2.3b 로 분리. | ✅ (삭제만) |
| M2.4 | 드래그 중 ghost preview — `EditorPreviewLayer` 에 PaintSession 이 PreviewRect push. (RegionRasterizer 통한 disc/ring 정밀 미리보기는 M2.4b.) | ✅ (rect bbox) |
| M2.5 | Eraser: "default" purpose 로 도형 paint → overrides 에서 해당 셀 제거. **M2.5 로 분리, 추후 진행.** | ⏳ deferred |
| M2.6 | `StageLoader.SavePartial(stageId, JObject patch)` 추가 — 임의 top-level key 부분 패치. | ✅ |
| M2.7 | (Edit mode) `SceneView` 클릭으로 페인팅 — `SceneViewPaintInput` + `HandleUtility.GUIPointToWorldRay` (Alt+drag 는 카메라 우선). | ✅ |

**아키텍처**: 새 namespace `Game.Editor.Painting` 에 `PaintBrush` (정적 brush 상태) / `PaintSession` (drag 상태 + commit) / `SceneViewPaintInput` (SceneView 이벤트 후킹) 3개 파일. TerrainTab 은 PaintBrush 만 조작; SceneView 클릭→PaintSession Begin/Update/End→Scratchpad.Mutate. Play mode 에서는 commit 직후 `StageRuntime.SetStage(_scratchpad.Stage)` 로 live tilemap 즉시 갱신.

**검증**: rect 5×5 / disc r=2 페인트 → terrain.regions JArray 에 정확히 한 항목 추가. Layer 토글 시 overrides JArray 로 라우팅. Region 인스펙터 ✕ 클릭으로 삭제, Undo 로 복구. Edit mode SceneView 페인트는 Alt 누르면 비활성 (카메라 우선).

**Deferred 항목 — M2 추후 패스**:
- **M2.1b** — 라인(2점) / freehand 도형. EditorWindow event 로 키 `[`/`]` 사이즈 변경.
- **M2.3b** — Region 우클릭 컨텍스트(복제/SceneView 하이라이트), ListView 항목 선택 시 일시 outline.
- **M2.4b** — RegionRasterizer 호출 후 셀 단위 정밀 ghost (현재는 bbox).
- **M2.5** — Eraser purpose + 결과적으로 generator 베이스로 복귀.

### M3 — Resource 편집 강화

| # | 작업 |
|---|---|
| M3.1 | 자원 페인터: 단일 노드 + 클러스터 페인트. 클러스터는 `ProceduralClusterDef` (item, count, cluster_size, amount_range) 슬라이더로 즉시 미리보기. |
| M3.2 | Amount 슬라이더 (50/100/200/500/Rich center) — IsRich 노드는 1.35× 시각화. |
| M3.3 | 멀티 셀렉트 (Shift+drag → rect select) → 일괄 이동/삭제/복제. EditorWindow 의 ListView 와 양방향 셀렉트. |
| M3.4 | "Resource layer" 탭: authored 와 procedural 노드를 색으로 구분 (authored = 노랑, procedural = 연두, hidden_cache = 보라). |
| M3.5 | `StageLoader.SaveProceduralClusters(stageId, clusters)` 추가. |

**검증**: 클러스터 한 줄 드래그 → 노드 12개 자동 배치, 슬라이더로 cluster_size 변경 → 미리보기 즉시 갱신, Ctrl+S 후 다시 로드해도 동일한 분포.

### M4 — Marker 편집 (Edit mode 에서도 가능)

| # | 작업 |
|---|---|
| M4.1 | Marker 탭: spawn_point / core_point 는 단일 마커. **Edit mode**: SceneView Handle 드래그. **Play mode**: Game view 클릭. |
| M4.2 | `ExitDef` 편집: direction dropdown (`east`/`west`/`north`/`south`), target stage 드롭다운 (Assets/StreamingAssets/Data/stages 의 다른 stage id 들), gate_tile SceneView 클릭 배치. |
| M4.3 | `EncounterDef` 편집: regions 다중선택 (chip), chance_per_step 슬라이더, pool 멀티 셀렉트 (현재 biome 의 enemy_pool 디폴트 + 임의 추가). |
| M4.4 | `EnemySpawnZoneDef` 편집: rect drag, weight 슬라이더, id 자동 부여. |
| M4.5 | 마커 시각화: SceneView gizmo (`Handles.DrawWireDisc`) + 색깔 라벨 (`spawn` 연두, `core` 보라, `exit` 노랑). Play mode 면 동일 색의 sprite. |
| M4.6 | `StageLoader.SaveExits(stageId, IList<ExitDef>)`, `SaveEncounters(stageId, EncounterDef)`, `SaveEnemySpawnZones(...)` 추가. |

**검증**: Edit mode 에서 stage1_1 의 spawn/core/exit 마커를 SceneView 핸들로 옮긴 뒤 Ctrl+S → JSON 에 좌표 정확히 반영. Play 진입 시 ExitGate 가 새 위치에 스폰.

### M5 — Procedural 탭 (시드/노이즈 튜닝, 미리보기, commit)

| # | 작업 |
|---|---|
| M5.1 | 시드 입력 (텍스트) + scrub (마우스 휠) + "Randomize" 버튼. 모든 변경은 `StageScratchpad.Stage.Seed` 갱신 + 미리보기 재계산. Edit/Play 양쪽 동작. |
| M5.2 | Generator 슬라이더: noise scale / octaves / lacunarity / persistence / thresholds(water/beach/land) — biome 기본값 위에 stage-level override 로 저장. |
| M5.3 | Resource cluster 패널: biome.resource_weights 기반 자동 생성 슬라이더 (전체 budget, per-item weight). |
| M5.4 | "Live preview" 토글: 갱신 빈도 (즉시 / 1초마다 / 수동). 큰 stage 에서 framerate 안전. EditorWindow 의 `EditorApplication.update` throttle. |
| M5.5 | "Commit Baseline → Regions": 현재 미리보기 결과를 rect 압축해서 `terrain.regions` 로 변환 (`RegionDef.shape="rect"` 시리즈). 압축 알고리즘: greedy horizontal run, then vertical merge. |
| M5.6 | "Commit Baseline → Generator": commit 대신 `stage.generator = { mode:"auto", noise_scale, water_level, octaves, ... }` 로만 저장 — 실제 grid 는 런타임에 재생성. **이게 디폴트**. M5.5 는 옵션. |
| M5.7 | Cluster regen: per-row "Regenerate" 버튼 → 해당 cluster 만 시드 변경. |

**검증**: 시드를 7번 바꾸면서 SceneView 미리보기가 안 끊기고 갱신. "Commit baseline → Generator" 후 JSON 에 `generator` 키만 수정됨. 런타임 로드 결과 미리보기와 동일.

### M6 — Stage 매니지먼트 + QoL

| # | 작업 |
|---|---|
| M6.1 | Stage 탭: 디스크의 stage 리스트 (StreamingAssets/Data/stages) → 더블클릭으로 로드, New Stage / Duplicate / Rename / Delete. Edit mode 에서 동작. |
| M6.2 | Bounds 편집: width/height 입력 → grid 리사이즈. 셀 보존 (잘리는 셀은 경고). |
| M6.3 | Validation: spawn↔core 경로 존재? exit gate 가 bounds 안인가? encounter pool 의 모든 enemy id 가 enemies/basic.json 에 있는가? Window 상단 경고 패널. |
| M6.4 | Auto-backup: Ctrl+S 시 이전 JSON 을 `Backup/stage1_1.json.bak-<ts>` 로 복사 (최근 5개 유지). Edit mode 도 동일. |
| M6.5 | 미니맵 (Window 하단 200×200): 현재 stage 전체 + 카메라 사각형 (Play mode) 또는 SceneView 카메라 (Edit mode). 클릭으로 카메라 점프. |
| M6.6 | Hotkey overlay: `?` 키로 단축키 cheat sheet 표시. |
| M6.7 | (옵션) `[InitializeOnLoadMethod]` 으로 Editor 시작 시 마지막 작업 stage 자동 로드. |

**검증**: stage1_1 복제 → stage1_4 로 rename → bounds 80×60 으로 리사이즈 → 저장 → Play 진입 후 게임에서 ExitGate 로 진입 가능.

---

## 5. 데이터 스키마 변경

대부분 기존 `StageDef` 로 충분. 다음만 추가:

```jsonc
// stages/stage1_1.json — 변경 사항
{
  "generator": {                  // 기존 GeneratorDef 활용
    "mode": "auto",
    "noise_scale": 0.05,
    "water_level": 0.30,
    // M5.6 신규
    "octaves": 4,
    "lacunarity": 2.0,
    "persistence": 0.5,
    "thresholds": { "water": 0.25, "beach": 0.32, "land": 0.85 }
  },
  "terrain": {
    "regions":   [ /* M5.5 시 자동 채움 */ ],
    "overrides": [ /* 사용자 hand-edit */ ]
  },
  "_editor": {                    // optional, 툴 메타데이터
    "last_camera": { "x": 30, "y": 20, "zoom": 1.0 },
    "version": 1
  }
}
```

`GeneratorDef` 에 `octaves/lacunarity/persistence/thresholds` 필드 추가 (현재는 biome 기본만 사용). 기존 stage 호환을 위해 모두 nullable + biome fallback.

새 helper (모두 `StageLoader` 에, Editor asmdef 는 Game.Loaders 참조):
- `StageLoader.SavePartial(stageId, JObject patchKeys)`
- `StageLoader.SaveExits(stageId, IList<ExitDef>)`
- `StageLoader.SaveEncounters(stageId, EncounterDef)`
- `StageLoader.SaveGenerator(stageId, GeneratorDef)`
- `StageLoader.SaveProceduralClusters(stageId, IList<ProceduralClusterDef>)`
- `StageLoader.SaveEnemySpawnZones(stageId, IList<EnemySpawnZoneDef>)`

---

## 6. UI 레이아웃 스케치 (UIElements EditorWindow, dockable)

```
┌─────────────────────────────────────────────────────┐
│ Stage Editor                              [_][□][X] │
├─────────────────────────────────────────────────────┤
│ Stage: [stage1_1 ▼]  [New] [Dup] [Save]  ● dirty   │
├─────────────────────────────────────────────────────┤
│ [Terrain] [Resource] [Marker] [Procedural] [Stage] │
├─────────────────────────────────────────────────────┤
│  Palette                                            │
│  ┌──┬──┬──┬──┬──┬──┬──┬──┬──┐                      │
│  │L │W │B │C │P │WB│HP│VP│CR│                      │
│  └──┴──┴──┴──┴──┴──┴──┴──┴──┘                      │
│                                                     │
│  Brush size: ◯ 5  Shape: ▭ rect  Layer: ● override │
│                                                     │
│  ──── Region Inspector ──────────────────────────── │
│  ▾ Regions (3)                                      │
│    • #1 land    rect 0,0 60x10                     │
│    • #2 water   rect 0,30 60x10  [highlight][del] │
│    • #3 path    rect 28,10 4x20                    │
│                                                     │
│  ──── Validation ────────────────────────────────── │
│   ⚠ spawn → core 경로 끊김                          │
├─────────────────────────────────────────────────────┤
│ (30,18) land | Play | mode:Terrain/override/rect5  │
└─────────────────────────────────────────────────────┘
              SceneView / GameView 에 ghost preview
              + region highlight + marker handles
```

Window 는 자유롭게 도킹 가능 (Inspector 옆, 별도 모니터, 떠다니는 창). UI Toolkit 사용 시 UXML/USS 로 정리.

---

## 7. 리스크 / 결정 포인트

| 리스크 | 완화 |
|---|---|
| Editor asmdef 는 MonoBehaviour 가 scene 에 못 붙음 (Play scene 컴포넌트가 안 됨) | Play-mode brain 은 기존 `StageEditor` (Game.Gameplay asmdef) 유지. EditorWindow 는 그 싱글톤을 *제어* 만. |
| Edit mode 에서 페인팅 입력 처리 | `SceneView.duringSceneGui += OnSceneGUI` 콜백 + `Event.current` 로 마우스. Input System 안 쓴다. |
| Edit mode 미리보기 렌더링 | `Handles.DrawSolidRectangleWithOutline`, `Gizmos`, 또는 SceneView 에 임시 GameObject (HideFlags.HideAndDontSave) 후 도메인 reload 시 cleanup. |
| 미리보기 비용 (큰 stage 에서 매 프레임 재생성) | M5.4 throttle + dirty flag. 미리보기는 변경시에만 한 번. `EditorApplication.update` 폴링 10Hz 상한. |
| Regions 압축 (M5.5) 결과가 노이지해서 rect 수천 개로 폭발 | 디폴트는 M5.6 (generator 저장). M5.5 는 "Bake" 옵션, 압축 알고리즘에 max-rect-count 가드. |
| 빌드에 editor 코드 섞일 위험 | `Game.Editor.asmdef` `includePlatforms: ["Editor"]` + 모든 파일 `#if UNITY_EDITOR` (double-belt). |
| Undo stack 메모리 (StageDef 통째 복사) | StageDef → JObject snapshot. JObject deep clone 은 수 KB. 50 step OK. Unity `Undo` 와 묶어서 그룹화. |
| Domain reload 시 scratchpad/preview 휘발 | `[SerializeField]` + `EditorWindow` 의 serialization 활용. Play↔Edit 전환 시 scratchpad 보존. |
| JSON 부분 패치가 다른 키를 망가뜨릴 위험 | `SavePartial` 은 JObject 단위 swap 만. EditMode 테스트로 라운드트립 보장. |
| Resource scatter 시드 동기화 | 클러스터별 seed 분리 — `stage.Seed` XOR cluster index, 기존 로직 그대로. |
| 스키마 변경 시 옛 JSON | `StageDef.Version` 추가 + StageLoader 에 마이그레이션 hook 한 줄. M1 에서 같이. |

---

## 8. 산출물 체크리스트

- [x] `Assets/Editor/Game.Editor.asmdef` (`includePlatforms: ["Editor"]`) ✅ M0
- [x] `Assets/Editor/StageEditorWindow.cs` (IMGUI 스캐폴드) ✅ M0
- [x] `Assets/Scripts/UI/StageEditorView.cs` 삭제 ✅ M0
- [x] `GameUI.cs` / `Game.UI.csproj` 의 StageEditorView 참조 제거 ✅ M0
- [x] `Assets/Editor/Tabs/{IMapTab,Terrain,Resource,Marker,Procedural,Stage}.cs` ✅ M1
- [x] `Assets/Editor/Core/StageScratchpad.cs` (undo, dirty, JObject snapshot) ✅ M1
- [x] `Assets/Editor/Preview/EditorPreviewLayer.cs` (Edit-mode SceneView; Play-mode tilemap overlay 는 M3) ✅ M1
- [x] UI Toolkit 코드형 마이그레이션 (UXML 없이 — 기존 게임 UI 스타일과 일관) ✅ M1
- [x] `Assets/Editor/Painting/{PaintBrush,PaintSession,SceneViewPaintInput}.cs` ✅ M2
- [x] `Game.Gameplay.World.StageRuntime.SetStage(StageDef)` (Play-mode live preview) ✅ M2
- [ ] `Assets/Editor/Handles/MarkerHandles.cs` (SceneView 마커 핸들)
- [x] `Assets/Scripts/Loaders/StageLoader.cs` 에 SavePartial 추가 ✅ M2 — SaveExits / SaveEncounters / SaveGenerator / SaveProceduralClusters / SaveEnemySpawnZones 는 해당 phase 에서
- [ ] `Assets/Scripts/Data/StageDef.cs` `GeneratorDef` 확장 (thresholds/octaves/lacunarity/persistence)
- [ ] EditMode 테스트: SavePartial 라운드트립, 마커 좌표 정확도, regions rect 압축 결과 검증
- [ ] `mapTool_plan.md` (이 문서) → 진행에 따라 phase 완료 ✅ 마킹

---

## 9. 작업 순서 권장

0. **M0 ✅** — EditorWindow 스캐폴드. (완료)
1. **M1 ✅** — UI Toolkit 마이그레이션 + 탭 구조 + scratchpad/undo + 미리보기 인프라. (완료)
2. **M2 ✅** (1차) — rect/disc 페인팅 + Regions/Overrides 라우팅 + Region 인스펙터 + SceneView 페인팅 + SavePartial. (완료, line/freehand/eraser 는 후속 패스)
3. **M5.6** 또는 **M2 후속 패스 (eraser + line/freehand)** — 가장 즉시 유용한 다음 카드.
3. **M4** — exits/encounters/markers 가 데이터 측면에서 이미 wired up 되어 있어서 UI 만 붙이면 끝. SceneView 핸들로 Edit mode 에서도 가능 → workflow 가 크게 빨라진다.
4. **M3** — resource 도구는 만들면 좋지만 시급도 낮음.
5. **M5.1~M5.5** — 미리보기 친화 워크플로. 시드 튜닝 즐거움.
6. **M6** — QoL. 다른 다 끝나고.

각 phase 종료 시 `upgradeplan.md` 와 동일하게 한 줄 commit log 로 검증 결과 남기면 좋다.

---

## 10. 아키텍처 메모

```
┌──────────────────────────────────────────────────────────────┐
│  Editor Domain (Game.Editor.asmdef, Editor-only)             │
│                                                              │
│  StageEditorWindow ──────► StageScratchpad (JObject)         │
│         │                         │                          │
│         │ (Play mode)             │                          │
│         ▼                         ▼                          │
│  ┌─────────────────┐      ┌─────────────────┐                │
│  │ StageEditor     │      │ StageLoader     │                │
│  │ (runtime brain) │      │ Save* helpers   │                │
│  │ Game.Gameplay   │      │ Game.Loaders    │                │
│  └─────────────────┘      └─────────────────┘                │
│         │                         │                          │
└─────────┼─────────────────────────┼──────────────────────────┘
          │                         │
          ▼                         ▼
   Camera.main / Input        Disk JSON
   StageRuntime.NotifyChanged()
```

- **Editor asmdef** 는 `Assets/Editor/`. `includePlatforms: ["Editor"]`. references = Game.Data + Game.Gameplay + Game.Loaders.
- **EditorWindow** 는 UI + scratchpad + 디스크 IO 만 담당.
- **StageEditor 싱글톤** 은 Play mode brain — 마우스 입력, ghost sprite, runtime stage 업데이트. EditorWindow 가 메서드 호출로 제어.
- **Edit mode 작업** (메타데이터, generator 슬라이더, marker 핸들) 은 EditorWindow + SceneView 만으로 처리 — `StageEditor` 싱글톤 불필요.
- **빌드 산출물**: Game.Editor.dll 자체가 빠진다. StageEditor.cs 는 게임 assembly 에 남지만 `EnsureExists` 가 `#if UNITY_EDITOR` 라 Instance 가 null. 모든 consumer null-check 통과.
