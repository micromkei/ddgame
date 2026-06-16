# Upgrade Plan — Stage Generation, Resource Placement, Pokémon-Inspired Direction

작성일: 2026-06-15
대상 프로젝트: `DefenceGame/` (Unity)
범위: 스테이지 데이터 → 타일맵 → 자원 배치 → 자동 맵 생성 → 포켓몬 스타일 확장

---

## 0. TL;DR

현재 코드는 "팩토리 디펜스" MVP에 머물러 있고, 스테이지 JSON에 풍부한 디자인 정보(terrain regions, resources.procedural, waves, objective)가 들어 있는데도 런타임이 그 데이터를 거의 무시한다. 본 계획서는 그 누락을 메우는 동시에, **시드 기반 맵 자동 생성** + **포켓몬식 "루트/바이옴/조우/포획" 구조**를 점진적으로 얹는다. 다섯 단계 (A→E) 로 나눠 각 단계가 독립적으로 끝낼 수 있는 verifiable goal 을 갖도록 설계했다.

핵심 키워드: **biome-driven generation, Poisson disc clusters, route topology, tame & deploy, codex**.

---

## 1. 현재 상태 스냅샷

| 영역 | 상태 |
|---|---|
| `StageDef` | id/name/biome/difficulty/seed 만 타입 매핑. terrain·resources·waves·objective 는 `JExtensionData` 의 `Raw` 딕셔너리에 그대로 방치. |
| `StageLoader` | 읽기만 가능. `SaveAuthored` 로 `resources.authored` 만 다시 쓴다. terrain/waves/procedural 미사용. |
| `World.cs` | 하드코딩된 5개 자원 스폰. 스테이지 JSON 무시. `Tile = 1f` 상수. |
| `GameScene/Grid` | `m_CellSize = 0.5` — `World.Tile = 1f` 와 **불일치**. |
| `GameScene/Tilemap` | 손으로 칠해 둔 테스트 타일이 박혀 있고, 런타임이 다시 그려주지 않는다. |
| `StageEditor` | F1 토글, 자원 노드만 페인트 (terrain 미지원). 1-5 팔레트는 Wood/Stone/IronOre/CopperOre/Coal 고정. |
| `HudView` | HP/Wave/Pollution/Hotbar/Queue 만 표시. 스테이지 이름·목표·바이옴 표시 없음. |
| `WaveSpawner` | 오염 임계치 기반. `stage.waves[].groups` 정의 무시 (그루트/러너/브루트 3종만 사용). |
| `enemies/basic.json` | 3종 (Grunt, Runner, Brute). 타입/저항 개념 없음. |

요약: **데이터 스키마는 야심차게 잡혀 있지만, 실행 경로는 5% 정도만 살아 있다.**

---

## 2. 디자인 비전 — Factory Defense × Pokémon

원작은 유지하되, **"맵을 가로지르는 탐험과 발견"** 의 감각을 포켓몬에서 따 온다. 다음 4개 디자인 기둥을 새로 세운다.

1. **루트 토폴로지 (Route Topology)**
   스테이지를 단일 격투장이 아니라 *루트*로 구성: 출발점 → 자원 노드들이 흩어진 야생 구간 (tall grass) → 보스 둥지 → 다음 스테이지로 가는 길목.
2. **바이옴 차별화 (Biome Differentiation)**
   숲/해안/사막/화산/폐허 등 바이옴마다 다른 타일셋, 다른 자원 분포, 다른 적 풀, 다른 오염 반응을 갖는다.
3. **조우 → 포획 → 배치 (Encounter → Tame → Deploy)**
   적을 무력화한 뒤 일정 확률로 *포획*하여 *우호 NPC* 로 만들 수 있다. 포획체는 터렛 대용·자원 채집 보조·전투 보조로 배치 가능. 포켓몬의 "잡고 키우고 같이 싸운다" 루프.
4. **코덱스 (Codex / Pokédex)**
   본 게임의 진척도는 코덱스(발견한 자원·바이옴·생물·레시피) 채우기로 가시화. 메인 진행 외의 collect-em-all 보상 동기.

이 4개 기둥이 본 계획서 모든 단계의 의사결정 기준.

---

## 3. 단계별 업데이트 계획

### Phase A — Foundation Fixes (1~2 일)

**Goal**: 현재 깨진 연결을 복구하여, 스테이지 JSON 이 화면에 실제로 반영되도록 만든다.

| # | 작업 | Verify |
|---|---|---|
| A1 | `World.Tile` 와 `Grid.CellSize` 통일 (제안: 둘 다 `1.0`) | 자원 노드 위치가 Tilemap 셀 중심에 스냅된다 |
| A2 | `StageDef` 확장: `TerrainDef`, `RegionDef`, `ResourcesDef`, `ProceduralClusterDef`, `AuthoredResourceDef`, `WaveDef`, `WaveGroupDef`, `EnemySpawnZoneDef`, `ObjectiveDef`, `RewardDef` 타입화. `Raw` 의존 제거. | stage1_1.json 역직렬화 후 모든 필드 접근 가능 |
| A3 | `StageRuntime` (신규) 컴포넌트: `Awake` 에서 `Session.StageId` 로 스테이지 로드 → `StageDef` 를 다른 시스템(World, WaveSpawner, HudView)에 broadcast. | 콘솔에 `[StageRuntime] loaded stage1_1: First Encounter` 로그 |
| A4 | `TerrainPainter` (신규): `StageDef.Terrain.Regions` 를 순회하며 `Tilemap.SetTile` 호출. 사각형/사각테/원/링/디스크 shape 처리. | stage1_1 의 water_border/beach/paths/crossroads 가 화면에 그려진다 |
| A5 | `BiomeTilePalette` ScriptableObject: biome 키 → (TileBase default, dictionary<purpose, TileBase>). Resources/Tilemaps/ 에 grassland/wasteland 인스턴스 생성. | `terrain.default_tile` 인덱스가 팔레트 미스이면 핫핑크로 폴백 |
| A6 | `World.cs` 의 하드코딩 spawn list 제거 → `StageDef.Resources.Authored` 와 `Procedural.Clusters` 를 받아 노드를 인스턴스화. | stage1_1 의 5개 클러스터 (Wood×8, Stone×4, IronOre×3, CopperOre×3, Coal×2) 가 맵에 분포 |
| A7 | `HudView` 상단에 `StageInfoPanel` 추가: 스테이지 이름, 짧은 설명, 현재 목표 (예: "Survive 3 waves — 1/3"), 바이옴 아이콘. | 게임 시작 시 좌상단에 "Stage 1-1 First Encounter / Grassland / Wave 1/3" 가 보인다 |

`Validate()` 실패 케이스 (스테이지 누락 / 알 수 없는 biome / 알 수 없는 enemy_id) 는 콘솔 에러 + 폴백.

---

### Phase B — Procedural Map Generation (2~3 일)

**Goal**: 스테이지 JSON 이 "씨드 + 규칙" 만으로도 풍부한 맵을 만들어내도록 한다. 손으로 모든 region 을 적지 않아도 된다.

| # | 작업 | Verify |
|---|---|---|
| B1 | `BiomeDef` JSON 신설 (`data/biomes/grassland.json` 등): tile palette, noise params (frequency/amplitude), 자원 가중치, 적 풀, BGM 키. | grassland/wasteland/forest/coast/volcanic 5종 정의 |
| B2 | `WorldGenerator` 클래스: `(StageDef, BiomeDef, seed) → TerrainGrid`. Perlin/Simplex noise 로 고도맵 만들고, threshold 로 water/beach/land/cliff 자동 분류. | 같은 seed 면 동일한 결과 (deterministic) |
| B3 | `MultiBiomeStage`: 한 스테이지가 여러 바이옴을 가질 수 있도록 — Voronoi cell 로 영역 분할, 경계에 자연 전환 (beach ↔ grass) 적용. | stage2_1 같은 멀티 바이옴 데모 스테이지 시연 |
| B4 | Path generation: spawn_point ↔ core_point ↔ exits 를 A* 로 잇고, 경로 폭만큼 tile.purpose=path 로 덮어쓴다. | 모든 스테이지에서 코어 진입로가 항상 보장된다 |
| B5 | Override 우선순위 정리: `terrain.overrides` (수동) > `regions` (디자이너 지정) > `WorldGenerator` (자동). | 디자이너가 손으로 칠한 영역은 절대 자동 생성에 덮이지 않는다 |
| B6 | `StageEditor` 에 **Terrain mode** 추가 (F2 토글): 1-9 키로 region purpose 선택, drag 로 사각형 페인트. 저장 시 `terrain.regions` 에 신규 엔트리 추가. | F2 모드에서 그린 region 이 JSON 재시작 후에도 살아 있다 |

WorldGenerator 는 헤드리스 테스트 가능하도록 MonoBehaviour 가 아닌 순수 C# 클래스로.

---

### Phase C — Smarter Resource Placement (1~2 일)

**Goal**: 자원이 "균일 그리드" 가 아니라 **자연스럽게 군집** 하면서 바이옴 룰을 따르도록 만든다.

| # | 작업 | Verify |
|---|---|---|
| C1 | Poisson-disc sampling 으로 클러스터 *센터* 배치 — 클러스터끼리 최소 거리 보장. | 같은 자원 클러스터가 서로 겹치지 않는다 |
| C2 | 각 클러스터는 *블롭* 모양 (Worley/cellular noise) — 정사각형이 아닌 자연스러운 모양. | 자원 노드 분포가 우물 단지처럼 보인다 |
| C3 | 클러스터 중심에 **rich center node** (높은 amount, 다른 색), 가장자리는 amount 감소. | 노드 호버 시 amount 가 중심 > 가장자리 |
| C4 | **Biome-weighted resource table**: forest 는 Wood/Mushroom, coast 는 Fish1/Fish2/Salt, volcanic 은 Lava/Carbon/Sulfur. `BiomeDef.resource_weights` 를 따른다. | wasteland 스테이지에 Wood 가 거의 없다 |
| C5 | **Hidden caches** (포켓몬의 보이지 않는 아이템): 5% 확률로 일반 자원 노드가 희귀 아이템 노드로 치환. 처음 채굴 시 코덱스에 등록. | 채굴 후 codex 항목이 늘어난다 |
| C6 | **Resource respawn 옵션**: `BiomeDef.respawn_seconds` 가 있으면 채굴 N초 뒤 같은 위치에 다시 생성. 포켓몬의 풀숲 재조우 감각. | grassland 의 Wood 가 60s 뒤 재생성 |

C1~C3 은 `WorldGenerator` 와 같은 시드를 공유 → 같은 스테이지는 항상 같은 자원 레이아웃.

---

### Phase D — Pokémon-Inspired Features (3~5 일)

**Goal**: 디자인 비전의 4개 기둥을 게임에 실제로 구현한다.

#### D1. Route Topology

- `StageDef.exits[]` 추가: `{direction, target_stage_id, gate_tile}`.
- 코어를 클리어하면 exit gate 가 열린다. 그 타일에 진입하면 다음 스테이지로 전환.
- 스테이지를 노드, exit 를 엣지로 보면 **루트 그래프** — 분기/순환 가능.
- 첫 구현은 선형 트리(1-1 → 1-2 → 1-3) → 향후 분기 추가.

#### D2. 적 타입 시스템

`enemies/basic.json` 에 `types: ["normal" | "flying" | "ground" | "fire" | "electric" | ...]` 추가.
무기에 `damage_types: { kinetic, fire, electric, ... }` 추가.
`TypeChart` 정적 테이블 (Pokémon 의 type chart 흉내) — 1.0/2.0/0.5/0.0 배수.
**터렛 선택의 깊이가 단번에 증가**: "이 바이옴엔 flying 적이 많으니 electric 터렛 위주".

#### D3. 조우 / 포획 / 팀

| 요소 | 메커니즘 |
|---|---|
| Encounter | "tall grass" region (terrain.purpose) 안에서 일정 확률로 야생 적 등장. 웨이브와 별개. |
| Capture | 적의 HP 가 임계치 이하면 `CaptureDevice` 아이템으로 포획 시도. 확률 = `f(remaining_hp, device_tier, enemy_rarity)`. |
| Team Slot | UI 에 별도 "Companions" 슬롯 (최대 6). 각 컴패니언은 player 가까이서 자동 follow + 적 공격. |
| Leveling | 컴패니언이 적을 잡으면 XP. 레벨업 시 stat 증가, 일정 레벨에서 evolution. |
| Deploy | 컴패니언을 특정 타일에 정착시키면 *영역 방어 모드* (터렛 대용). 회수 가능. |

이 기능은 **별도 컴포넌트** (`CompanionSystem`, `EncounterSystem`, `CaptureDevice`) 로 분리해 기존 빌드/팩토리 시스템과 충돌하지 않도록.

#### D4. Codex

- 카테고리: Items / Enemies / Biomes / Recipes / Companions.
- 첫 발견 시 토스트: "New Codex entry: HolmiumOre".
- Codex 키 = "발견 진척도". 일정 % 도달 시 보상 (인벤토리 슬롯 확장, 신규 레시피 잠금 해제 등).
- UI: `CodexView` 키 단축키 (K).

#### D5. Town/Hub Stage (선택)

- 전투 없는 안전 스테이지. NPC 서너 명: Recipe Trader, Companion Healer, Stage Selector.
- 새 스테이지 진입 전 항상 거치는 *Pokémon Center* 컨셉.
- 후순위 — 다른 기능이 우선.

---

### Phase E — Polish & Quality of Life (1~2 일)

| # | 작업 |
|---|---|
| E1 | `RuleTile` 도입으로 grass↔water 자동 이음매 (현재 평평한 사각형 → 자연스러운 해안선) |
| E2 | 카메라: 부드러운 follow + 새 바이옴 진입 시 잠깐 zoom-out |
| E3 | 바이옴별 BGM 전환, 채굴/포획/레벨업 SFX |
| E4 | Day/Night cycle (간단한 ambient 톤 변화) + 야간 적 스폰 +20% |
| E5 | 세이브 포맷에 companions / codex / route_progress 추가 (`SaveData` 확장) |
| E6 | `Validate()` 에 biome ↔ enemy ↔ resource 교차 검증 추가 |

---

## 4. 데이터 스키마 (요약)

```jsonc
// stages/stage1_1.json (확장 후)
{
  "id": "stage1_1",
  "name": "First Encounter",
  "biome": "grassland",
  "biome_mix": [                  // 선택 — 멀티바이옴
    { "biome": "grassland", "weight": 0.7 },
    { "biome": "coast",     "weight": 0.3 }
  ],
  "seed": 12345,
  "generator": {                  // 신규 — Phase B
    "mode": "auto",               // "auto" | "manual" | "hybrid"
    "noise_scale": 0.05,
    "water_level": 0.3
  },
  "terrain": { /* 기존 regions + overrides 유지 */ },
  "resources": { /* 기존 procedural/authored 유지 + biome_weights override */ },
  "exits": [                       // 신규 — Phase D1
    { "direction": "east", "target": "stage1_2", "gate_tile": [49, 0] }
  ],
  "encounters": {                  // 신규 — Phase D3
    "regions": ["tall_grass"],
    "chance_per_step": 0.02,
    "pool": ["grunt", "runner"]
  },
  "waves": [ /* 기존 */ ],
  "objective": { /* 기존 */ },
  "reward": { /* 기존 */ }
}
```

```jsonc
// biomes/grassland.json (신규)
{
  "id": "grassland",
  "name": "Grassland",
  "palette": "Tilemaps/GrasslandPalette",
  "noise": { "scale": 0.05, "octaves": 4, "lacunarity": 2.0 },
  "thresholds": { "water": 0.2, "beach": 0.3, "land": 1.0 },
  "resource_weights": {
    "Wood": 0.45, "Stone": 0.2, "IronOre": 0.15, "Coal": 0.1, "CopperOre": 0.1
  },
  "enemy_pool": ["grunt", "runner"],
  "bgm": "Audio/BGM/grassland",
  "respawn_seconds": 60
}
```

---

## 5. 추정 일정 / 의존성

```
Phase A  ─┬──> Phase B ──┬──> Phase C
          │              │
          └──────────────┴──> Phase D ──> Phase E
```

A 완료 후 B/C/D 는 부분 병렬 가능. D 는 A 의 데이터 모델에만 의존.
실측 일정은 1인 개발 기준 약 **8~14일**.

---

## 6. 비목표 (Out of Scope)

다음은 본 계획에서 **명시적으로 제외** — 풀려고 들면 무한히 커진다.

- 멀티플레이/넷코드
- 완전한 RTS-급 적 AI (현 추격형 유지)
- 풀 3D / 등각투상
- 모드 SDK 공개
- 모바일 컨트롤 (PC 키마우 우선)

---

## 7. 시작 후보 (다음 한 시간)

가장 먼저 실행하면 가장 큰 가치를 주는 작업 3개:

1. **A1** (Grid CellSize ↔ World.Tile 정합) — 5분, 즉각적으로 화면 정렬이 맞는다.
2. **A3 + A4** (StageRuntime + TerrainPainter 골격) — stage1_1 이 처음으로 디자인된 모양으로 보인다.
3. **A7** (HudView 에 StageInfoPanel) — 디버깅과 데모가 훨씬 쉬워진다.

이 3개만 끝내도 *"스테이지 정보가 제대로 작동" * 한다는 최초 마일스톤 달성.
