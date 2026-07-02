# Box2D for Unity — API 레퍼런스

이 문서는 `box2d-unity` 패키지(네임스페이스 `Box2D`)가 제공하는 모든 공개 타입과 멤버를 정리한 레퍼런스입니다.
다른 Unity 프로젝트에서 이 라이브러리를 재사용할 때 소스를 다시 읽지 않고 이 문서만으로 API를 찾아 쓸 수 있도록
작성했습니다. 설치 방법과 빌드 방법은 `README.md`를 참고하세요.

## 0. 전제 조건 및 규칙

- **단정밀도(single precision) 전용.** `BOX2D_DOUBLE_PRECISION`으로 빌드된 네이티브 라이브러리는 지원하지
  않습니다. `B2World.Create()` 호출 시 `b2IsDoublePrecision()`을 확인해 다르면 예외를 던집니다.
- 좌표/변환 관련 타입은 실제 C API의 `b2Pos`/`b2WorldTransform`과 `b2Vec2`/`b2Transform`이 단정밀도에서
  동일한 메모리 구조이므로, 이 바인딩에서는 `B2Vec2`/`B2Transform`으로 통일되어 있습니다.
- 모든 `Create(...)` 계열 정적 메서드는 실제로 바디/조인트/월드를 생성하고, 리턴값은 해당 객체를 가리키는
  가벼운 값 타입(핸들)입니다. `B2World`만 클래스이고 나머지(`B2Body`, `B2Shape`, `B2Chain`, `B2Contact`,
  `B2Joint`류)는 `readonly struct`라서 자유롭게 복사해서 들고 다녀도 됩니다.
- `XxxDef` 구조체는 항상 `XxxDef.Default()`로 생성한 뒤 필요한 필드만 바꿔서 사용하세요. Box2D 내부적으로
  기본값 검증용 `internalValue` 필드를 사용하므로 직접 `new B2XxxDef()`로 만들면 안 됩니다.
- `EnableXxx`/`IsXxx` 같은 이름은 C#의 `bool` 프로퍼티(값 변환 포함)이고, `enableXxx`처럼 소문자로 시작하는
  이름은 C API와 동일한 이름의 `float`/`int`/구조체 **필드**입니다(대소문자를 헷갈리지 않도록 주의).
- 각 랩퍼 구조체는 대응하는 원시 ID(`B2BodyId`, `B2ShapeId`, `B2JointId`, `B2ChainId`, `B2ContactId`,
  `B2WorldId`)로의 암시적 변환을 제공합니다.

---

## 1. B2World (`World.cs`)

시뮬레이션 월드. `sealed class`이며, 콜백 델리게이트를 필드로 들고 있어야 하므로 클래스로 구현되어 있습니다.

| 멤버 | 설명 |
|---|---|
| `static B2World Create(B2WorldDef def)` | 월드 생성. 네이티브 라이브러리가 double precision이면 예외 발생 |
| `static B2World Create()` | `B2WorldDef.Default()`로 월드 생성 |
| `B2WorldId Id { get; }` | 원시 핸들 |
| `void Destroy()` | 월드 파괴 |
| `bool IsValid` | 핸들 유효성 |
| `void Step(float timeStep, int subStepCount = 4)` | 시뮬레이션 한 스텝 진행 |
| `void Draw(B2DebugDraw draw)` | 디버그 드로우 콜백 실행 (§9 참고) |
| `B2AABB Bounds` | 모든 셰이프를 포함하는 월드 바운즈 |
| `B2BodyMoveEvent[] GetBodyEvents()` | 이번 스텝에 이동한 바디 이벤트 배열 |
| `void GetSensorEvents(out B2SensorBeginTouchEvent[] begin, out B2SensorEndTouchEvent[] end)` | 센서 시작/종료 이벤트 |
| `void GetContactEvents(out B2ContactBeginTouchEvent[] begin, out B2ContactEndTouchEvent[] end, out B2ContactHitEvent[] hit)` | 접촉 시작/종료/충돌(hit) 이벤트 |
| `B2JointEvent[] GetJointEvents()` | 임계값을 초과한 조인트 이벤트 |
| `B2TreeStats OverlapAABB(B2Vec2 origin, B2AABB aabb, B2QueryFilter filter, B2OverlapResultFcn callback)` | AABB 오버랩 쿼리 |
| `B2TreeStats OverlapShape(B2Vec2 origin, B2ShapeProxy proxy, B2QueryFilter filter, B2OverlapResultFcn callback)` | 임의 셰이프 오버랩 쿼리 |
| `B2TreeStats CastRay(B2Vec2 origin, B2Vec2 translation, B2QueryFilter filter, B2CastResultFcn callback)` | 레이캐스트 (모든 히트 콜백) |
| `B2RayResult CastRayClosest(B2Vec2 origin, B2Vec2 translation, B2QueryFilter filter)` | 가장 가까운 히트만 반환하는 간단한 레이캐스트 |
| `B2TreeStats CastShape(B2Vec2 origin, B2ShapeProxy proxy, B2Vec2 translation, B2QueryFilter filter, B2CastResultFcn callback)` | 셰이프 캐스트 |
| `bool EnableSleeping { get; set; }` | 슬립 허용 여부 |
| `bool EnableContinuous { get; set; }` | 연속 충돌 감지(CCD) 허용 여부 |
| `float RestitutionThreshold { get; set; }` | 반발(bounce) 적용 속도 임계값 |
| `float HitEventThreshold { get; set; }` | hit 이벤트 발생 속도 임계값 |
| `void SetCustomFilterCallback(B2CustomFilterFcn callback)` | 커스텀 충돌 필터 콜백 등록(참조 유지됨) |
| `void SetPreSolveCallback(B2PreSolveFcn callback)` | pre-solve 콜백 등록(참조 유지됨) |
| `B2Vec2 Gravity { get; set; }` | 중력 벡터 |
| `void Explode(B2ExplosionDef explosionDef)` | 폭발 충격 적용 |
| `void SetContactTuning(float hertz, float dampingRatio, float pushSpeed)` | 접촉 강성/댐핑/최대 분리 속도 조정 |
| `float ContactRecycleDistance { get; set; }` | 접촉점 재사용 거리 |
| `float MaximumLinearSpeed { get; set; }` | 최대 선속도 제한 |
| `bool EnableWarmStarting { get; set; }` | 웜스타팅(고급, 테스트용) |
| `int AwakeBodyCount` | 현재 깨어있는 바디 수 |
| `B2Profile Profile` | 스텝별 성능 프로파일 (§8) |
| `B2Counters Counters` | 바디/셰이프/접촉 등 카운터 (§8) |
| `B2Capacity MaxCapacity` | 최대 용량 통계 |
| `object UserData { get; set; }` | 임의의 관리 객체를 GCHandle로 저장 |
| `void SetFrictionCallback(B2FrictionCallback callback)` | 마찰 혼합 콜백 등록(참조 유지됨) |
| `void SetRestitutionCallback(B2RestitutionCallback callback)` | 반발 혼합 콜백 등록(참조 유지됨) |
| `int WorkerCount { get; set; }` | 내부 스레드풀 워커 수 (1보다 크면 멀티스레딩 사용) |
| `static B2HexColor GetGraphColor(int index)` | 제약 그래프 색상 슬롯 색 조회 |

---

## 2. B2Body (`Body.cs`)

`readonly struct`. `B2BodyId`를 감싼 값 타입.

| 멤버 | 설명 |
|---|---|
| `static B2Body Create(B2World world, B2BodyDef def)` | 바디 생성 |
| `void Destroy()` | 바디(및 부착된 셰이프/조인트) 파괴 |
| `bool IsValid` | 핸들 유효성 |
| `B2BodyType Type { get; set; }` | Static/Kinematic/Dynamic |
| `string Name { get; set; }` | 디버그용 이름 |
| `object UserData { get; set; }` | GCHandle 기반 사용자 데이터 |
| `B2Vec2 Position` / `B2Rot Rotation` / `B2Transform Transform` | 월드 트랜스폼 조회 |
| `void SetTransform(B2Vec2 position, B2Rot rotation)` | 텔레포트(비용이 큼) |
| `B2Vec2 GetLocalPoint/GetWorldPoint/GetLocalVector/GetWorldVector(...)` | 로컬↔월드 좌표 변환 |
| `B2Vec2 LinearVelocity { get; set; }` / `float AngularVelocity { get; set; }` | 속도 |
| `void SetTargetTransform(B2Transform target, float timeStep, bool wake = true)` | 다음 스텝에 목표 트랜스폼에 도달하는 속도로 설정 (키네마틱용) |
| `B2Vec2 GetLocalPointVelocity/GetWorldPointVelocity(...)` | 특정 점의 속도 |
| `void ApplyForce/ApplyForceToCenter/ApplyTorque(...)` | 힘/토크 적용 |
| `void ClearForces()` | 누적된 힘/토크 제거 |
| `void ApplyLinearImpulse/ApplyLinearImpulseToCenter/ApplyAngularImpulse(...)` | 임펄스 적용 |
| `float Mass` / `float RotationalInertia` / `B2Vec2 LocalCenter` / `B2Vec2 WorldCenter` | 질량 정보 조회 |
| `B2MassData MassData { get; set; }` | 질량 데이터 직접 오버라이드/조회 |
| `void ApplyMassFromShapes()` | 부착된 셰이프들로부터 질량 재계산 |
| `float LinearDamping { get; set; }` / `float AngularDamping { get; set; }` | 감쇠 |
| `float GravityScale { get; set; }` | 중력 배율 |
| `bool IsAwake { get; set; }` | 깨어있는지 여부 (set은 깨우기/재우기) |
| `void WakeTouching()` | 접촉 중인 바디들을 깨움 (정적 바디에도 사용 가능) |
| `bool EnableSleep { get; set; }` | 슬립 허용 여부 |
| `float SleepThreshold { get; set; }` | 슬립 속도 임계값 |
| `bool IsEnabled` / `void Disable()` / `void Enable()` | 시뮬레이션 참여 여부 |
| `B2MotionLocks MotionLocks { get; set; }` | 이동/회전 축 잠금 |
| `bool IsBullet { get; set; }` | 불릿(고속 연속 충돌) 여부 |
| `bool EnableContactRecycling { get; set; }` | 접촉점 재사용 여부 |
| `void EnableContactEvents(bool flag)` / `void EnableHitEvents(bool flag)` | 부착된 모든 셰이프의 이벤트 옵션 일괄 설정 |
| `B2World GetWorld()` | 소속 월드 조회 |
| `int ShapeCount` / `B2Shape[] GetShapes()` | 부착된 셰이프 목록 |
| `int JointCount` / `B2JointId[] GetJoints()` | 부착된 조인트 ID 목록 |
| `int ContactCapacity` / `B2ContactData[] GetContactData()` | 현재 접촉 중인 접촉 데이터 |
| `B2AABB ComputeAABB()` | 부착된 모든 셰이프의 월드 AABB |

---

## 3. B2Shape (`Shape.cs`)

`readonly struct`. `B2ShapeId`를 감싼 값 타입. 셰이프 생성은 타입별 정적 팩토리 메서드를 사용합니다.

| 멤버 | 설명 |
|---|---|
| `static B2Shape CreateCircle(B2Body body, B2ShapeDef def, B2Circle circle)` | 원 셰이프 생성 |
| `static B2Shape CreateSegment(B2Body body, B2ShapeDef def, B2Segment segment)` | 세그먼트 셰이프 생성 |
| `static B2Shape CreateChainSegment(B2Body body, B2ShapeDef def, B2ChainSegment chainSegment)` | 독립된(orphan) 체인 세그먼트 생성 |
| `static B2Shape CreateCapsule(B2Body body, B2ShapeDef def, B2Capsule capsule)` | 캡슐 셰이프 생성 |
| `static B2Shape CreatePolygon(B2Body body, B2ShapeDef def, B2Polygon polygon)` | 폴리곤 셰이프 생성 |
| `void Destroy(bool updateBodyMass = true)` | 셰이프 파괴 |
| `bool IsValid` | 핸들 유효성 |
| `B2ShapeType Type` | Circle/Capsule/Segment/Polygon/ChainSegment |
| `B2Body GetBody()` / `B2World GetWorld()` | 소유자 조회 |
| `bool IsSensor` | 센서 여부 (생성 후 변경 불가) |
| `object UserData { get; set; }` | GCHandle 기반 사용자 데이터 |
| `void SetDensity(float density, bool updateBodyMass = true)` / `float Density` | 밀도 |
| `float Friction { get; set; }` / `float Restitution { get; set; }` | 마찰/반발 |
| `ulong UserMaterial { get; set; }` | 사용자 정의 재질 ID |
| `B2SurfaceMaterial SurfaceMaterial { get; set; }` | 표면 재질 전체 |
| `B2Filter Filter { get; set; }` | 충돌 필터 |
| `bool EnableSensorEvents { get; set; }` | 센서 이벤트 발생 여부 |
| `bool EnableContactEvents { get; set; }` | 접촉 이벤트 발생 여부 |
| `bool EnablePreSolveEvents { get; set; }` | pre-solve 이벤트 발생 여부 |
| `bool EnableHitEvents { get; set; }` | hit 이벤트 발생 여부 |
| `bool TestPoint(B2Vec2 point)` | 점 포함 테스트 |
| `B2CastOutput RayCast(B2Vec2 origin, B2Vec2 translation)` | 단일 셰이프 레이캐스트 |
| `B2Circle GetCircle()` / `B2Segment GetSegment()` / `B2ChainSegment GetChainSegment()` / `B2Capsule GetCapsule()` / `B2Polygon GetPolygon()` | 형상 데이터 조회 (타입이 맞아야 함) |
| `void SetCircle/SetCapsule/SetSegment/SetPolygon/SetChainSegment(...)` | 형상 데이터 교체 (질량은 자동 갱신 안 됨) |
| `B2ChainId GetParentChain()` | 체인 소속 여부 (없으면 `Null`) |
| `int ContactCapacity` / `B2ContactData[] GetContactData()` | 현재 접촉 데이터 |
| `int SensorCapacity` / `B2Shape[] GetSensorData()` | (센서인 경우) 겹쳐 있는 셰이프 목록 |
| `B2AABB AABB` | 현재 월드 AABB |
| `B2MassData ComputeMassData()` | 질량 데이터 계산 |
| `B2Vec2 GetClosestPoint(B2Vec2 target)` | 셰이프 표면에서 target에 가장 가까운 점 |
| `void ApplyWind(B2Vec2 wind, float drag, float lift, bool wake = true)` | 바람 저항력 적용 |

---

## 4. B2Chain (`Chain.cs`)

`readonly struct`. `B2ChainId`를 감싼 값 타입. 한쪽 방향으로만 충돌하는 연결된 세그먼트(체인) 모음.

| 멤버 | 설명 |
|---|---|
| `static B2Chain Create(B2Body body, B2ChainDef def, B2Vec2[] points, B2SurfaceMaterial[] materials)` | 체인 생성. `points`는 4개 이상, `materials`는 1개 또는 세그먼트 수만큼. 배열은 호출 중에만 고정(pin)되며 Box2D가 즉시 복사함 |
| `static B2Chain Create(B2Body body, B2ChainDef def, B2Vec2[] points, B2SurfaceMaterial material)` | 재질 1개를 모든 세그먼트에 적용하는 오버로드 |
| `void Destroy()` | 체인 파괴 |
| `bool IsValid` | 핸들 유효성 |
| `B2World GetWorld()` | 소속 월드 |
| `int SegmentCount` / `B2Shape[] GetSegments()` | 생성된 세그먼트 셰이프 목록 |
| `int SurfaceMaterialCount` | 재질 개수 |
| `void SetSurfaceMaterial(B2SurfaceMaterial material, int materialIndex)` / `B2SurfaceMaterial GetSurfaceMaterial(int materialIndex)` | 세그먼트별 재질 설정/조회 |

---

## 5. B2Contact (`Contact.cs`)

`readonly struct`. `B2ContactId`를 감싼 값 타입. **일시적**인 핸들이므로(월드가 스텝마다 접촉 그래프를 재구성함)
이전 스텝에서 받은 ID는 사용 전에 항상 `IsValid`를 확인하세요.

| 멤버 | 설명 |
|---|---|
| `bool IsValid` | 유효성 확인 (특히 이벤트에서 받은 ID) |
| `B2ContactData GetData()` | 접촉 매니폴드 데이터 조회 |

---

## 6. 조인트 (`Joints/*.cs`)

모든 조인트 랩퍼는 `readonly struct`이고, `B2JointId` / `B2Joint`(공용 API)로의 암시적 변환을 제공합니다.
공용 기능은 `B2Joint`에, 타입별 기능은 각 구조체에 있습니다. `.Generic` 프로퍼티로 `B2Joint`에 접근하세요.

### 6.1 B2Joint — 모든 조인트 공통 기능

| 멤버 | 설명 |
|---|---|
| `void Destroy(bool wakeAttached = true)` | 조인트 파괴 |
| `bool IsValid` / `B2JointType Type` | 유효성 / 타입 (Distance, Filter, Motor, Prismatic, Revolute, Weld, Wheel) |
| `B2Body GetBodyA()` / `B2Body GetBodyB()` / `B2World GetWorld()` | 연결된 바디/월드 |
| `B2Transform LocalFrameA { get; set; }` / `LocalFrameB { get; set; }` | 각 바디 기준 로컬 프레임 |
| `bool CollideConnected { get; set; }` | 연결된 두 바디 간 충돌 허용 여부 |
| `object UserData { get; set; }` | GCHandle 기반 사용자 데이터 |
| `void WakeBodies()` | 연결된 바디 깨우기 |
| `B2Vec2 GetConstraintForce()` / `float GetConstraintTorque()` | 현재 구속력/토크 |
| `float GetLinearSeparation()` / `float GetAngularSeparation()` | 현재 분리 오차 |
| `void SetConstraintTuning(float hertz, float dampingRatio)` / `void GetConstraintTuning(out float hertz, out float dampingRatio)` | 구속 강성 튜닝(고급) |
| `float ForceThreshold { get; set; }` / `float TorqueThreshold { get; set; }` | 조인트 이벤트 발생 임계값 |

### 6.2 B2DistanceJoint — 두 점을 세그먼트로 연결 (로프/스프링)

`Create(B2World, B2DistanceJointDef)`. 주요 멤버: `Length`, `EnableSpring`, `SetSpringForceRange/GetSpringForceRange`,
`SpringHertz`, `SpringDampingRatio`, `EnableLimit`, `SetLengthRange`, `MinLength`, `MaxLength`, `CurrentLength`,
`EnableMotor`, `MotorSpeed`, `MaxMotorForce`, `MotorForce`(읽기 전용).

### 6.3 B2MotorJoint — 두 바디 사이 상대 속도/변환 제어 (top-down 마찰 등)

`Create(B2World, B2MotorJointDef)`. 주요 멤버: `LinearVelocity`, `AngularVelocity`, `MaxVelocityForce`,
`MaxVelocityTorque`, `LinearHertz`, `LinearDampingRatio`, `MaxSpringForce`, `AngularHertz`,
`AngularDampingRatio`, `MaxSpringTorque`.

### 6.4 B2FilterJoint — 두 바디 간 충돌만 비활성화

`Create(B2World, B2FilterJointDef)`. 타입 고유 멤버 없음 (`.Generic`으로 공통 기능만 사용).

### 6.5 B2PrismaticJoint — 슬라이더(피스톤, 이동 플랫폼)

`Create(B2World, B2PrismaticJointDef)`. 주요 멤버: `EnableSpring`, `SpringHertz`, `SpringDampingRatio`,
`TargetTranslation`, `EnableLimit`, `LowerLimit`/`UpperLimit`(읽기 전용), `SetLimits`, `EnableMotor`,
`MotorSpeed`, `MaxMotorForce`, `MotorForce`(읽기 전용), `Translation`(읽기 전용), `Speed`(읽기 전용).

### 6.6 B2RevoluteJoint — 힌지/핀 조인트 (가장 흔한 조인트)

`Create(B2World, B2RevoluteJointDef)`. 주요 멤버: `EnableSpring`, `SpringHertz`, `SpringDampingRatio`,
`TargetAngle`, `Angle`(읽기 전용), `EnableLimit`, `LowerLimit`/`UpperLimit`(읽기 전용), `SetLimits`,
`EnableMotor`, `MotorSpeed`, `MotorTorque`(읽기 전용), `MaxMotorTorque`.

### 6.7 B2WeldJoint — 두 바디를 강체처럼 연결 (스프링 옵션 포함)

`Create(B2World, B2WeldJointDef)`. 주요 멤버: `LinearHertz`, `LinearDampingRatio`, `AngularHertz`,
`AngularDampingRatio`.

### 6.8 B2WheelJoint — 차량 바퀴 시뮬레이션용

`Create(B2World, B2WheelJointDef)`. 주요 멤버: `EnableSpring`, `SpringHertz`, `SpringDampingRatio`,
`EnableLimit`, `LowerLimit`/`UpperLimit`(읽기 전용), `SetLimits`, `EnableMotor`, `MotorSpeed`,
`MaxMotorTorque`, `MotorTorque`(읽기 전용).

---

## 7. 수학 / 기하 타입 (`Types/B2Vec2.cs`, `B2Math.cs`, `B2Shapes.cs`)

### 7.1 B2Vec2 — 2D 벡터

필드 `x`, `y`. `UnityEngine.Vector2`와 암시적 변환, `Vector3`와는 명시적 변환(z=0으로 투영/추출).
정적 유틸리티: `Dot`, `Cross`, `CrossVS`, `CrossSV`, `LeftPerp`, `RightPerp`, `Lerp`, `Mul`, `MulAdd`, `MulSub`,
`Abs`, `Min`, `Max`, `Clamp`, `Distance`, `DistanceSquared`. 인스턴스: `Length()`, `LengthSquared()`,
`Normalized()`, `IsNormalized()`. 연산자 `+ - * == !=` 지원.

### 7.2 B2Rot — 2D 회전 (cos/sin 쌍)

`Identity`, `FromAngle(float radians)`(네이티브 삼각함수 사용, Box2D 솔버와 동일한 결과 보장), `FromUnitVector`,
`Angle`(읽기 전용, 네이티브 `atan2` 사용), `XAxis`/`YAxis`, `IsNormalized()`, `Normalized()`, `Inverted()`,
`Integrate(float deltaAngle)`, `NLerp`, `ComputeAngularVelocity`, `RotateVector`/`InvRotateVector`,
연산자 `*`(합성), `InvMul`, `RelativeAngle`, `UnwindAngle`. `UnityEngine.Quaternion`과 암시적 변환(Z축 회전 기준).

### 7.3 B2Transform — 위치 + 회전

필드 `p`(B2Vec2), `q`(B2Rot). `Identity`, `TransformPoint`/`InvTransformPoint`, 정적 `Mul`/`InvMul`.
(단정밀도 가정이므로 `b2WorldTransform`과 동일하게 취급됩니다.)

### 7.4 B2Mat22 — 2x2 행렬

필드 `cx`, `cy`(열벡터). `Zero`, 연산자 `*`(행렬-벡터 곱), `Inverse()`, `Solve(b)`.

### 7.5 B2AABB — 축정렬 바운딩 박스

필드 `lowerBound`, `upperBound`. `Center`, `Extents`, `Contains`, `Overlaps`, 정적 `Union`.

### 7.6 B2Plane — 2D 평면

필드 `normal`, `offset`. `Separation(point)`.

### 7.7 B2CosSin

필드 `cosine`, `sine`. `B2Rot.FromAngle`이 내부적으로 사용.

### 7.8 B2MathUtil (정적 클래스)

`SpringDamper(hertz, dampingRatio, position, velocity, timeStep)` — 1차원 스프링-댐퍼 적분 유틸리티.

### 7.9 셰이프 형상 구조체

| 타입 | 필드 | 주요 메서드 |
|---|---|---|
| `B2Circle` | `center`, `radius` | `ComputeMass`, `ComputeAABB`, `ContainsPoint`, `RayCast`, `ShapeCast` |
| `B2Capsule` | `center1`, `center2`, `radius` | 위와 동일 |
| `B2Segment` | `point1`, `point2` | `ComputeAABB`, `RayCast(input, oneSided)`, `ShapeCast` |
| `B2ChainSegment` | `ghost1`, `segment`, `ghost2`, `chainId` | 없음(데이터 전용) |
| `B2Polygon` | `vertices[8]`, `normals[8]`, `centroid`, `radius`, `count` | `MakeBox`, `MakeSquare`, `MakeRoundedBox`, `MakeOffsetBox`, `MakeOffsetRoundedBox`, `MakeFromHull`, `MakeOffsetFromHull`, `MakeOffsetRoundedFromHull`, `TransformedBy`, `ComputeMass`, `ComputeAABB`, `ContainsPoint`, `RayCast`, `ShapeCast` |
| `B2Hull` | `points[8]`, `count` | `static Compute(points[])`, `Validate()` |
| `B2ShapeProxy` | `points[8]`, `count`, `radius` | `static Make(points, radius)`, `static MakeOffset(points, radius, position, rotation)` |
| `B2MassData` | `mass`, `center`, `rotationalInertia` | — |
| `B2SurfaceMaterial` | `friction`, `restitution`, `rollingResistance`, `tangentSpeed`, `userMaterialId`, `customColor` | `static Default()` |
| `B2RayCastInput` | `origin`, `translation`, `maxFraction` | `IsValid()` |
| `B2ShapeCastInput` | `proxy`, `translation`, `maxFraction`, `CanEncroach`(bool 프로퍼티) | — |
| `B2CastOutput` | `normal`, `point`, `fraction`, `iterations`, `Hit`(bool, 읽기 전용) | — |
| `B2TreeStats` | `nodeVisits`, `leafVisits` | 쿼리 성능 통계 |

`B2Constants.MaxPolygonVertices` (= 8)도 함께 제공됩니다.

---

## 8. 정의(Def) 구조체 (`Types/B2Defs.cs`, `B2JointDefs.cs`, `B2Filters.cs`)

모두 `static Default()` 정적 팩토리를 제공하며, `bool` 필드는 `EnableXxx`/`IsXxx` 형태의 **프로퍼티**이고
나머지는 C API와 동일한 이름의 소문자 **필드**입니다.

### 8.1 B2WorldDef

필드: `gravity`, `restitutionThreshold`, `hitEventThreshold`, `contactHertz`, `contactDampingRatio`,
`contactSpeed`, `maximumLinearSpeed`, `workerCount`, `capacity`(B2Capacity).
프로퍼티: `EnableSleep`, `EnableContinuous`, `EnableContactSoftening`.
(`frictionCallback`/`restitutionCallback`/`enqueueTask`/`finishTask`/`userTaskContext`/`userData`는 `IntPtr`
로 노출되며, 보통 직접 건드리지 않고 `B2World`의 `SetFrictionCallback`/`SetRestitutionCallback`/`UserData`를
사용하면 됩니다.)

### 8.2 B2BodyDef

필드: `type`(B2BodyType), `position`, `rotation`, `linearVelocity`, `angularVelocity`, `linearDamping`,
`angularDamping`, `gravityScale`, `sleepThreshold`, `name`(string), `motionLocks`(B2MotionLocks).
프로퍼티: `EnableSleep`, `IsAwake`, `IsBullet`, `IsEnabled`, `AllowFastRotation`, `EnableContactRecycling`.

### 8.3 B2ShapeDef

필드: `material`(B2SurfaceMaterial), `density`, `filter`(B2Filter).
프로퍼티: `EnableCustomFiltering`, `IsSensor`, `EnableSensorEvents`, `EnableContactEvents`,
`EnableHitEvents`, `EnablePreSolveEvents`, `InvokeContactCreation`, `UpdateBodyMass`.

### 8.4 B2ChainDef

필드: `count`, `materialCount`, `filter`(B2Filter). 프로퍼티: `IsLoop`, `EnableSensorEvents`.
(`points`/`materials`는 `IntPtr` 필드로, 직접 채우지 말고 `B2Chain.Create(...)`를 사용하세요 — 배열 pin/복사를
대신 처리해 줍니다.)

### 8.5 B2ExplosionDef

필드: `maskBits`, `position`, `radius`, `falloff`, `impulsePerLength`.

### 8.6 B2Filter / B2QueryFilter / B2MotionLocks

- `B2Filter`: `categoryBits`, `maskBits`, `groupIndex`. `static Default()`.
- `B2QueryFilter`: `categoryBits`, `maskBits`. `static Default()`.
- `B2MotionLocks`: `LinearX`, `LinearY`, `AngularZ` (모두 bool 프로퍼티).

### 8.7 조인트 정의 구조체

공통 베이스는 `B2JointDef` 타입이며, 각 구체적 Def는 `Base` 필드(타입 `B2JointDef`)로 이를 포함합니다.

`B2JointDef.Base` 필드: `bodyIdA`, `bodyIdB`, `localFrameA`, `localFrameB`, `forceThreshold`,
`torqueThreshold`, `constraintHertz`, `constraintDampingRatio`, `drawScale`, `CollideConnected`(bool 프로퍼티).

| Def | 고유 필드 | 고유 bool 프로퍼티 |
|---|---|---|
| `B2DistanceJointDef` | `length`, `lowerSpringForce`, `upperSpringForce`, `hertz`, `dampingRatio`, `minLength`, `maxLength`, `maxMotorForce`, `motorSpeed` | `EnableSpring`, `EnableLimit`, `EnableMotor` |
| `B2MotorJointDef` | `linearVelocity`, `maxVelocityForce`, `angularVelocity`, `maxVelocityTorque`, `linearHertz`, `linearDampingRatio`, `maxSpringForce`, `angularHertz`, `angularDampingRatio`, `maxSpringTorque` | (없음) |
| `B2FilterJointDef` | (없음) | (없음) |
| `B2PrismaticJointDef` | `hertz`, `dampingRatio`, `targetTranslation`, `lowerTranslation`, `upperTranslation`, `maxMotorForce`, `motorSpeed` | `EnableSpring`, `EnableLimit`, `EnableMotor` |
| `B2RevoluteJointDef` | `targetAngle`, `hertz`, `dampingRatio`, `lowerAngle`, `upperAngle`, `maxMotorTorque`, `motorSpeed` | `EnableSpring`, `EnableLimit`, `EnableMotor` |
| `B2WeldJointDef` | `linearHertz`, `angularHertz`, `linearDampingRatio`, `angularDampingRatio` | (없음) |
| `B2WheelJointDef` | `hertz`, `dampingRatio`, `lowerTranslation`, `upperTranslation`, `maxMotorTorque`, `motorSpeed` | `EnableSpring`, `EnableLimit`, `EnableMotor` |

각 Def는 `static Default()` 팩토리를 가집니다 (예: `B2RevoluteJointDef.Default()`).

---

## 9. 열거형 (`Types/B2Enums.cs`, `B2HexColor.cs`)

- `B2BodyType`: `Static = 0`, `Kinematic = 1`, `Dynamic = 2`
- `B2ShapeType`: `Circle`, `Capsule`, `Segment`, `Polygon`, `ChainSegment`
- `B2JointType`: `Distance`, `Filter`, `Motor`, `Prismatic`, `Revolute`, `Weld`, `Wheel`
- `B2HexColor`: SVG 색상 이름 전체 + `Box2DRed`/`Box2DBlue`/`Box2DGreen`/`Box2DYellow` (값은 `0xRRGGBB`).
  확장 메서드 `ToColor32(byte alpha = 255)`로 `UnityEngine.Color32` 변환 가능(Unity 환경에서만).

---

## 10. 이벤트 / 진단 구조체 (`Types/B2Events.cs`)

| 타입 | 필드 |
|---|---|
| `B2RayResult` | `shapeId`, `point`, `normal`, `fraction`, `nodeVisits`, `leafVisits`, `Hit`(bool, 읽기 전용) |
| `B2SensorBeginTouchEvent` / `B2SensorEndTouchEvent` | `sensorShapeId`, `visitorShapeId` |
| `B2ContactBeginTouchEvent` / `B2ContactEndTouchEvent` | `shapeIdA`, `shapeIdB`, `contactId` |
| `B2ContactHitEvent` | `shapeIdA`, `shapeIdB`, `contactId`, `point`, `normal`, `approachSpeed` |
| `B2BodyMoveEvent` | `userData`(IntPtr), `transform`, `bodyId`, `FellAsleep`(bool, 읽기 전용) |
| `B2JointEvent` | `jointId`, `userData`(IntPtr) |
| `B2ContactData` | `contactId`, `shapeIdA`, `shapeIdB`, `manifold`(B2Manifold) |
| `B2Manifold` | `normal`, `rollingImpulse`, `points[2]`(B2ManifoldPoint), `pointCount` |
| `B2ManifoldPoint` | `anchorA`, `anchorB`, `separation`, `baseSeparation`, `normalImpulse`, `tangentImpulse`, `totalNormalImpulse`, `normalVelocity`, `id`(ushort), `Persisted`(bool, 읽기 전용) |
| `B2Profile` | `step`, `pairs`, `collide`, `solve`, ... 등 스텝 단계별 소요 시간(ms), 총 23개 필드 |
| `B2Counters` | `byteCount`, `bodyCount`, `shapeCount`, `contactCount`, `jointCount`, `islandCount`, `stackUsed`, `staticTreeHeight`, `treeHeight`, `taskCount`, `colorCounts[24]`, `awakeContactCount`, `recycledContactCount` |

이벤트 배열은 `B2World`의 `GetBodyEvents()`/`GetSensorEvents(...)`/`GetContactEvents(...)`/`GetJointEvents()`
로 가져오세요(내부적으로 네이티브 포인터+개수를 관리형 배열로 복사해 줍니다).

---

## 11. 콜백 델리게이트 (`Types/B2Callbacks.cs`)

| 델리게이트 | 시그니처 | 용도 |
|---|---|---|
| `B2OverlapResultFcn` | `bool(B2ShapeId shapeId, IntPtr context)` | 오버랩 쿼리 결과 콜백. `false`면 쿼리 중단 |
| `B2CastResultFcn` | `float(B2ShapeId shapeId, B2Vec2 point, B2Vec2 normal, float fraction, IntPtr context)` | 레이/셰이프 캐스트 결과 콜백. 반환값으로 클리핑 제어 |
| `B2CustomFilterFcn` | `bool(B2ShapeId a, B2ShapeId b, IntPtr context)` | 커스텀 충돌 필터(스레드 안전 필요) |
| `B2PreSolveFcn` | `bool(B2ShapeId a, B2ShapeId b, B2Vec2 point, B2Vec2 normal, IntPtr context)` | pre-solve 콜백(스레드 안전 필요) |
| `B2FrictionCallback` | `float(float frictionA, ulong matA, float frictionB, ulong matB)` | 마찰 혼합 함수 |
| `B2RestitutionCallback` | `float(float restitutionA, ulong matA, float restitutionB, ulong matB)` | 반발 혼합 함수 |

`B2World.SetXxxCallback`류 메서드에 넘긴 델리게이트는 `B2World` 내부 필드로 참조가 유지되므로 GC 걱정 없이
사용할 수 있습니다. 반면 `OverlapAABB`/`CastRay` 등 쿼리성 콜백은 호출 동안만 유효하면 되므로 별도 보관이
필요 없습니다.

---

## 12. 디버그 드로우 (`Types/B2DebugDraw.cs`)

`B2DebugDraw`는 일반 클래스이며, 필요한 `Draw*` 가상 메서드만 오버라이드해서 `B2World.Draw(draw)`에 넘기면
됩니다(Gizmos나 `Debug.DrawLine` 등으로 그리기 구현).

가상 메서드: `DrawPolygon`, `DrawSolidPolygon`, `DrawCircle`, `DrawSolidCircle`, `DrawSolidCapsule`,
`DrawLine`, `DrawTransform`, `DrawPoint`, `DrawString`, `DrawBounds`.

설정 필드(모두 public, 기본값 있음): `DrawingBounds`(B2AABB), `ForceScale`, `JointScale`, `DrawContacts`,
`DrawAnchorA`, `DrawShapes`(기본 true), `DrawChainNormals`, `DrawJoints`(기본 true), `DrawJointExtras`,
`DrawShapeBounds`, `DrawMass`, `DrawBodyNames`, `DrawGraphColors`, `DrawContactFeatures`,
`DrawContactNormals`, `DrawContactForces`, `DrawFrictionForces`, `DrawIslands`.

---

## 13. ID / 핸들 타입 (`Types/B2Ids.cs`)

`B2WorldId`, `B2BodyId`, `B2ShapeId`, `B2ChainId`, `B2JointId`, `B2ContactId` — 모두 네이티브 구조체와 1:1로
매핑되는 값 타입입니다. 공통 멤버: `static Null`, `IsNull`, `IsNonNull`, `Equals`/`==`/`!=`. 직접 다룰 일은
거의 없고, 각 상위 랩퍼(`B2Body.Id` 등)를 통해 사용합니다.

---

## 14. 제외된 기능 (Not implemented)

다음은 게임 개발에서 잘 쓰이지 않는 고급/내부 기능이라 이 바인딩에서 의도적으로 제외했습니다. 필요하면
`Runtime/Interop/`에 같은 패턴으로 `[DllImport]` 선언을 추가하면 됩니다:

- Recording / Snapshot / Replay API (`b2CreateRecording`, `b2World_Snapshot`, `b2RecPlayer_*`)
- Dynamic Tree API (`b2DynamicTree_*`)
- GJK 거리/TOI 저수준 함수 (`b2ShapeDistance`, `b2TimeOfImpact`, 심플렉스 타입)
- 캐릭터 무버 함수 (`b2World_CastMover`, `b2World_CollideMover`, `b2SolvePlanes`, `b2ClipVector`)
- 커스텀 태스크 시스템 콜백 (`b2EnqueueTaskCallback`/`b2FinishTaskCallback`) — `B2WorldDef.workerCount`만
  설정하면 Box2D 자체 스레드풀을 사용합니다.

---

## 15. 최소 예제

```csharp
using Box2D;

B2World world = B2World.Create(); // 기본 중력 (0, -10)

B2BodyDef groundDef = B2BodyDef.Default();
groundDef.type = B2BodyType.Static;
B2Body ground = B2Body.Create(world, groundDef);
B2Shape.CreatePolygon(ground, B2ShapeDef.Default(), B2Polygon.MakeBox(10f, 0.5f));

B2BodyDef boxDef = B2BodyDef.Default();
boxDef.type = B2BodyType.Dynamic;
boxDef.position = new B2Vec2(0f, 5f);
B2Body box = B2Body.Create(world, boxDef);
B2Shape.CreatePolygon(box, B2ShapeDef.Default(), B2Polygon.MakeBox(0.5f, 0.5f));

// 매 FixedUpdate 마다:
world.Step(Time.fixedDeltaTime, 4);
transform.position = (Vector2)box.Position;
transform.rotation = box.Rotation;
```

더 완전한 예제는 `Samples~/HelloWorld/HelloWorldBox2D.cs`를, 네이티브 라이브러리 없이 바인딩 자체를
검증하는 예제는 `native/tests/Box2DSmokeTest/Program.cs`를 참고하세요.
