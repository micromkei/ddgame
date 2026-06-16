# DefenceGame — Unity 이식 계획

`FactoryDefense_v2` (Godot 4.6 / GDScript) 의 게임 디자인·시스템을 **Unity 6000.4.10f1 (2D) / C#** 환경의 `DefenceGame` 프로젝트로 새로 만드는 마이그레이션 + 재설계 계획.

진실의 단일 소스: `C:\Users\microm\.ai\game.md` (Factory Defense GDD 요약).
참조 구현: `C:\Users\microm\.ai\FactoryDefense_v2\`.

---

## 0. 환경 / 전제

| 항목 | Godot 원본 | Unity 신규 |
|------|-----------|-----------|
| 엔진 | Godot 4.6 Forward+ | Unity 6000.4.10f1 (2D Feature 2.0.2) |
| 언어 | GDScript (정적 타이핑) | C# (nullable enable 권장) |
| 입력 | Input map | Input System 1.19 (`InputSystem_Actions.inputactions` 이미 존재) |
| 렌더 | 2D, 32px 그리드 | URP 2D, **32 PPU 그리드** 유지 |
| 데이터 | JSON in `res://data/` | JSON in `Assets/StreamingAssets/Data/` + ScriptableObject 캐시 |
| 오토로드 | `GameData`, `Session` | `MonoBehaviour Singleton` (`DontDestroyOnLoad`) |
| UI | Custom `Control._draw()` | **UI Toolkit (UITK)** 우선, 슬롯 grid는 UITK `VisualElement` + `generateVisualContent` |
| 직렬화 | JSON 단일 파일 | JSON 단일 파일 (Newtonsoft.Json 또는 `JsonUtility`) |
| 테스트 | 없음 | Unity Test Framework 1.6 (EditMode + PlayMode) |

> 폴더 컨벤션은 Unity 표준 (`Assets/`, `PascalCase` 폴더, asmdef 분할). Godot `scripts/gameplay/...` → Unity `Assets/Scripts/Gameplay/...`.

---

## 1. 최종 폴더 구조 (목표)

```
DefenceGame/
├── Assets/
│   ├── Art/
│   │   ├── Icons/items.png            (Factorio-style 64px atlas → Sprite Atlas)
│   │   ├── Tiles/                     (Tilemap 타일셋)
│   │   └── Sprites/                   (Building / Enemy / Projectile)
│   ├── Scenes/
│   │   ├── Lobby.unity
│   │   ├── Main.unity
│   │   └── Bootstrap.unity            (Persistent singletons init)
│   ├── Settings/
│   │   ├── URP 2D Renderer / Quality
│   │   └── InputSystem_Actions.inputactions  (이미 존재 — 액션맵 확장)
│   ├── Scripts/
│   │   ├── Data/                      (POCO Def 클래스 + enum, asmdef: Game.Data)
│   │   ├── Loaders/                   (JSON → Def, asmdef: Game.Loaders)
│   │   ├── Gameplay/
│   │   │   ├── Buildings/             (Building, CrafterBuilding, StorageBox, Turret)
│   │   │   ├── Enemies/
│   │   │   ├── Systems/               (BuildSystem, AutoTransferSystem, WaveSpawner,
│   │   │   │                           CraftQueue, SaveController, PollutionTracker)
│   │   │   ├── Player/                (Character, GameCamera)
│   │   │   ├── World/                 (ResourceNode, TerrainPainter)
│   │   │   └── Core/                  (GameData singleton, Session, RunSaveBridge)
│   │   ├── UI/                        (HUD, InventoryView, SlotGrid, Tooltip, Lobby, Pause)
│   │   └── Editor/                    (StageEditor 윈도우, Def 인스펙터)
│   ├── StreamingAssets/
│   │   └── Data/
│   │       ├── items/{minerals,naturals,intermediates,weapons}.json
│   │       ├── facilities/{mining,furnaces,assembly,power,pumps}.json
│   │       ├── storage/boxes.json
│   │       ├── recipes/{smelting,crafting,military}.json
│   │       ├── enemies/basic.json
│   │       └── stages/stage1_{1,2,3}.json
│   └── Tests/
│       ├── EditMode/                  (Def 로딩, ItemId 인코딩, 인벤토리 단위 테스트)
│       └── PlayMode/                  (Crafter cycle, auto-transfer, wave smoke test)
├── Packages/manifest.json             (URP 추가)
└── ProjectSettings/
```

asmdef 분할: `Game.Data` ← `Game.Loaders` ← `Game.Gameplay` ← `Game.UI`. Tests는 EditMode/PlayMode 별 asmdef.

---

## 2. 매핑: Godot → Unity

| Godot 개념 | Unity 대응 |
|-----------|-----------|
| `Node2D` Main | `Main.unity` 씬 + root `GameObject` |
| `CharacterBody2D` (`character.gd`) | `Rigidbody2D` (Kinematic) + `BoxCollider2D` + `PlayerController.cs` (`Move`/`Mine` actions) |
| `StaticBody2D` Building | `GameObject` + `BoxCollider2D` (정적) + `Building.cs` |
| `Camera2D` | `Camera` (Orthographic) + Cinemachine 또는 직접 추적 |
| `AStarGrid2D` addon | Unity 기본은 없음 — MVP는 그리드 점유 dict 만으로 처리. NavMesh 불필요. |
| `Tilemap` 노드 | `Grid` + `Tilemap` (com.unity.modules.tilemap, 이미 포함) |
| `autoload` `GameData` | `GameData : MonoBehaviour` 싱글톤, `Bootstrap` 씬에서 생성 후 `DontDestroyOnLoad` |
| `signal` | `event Action<T>` (정적 시그널) 또는 UnityEvent |
| `Resource` (`.tres`) | `ScriptableObject` (단, Def는 POCO + JSON 권장) |
| `Control._draw()` | UITK `VisualElement.generateVisualContent` (Painter2D) |
| `RefCounted Inventory` | POCO `Inventory` class + `event Action Changed` |
| `JSON.parse` | `JsonUtility` 또는 `Newtonsoft.Json` (Newtonsoft 권장: dict / nullable 지원) |
| `_process(delta)` | `Update()` / `FixedUpdate()` (물리) |
| `await get_tree().create_timer(t)` | `Coroutine` 또는 `Awaitable` (Unity 6) |

선택: **Newtonsoft.Json** 패키지 (`com.unity.nuget.newtonsoft-json`) 추가 — Def 역직렬화 편의.

---

## 3. 단계별 작업 계획

각 단계는 **목표 → 산출물 → 검증** 형식. 단계 종료 시 콘솔 에러 0 / 단위 테스트 통과를 요구.

### Phase 0 — 프로젝트 부트스트랩
- 목표: Unity 프로젝트가 빌드되고 `Main` 씬에서 빈 캐릭터가 움직임.
- 작업:
  1. `Packages/manifest.json` 에 URP + Newtonsoft.Json 추가
  2. URP 2D Renderer 에셋 / Quality 세팅
  3. `Bootstrap → Lobby → Main` 씬 흐름과 씬 로딩 헬퍼
  4. `Assets/Scripts/Gameplay/Player/PlayerController.cs` — WASD 이동 (`InputSystem_Actions` 의 `Move`)
  5. asmdef 4개 생성
- 검증: Play 모드에서 캐릭터가 화면 위에서 WASD로 움직임. 콘솔 클린.

### Phase 1 — 데이터 레이어 (Defs + Loaders)
- 목표: `StreamingAssets/Data/` 의 모든 JSON을 부팅 시 로드 + 무결성 검증 통과.
- 작업:
  1. `FactoryDefense_v2/data/**/*.json` 을 `Assets/StreamingAssets/Data/` 로 복사 (변경 없이)
  2. POCO Def 클래스 (Godot `scripts/data/*.gd` 미러)
     - `ItemDef`, `FacilityDef : ItemDef`, `StorageDef : ItemDef`, `RecipeDef`, `IngredientDef`, `EnergyDef`, `EnemyDef`, `StageDef`
     - enum: `ItemCategory`, `FacilitySubCategory`, `EnergyType`
     - `ItemId` 유틸: `Encode(category, sub, index) → ulong`, `DecodeCategory(id)`, …
       (Godot의 `[WORD][WORD][DWORD]` 인코딩 그대로 — `0x0001…` Mineral … `0x000A…` Module)
  3. Loaders (`Loaders/`):
     - `JsonLoader` (Newtonsoft 래핑, StreamingAssets 경로 처리)
     - `ItemLoader`, `FacilityLoader`, `StorageLoader`, `RecipeLoader`, `EnemyLoader`, `StageLoader`
     - `GameDataLoader.LoadAll()` — 카테고리별 로드 후 `Validate()` (ingredient/result/produced_in 참조 무결성)
  4. `GameData` 싱글톤 — `Item(id)`, `ItemByName(name)`, `Recipe(id)`, `RecipesProducing(itemId)` 조회 API
  5. EditMode 테스트: 각 카테고리별 로드 + Validate 통과
- 검증: Bootstrap 씬에서 `LoadAll()` 호출 시 로그 "Loaded N items, M recipes…" 출력, Validate 에러 없음.

### Phase 2 — 인벤토리 / 아이콘
- 목표: 40슬롯 인벤토리 + 핫바 8슬롯 + 자원 자동 백팩 배치 + 아이콘 그리드 표시.
- 작업:
  1. `Inventory.cs` POCO — `slots[40]`, `Add/Remove/CanAccept`, 자원 카테고리 `PlayerStartSlot() = 8` (Godot 패턴 동일)
  2. `event Action Changed` — UI redraw 트리거
  3. `ItemIcons.cs` — `Vector2Int (col,row)` 테이블 + 64px 아틀라스(`Assets/Art/Icons/items.png`) Sprite Atlas
  4. `ItemColors.cs` — 매핑 없는 아이템 단색 fallback (이름 해시 → HSV)
  5. UITK 초기 `InventoryView.uxml/uss` — 핫바만 (8슬롯 가로)
- 검증: 코드로 `inv.Add(woodId, 10)` 호출 시 핫바가 아닌 백팩 슬롯에 표시. Wood 아이콘 또는 단색 사각형.

### Phase 3 — 월드 / 자원 노드
- 목표: 자원 노드 클릭으로 채굴, 인벤토리 누적.
- 작업:
  1. `World.unity` Tilemap (지형) — `terrain_painter.gd` 의 룰을 C# 으로 포팅 (또는 정적 타일맵)
  2. `ResourceNode.cs` — `itemId`, `remaining`, OnMouseDown 또는 `IPointerClickHandler` 로 채굴
  3. `Character.Mine()` — `REACH = 90`, `MINE_INTERVAL = 0.35s` (game.md §6)
  4. 채굴 시 인벤토리 add + 노드 hp 감소, 0이면 destroy
- 검증: 우드 노드 5번 클릭 → 인벤토리 Wood 5개. 거리 초과 시 무시.

### Phase 4 — 빌딩 베이스
- 목표: 핫바에서 시설 선택 → 마우스 위치 고스트 → 클릭 배치 → 시공 타이머 → 가동.
- 작업:
  1. `Building.cs` — 공통 (hp, 그리드 점유, `ConstructionTime`, `Deconstruct(intoInventory)`)
  2. `CrafterBuilding : Building` — Mining/Furnace/Assembly 통합 (`subCategory` 분기)
     - `selectedRecipe`, `CycleRecipe()`, input/output `Inventory`
     - `Update()` 에서 `speedPerSec` 누적 → `recipe.time` 도달 시 결과 산출 + 잉여 input 소비
     - `IsMiningDrill()` → 노드 위에 있어야 활성
  3. `StorageBox : Building` — 단순 `Inventory` 보유
  4. `Turret : Building` — 적 탐색 → `Projectile` 발사 (Phase 7 까지 stub)
  5. `BuildSystem.cs` — 고스트 표시, 그리드 스냅(32px), 충돌 체크, 배치 시 `Instantiate(prefab)`
- 검증: 핫바에서 StoneFurnace 선택 → 월드 클릭 → 시공 오버레이 → 시공 완료 후 input에 IronOre 넣으면 IronPlate 생산.

### Phase 5 — 자동 운반
- 목표: 플레이어 `INTERACT_RANGE = 80px` 안의 모든 시설에 input 공급 / output 회수.
- 작업:
  1. `AutoTransferSystem.cs` — 0.3s 주기, output→인벤(최대 10/회), input←인벤(`CanAccept` 만, 슬롯 cap 50)
  2. 시설 리스트는 `Building` 등록 이벤트로 자동 구독
- 검증: 시설 근처 머무르면 핫바에 결과물 누적, 떨어지면 정지.

### Phase 6 — 수동 제작 큐
- 목표: 우측 3×3 그리드에서 `HandCrafter.ALLOWED` 9개 레시피 → 큐 (최대 20).
- 작업:
  1. `HandCrafter.cs` (화이트리스트 9개)
  2. `CraftQueue.cs` — 진행 큐 (max 20), 완료 시 인벤토리 add
  3. UITK 좌하단 진행 바 + 우측 3×3 그리드
- 검증: Wood 2 + IronGearWheel 4 → IronStick 만들면 30초 후 인벤에 IronStick 1개.

### Phase 7 — 적 / 웨이브 / 오염
- 목표: 가동 시설의 오염 누적 → 임계치 + 25s 간격 충족 시 적 스폰 → 터렛이 격추.
- 작업:
  1. `EnemyDef` 데이터화 (Godot 의 `enemy.gd` 상수 → `Assets/StreamingAssets/Data/enemies/basic.json` 이미 존재)
  2. `Enemy.cs` — Rigidbody2D, 플레이어 우선 / 가까운 빌딩 공격
  3. `PollutionTracker.cs` — 매 프레임 활성 시설의 `pollutionPerSec` 누적
  4. `WaveSpawner.cs` — `POLLUTION_PER_WAVE = 6.0`, `WAVE_MIN_INTERVAL = 25s`, `SPAWN_RADIUS = 700`
  5. `Turret.Fire()` + `Projectile.cs` — 즉시 데미지 (MVP, 탄도/탄약은 v2)
  6. **첫 Turret 건설 후에만 웨이브 시작** (game.md §9.3)
- 검증: Furnace 가동 → 25s + 오염 6.0 누적 후 웨이브 1마리 스폰, 터렛이 격추.

### Phase 8 — UI 풀세트
- 목표: 게임 진행에 필요한 모든 패널 동작.
- 작업:
  1. `HUD` — 좌상단 HP/Wave/Pollution, 우측 hover 정보, 좌하단 craft queue, 중앙 하단 핫바
  2. `InventoryView` (I 키 토글) — 백팩 + 3×3 craft grid + 드래그&드롭
     - `_heldItemId/_heldCount/_heldOrigin` 상태 (Godot 동일)
     - 좌클릭 = 전체 집기/놓기/스택/스왑, 우클릭 = 절반 집기 / 한 개씩 놓기
     - ESC = `ReturnHeld()` 원위치 우선, 아니면 add
  3. `HeldCursorOverlay` — `pickingMode = Ignore`
  4. `SlotGrid` — Storage / Crafter input/output 공통 (accept filter Func)
  5. `ItemTooltip` — hover 시 표시, 자원은 스킵 (`ItemCategory.IsResource`)
  6. `BuildingPropertiesView` — Crafter 선택 시 레시피 변경 + input/output 슬롯 + Deconstruct 버튼
  7. `Lobby` (New Game / Continue / Quit), `PauseMenu` (Esc, 게임 일시정지)
- 검증: 드래그&드롭으로 슬롯 간 이동, 스택 머지, 스왑 모두 동작. Crafter 레시피 변경 가능.

### Phase 9 — 세이브 / 로드
- 목표: 창 닫기 또는 Pause → "Save & Quit" 시 단일 JSON 저장, 로비 "Continue" 로 복원.
- 작업:
  1. `SaveController.cs` — 직렬화 대상: player(pos, hp), inventory, wave state, craft queue, buildings(pos/hp/progress/input/output/recipe), resources
  2. `version` 필드 + 마이그레이션 hook
  3. `Session.PendingSave` 패턴 (Godot 동일) — 로비에서 set, Main 씬 OnEnable 에서 `ApplySave()`
  4. 저장 경로: `Application.persistentDataPath/save.json`
- 검증: 자원 채집 → 건물 설치 → Save & Quit → Lobby Continue → 동일 상태 복원.

### Phase 10 — 스테이지 에디터
- 목표: F1 토글, 1~5 키 자원 선택, LMB 배치 / RMB 제거 / Ctrl+S 저장.
- 작업:
  1. `StageEditor.cs` — 런타임 UITK 팔레트 + 입력 처리
  2. 저장: `StreamingAssets/Data/stages/*.json` 에 직렬화 (개발 빌드만 — Editor 가드)
  3. (선택) `Assets/Scripts/Editor/StageEditorWindow.cs` — Unity Editor 윈도우 버전
- 검증: F1 → 1 → 클릭 5회 → IronOre 노드 5개 배치 → Ctrl+S → JSON 갱신.

### Phase 11 — 폴리시 / 테스트
- 목표: 안정화 + 기본 테스트.
- 작업:
  1. EditMode 테스트:
     - `ItemId` 인코딩 라운드트립
     - 모든 카테고리 JSON Validate
     - `Inventory.Add/Remove/CanAccept` 경계 케이스
  2. PlayMode 테스트:
     - Crafter 가동 1 cycle 완료
     - AutoTransfer in/out 동작
     - Wave 1회 스폰 → 적 1마리 사망
  3. 빌드 (StandaloneWindows64) — Lobby → Main → 종료 까지 무에러
- 검증: 모든 테스트 그린, 빌드 실행 가능.

---

## 4. game.md 의 §7.2 (재설계/강화) 반영

원본 GDD가 "신규 프로젝트에서 강화" 로 표시한 항목 — Unity 이식 단계에서 처리할지 v2 로 미룰지 결정.

| 항목 | 이번 단계에서 | 이유 |
|------|------|------|
| 적 데이터화 (`enemies/*.json` + `EnemyDef`) | ✅ Phase 7 포함 | 이미 JSON/`enemies/basic.json` 존재. C# 포팅 시 비용 적음. |
| `main.gd` → `BuildingManager` / `AutoTransferSystem` / `SaveController` 분리 | ✅ 처음부터 분리 | Unity 는 컴포넌트 지향이라 자연스러움 (Phase 4/5/9) |
| HUD 패널별 컴포넌트 분리 | ✅ Phase 8 (UITK 자체가 분리 유도) | |
| 단위 테스트 (GUT → UTF) | ✅ Phase 1·11 | Unity Test Framework 이미 포함 |
| 벨트 / 인서터 (transport `0x0007`) | ❌ v2 | MVP 범위 초과 — `ItemId` prefix 만 예약 유지 |
| 전력 그리드 (`EnergyDef` 실구현) | ❌ v2 | `EnergyDef` POCO 만 포팅, 동작은 Burner 가정 |
| 터렛 탄도 / 탄약 | ❌ v2 | Projectile 즉시 데미지 MVP |
| 사이언스 팩 / 연구 트리 (`0x0009`) | ❌ v2 | prefix 예약만 |
| 모듈 시스템 (`0x000A`) | ❌ v2 | prefix 예약만 |
| 유체(Fluid) `0x0003` | ❌ v2 | prefix 예약만 |

---

## 5. 코딩 컨벤션 (게임 game.md §7.3 의 C# 버전)

- 파일당 ≤ 500 줄
- `class` + 명시적 접근 제어자, `nullable enable`
- 아이템 룩업은 항상 `ulong` id, `string` name 은 표시·디버그
- 시그널/이벤트 명명: `Changed`, `Requested`, `Finished` (C# 관례 PascalCase event)
- 입력은 Input System 액션맵 — 직접 `Input.GetKey` 금지
- 매직 넘버 금지 — 데이터는 JSON, 상수는 `static readonly` 최상단
- 그리드 단위: `const float Tile = 32f;` 한 곳에서 관리
- `MonoBehaviour` 는 얇게, 게임 로직은 POCO + 시스템 클래스

---

## 6. 마일스톤 / 끝나는 조건

- **M1 (Phase 0–2 종료)**: 캐릭터가 움직이고 인벤토리에 아이콘이 보인다. 모든 JSON 로드 검증 통과.
- **M2 (Phase 3–6 종료)**: 채집 → 시설 배치 → 자동 운반 → 수동 제작 까지 한 사이클이 동작한다.
- **M3 (Phase 7–9 종료)**: 웨이브가 오고 터렛이 막고 저장/로드가 된다. → **플레이어블 빌드**
- **M4 (Phase 10–11 종료)**: 스테이지 에디터 + 테스트 그린 + 빌드 실행 OK.

---

## 7. 즉시 다음 액션 (Phase 0 착수 시)

1. `Packages/manifest.json` 에 추가:
   - `"com.unity.render-pipelines.universal": "17.x"`
   - `"com.unity.nuget.newtonsoft-json": "3.2.x"`
2. URP 2D Renderer + Quality 에셋 생성 → `Project Settings → Graphics`
3. `Assets/Scenes/Bootstrap.unity`, `Lobby.unity`, `Main.unity` 생성 + Build Settings 등록
4. asmdef 4개 (`Game.Data` → `Game.Loaders` → `Game.Gameplay` → `Game.UI`) 생성
5. `Assets/Scripts/Gameplay/Player/PlayerController.cs` — InputSystem `Move` 액션 수신, `Rigidbody2D` 이동

여기까지 통과하면 Phase 1 (데이터 레이어) 로 진입.
