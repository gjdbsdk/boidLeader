# 🐬 boidleader

> **사이버 데이터 금고에 갇힌 2D 네온 픽셀 물고기들을 구출하여 바다로 인도하는 3D 잠입 & 군집 생존 게임**  
> 덕성여자대학교 디지털소프트웨어공학부 | **게임프로그래밍 심화 (Advanced Game Programming)** 프로젝트

---

## 📌 1. 프로젝트 개요 (Project Overview)

* **프로젝트명**: `boidleader`
* **장르**: 3D 잠입 액션 & 군집 인도 시뮬레이션 (3D Stealth & Flocking Simulation)
* **개발 환경**: Unity 6 (6000.0 LTS, Universal Render Pipeline), C#
* **핵심 컨셉**: 
  * 평화롭던 대서양의 해양 생물들이 사이버 도둑 조직에게 포획되어 **2D 디지털 데이터(네온 픽셀)** 형태로 추출·감금되었습니다.
  * 플레이어는 데이터 잠입 요원(리더)이 되어 경비병들의 시야와 감시망을 피해 물고기 데이터를 해방시키고, **Boids 군집 대열**을 이루어 최종 탈출 지점인 **해양 업링크 포탈(Ocean Uplink Portal)**까지 안전하게 인도해야 합니다.

---

## 🧠 2. 핵심 게임 AI 기술 설계 (Core Game AI Architecture)

본 프로젝트는 강의에서 학습한 핵심 게임 인공지능 이론과 실습 코드 구조를 기반으로 설계되었습니다.

| 구분 | 적용 대상 | 적용 AI 기술 (실습 기반) | 세부 구현 내용 |
| :--- | :--- | :--- | :--- |
| **군집 AI**<br>*(Collective AI)* | 구출된 2D 네온 데이터 물고기 무리 | **Flocking (Boids 알고리즘)**<br>(`Flock.cs`, `FlockController.cs` 기반) | • **분리(Separation), 정렬(Alignment), 응집(Cohesion)** 3대 규칙을 통한 군집 대열 유지<br>• 플레이어를 리더(`origin`)로 추종하며, 구출 개체 수 증가에 따라 대열 길이 확장 |
| **감각 시스템**<br>*(Sensor System)* | 사이버 도둑 (경비병) | **Sense & Aspect 아키텍처**<br>(`Aspect.cs`, `Sense.cs`, `Sight.cs`, `Touch.cs` 기반) | • **시각(`Sight`)**: `Vector3.Angle` 시야각($FOV$) 및 `Physics.Raycast`를 통한 플레이어/물고기 대열 탐지 (서버 랙 엄폐 시 Ray 차단)<br>• **촉각/근접(`Touch`)**: `OnTriggerEnter` 기반 근접 발각 판정 |
| **의사결정 AI**<br>*(Decision Making)* | 사이버 도둑 (경비병) | **FSM / Behavior Tree**<br>(`FSM.cs`, `SimpleFSM.cs`, `Behavior Bricks` 기반) | • `Patrol(순찰)` → `Chase(추격)` → `Attack(재포획)` 상태 전이 제어<br>• 우선순위 선택자(`Priority Selector`) 및 조건 감시(`Condition`) 기반 행동 제어 |
| **경로 탐색 및 조향**<br>*(Path & Steering)* | 경비병 순찰 및 장애물 우회 | **Waypoints & NavMesh**<br>(`Path.cs`, `VehicleAvoidance.cs`, `NavMeshAgent` 기반) | • 지정된 `WayPoints` 경로 순찰 및 `Physics.SphereCast`(Bitmask Layer) 기반 장애물 회피<br>• `NavMeshSurface` 베이킹 및 `NavMeshAgent`를 활용한 실내 서버실 최단 경로 추격 |
| **소극적 AI**<br>*(Passive AI)* | 조력자 해달 데이터 (`Otter Data`), 보안 센서 | **Trigger 반응형 시스템** | • 플레이어 접근 트리거(`OnTriggerEnter`) 시에만 반응하여 주변 경비병 시야 교란 및 무리 회복/속도 버프 제공 |

---

## 🎮 3. 주요 게임 메커니즘 (Key Mechanics)

1. **잠입과 엄폐 (Stealth & Cover)**
   * 3D 서버실 맵 내의 서버 랙(`Server Rack`)과 벽면을 이용해 보초 도둑의 붉은 시야 원뿔(`Vision Cone`)을 회피합니다.
2. **데이터 해방 및 꼬리 관리 리스크 (Boids Tail Risk)**
   * 데이터 캡슐(`Data Capsule`)에 접근해 2D 네온 물고기들을 구출하면 플레이어 뒤를 따르는 Boids 무리에 합류합니다.
   * 구출한 물고기가 많아질수록 군집 행렬이 길어져 엄폐물 뒤에 숨을 때 꼬리가 발각될 위험이 높아지는 전략적 긴장감을 제공합니다.
3. **2D 빌보드 & 네온 글로우 연출 (Billboard & URP Bloom)**
   * 3D 공간 내에서 2D 픽셀 물고기가 항상 카메라를 바라보도록 빌보드(Billboard) 처리하고, URP Emission과 Global Volume(Bloom)을 결합해 홀로그램 데이터 비주얼을 연출합니다.

---

## 🛠️ 4. 현재 개발 진행 상황 (Current Progress)

- [x] **기획 및 레벨 디자인**: `boidleader` 컨셉 확정 및 데이터 금고(Data Vault) 미니맵/동선 설계 완료
- [x] **렌더링 및 맵 환경 구축**: Unity 6 URP 프로젝트 세팅, `Global Volume` 및 실내 `Map` 배치 완료
- [x] **캐릭터 모델 임포트**: 플레이어(`Ch31_nonPBR`) 및 적 경비병(`Ch18_nonPBR`) 3D 캐릭터 에셋 임포트 완료
- [ ] **3인칭 캐릭터 컨트롤러 연동 (진행 중)**: `Starter Assets - ThirdPerson` 및 `Cinemachine` 패키지를 활용한 `Ch31_nonPBR` 3인칭 카메라/이동 연동
- [ ] **Boids 물고기 군집 구현**: `Flock` 알고리즘 이식 및 구출 합류 로직 작성
- [ ] **경비병 AI 구현**: `Sight`/`Touch` 감각 센서 및 `FSM`/`NavMeshAgent` 순찰·추격 구현
- [ ] **스테이지 기믹 및 UI**: 조력자(해달 데이터) 버프, 탈출 포탈(`Ocean Uplink Portal`) 및 게임 루프 UI 완성

---

## 📂 5. 폴더 구조 (Directory Structure)

```text
Assets/
├── Models/                  # 캐릭터(Ch18, Ch31 등) 및 3D 프랍 모델
├── Materials/               # URP 머티리얼 및 네온 발광 셰이더 에셋
├── Prefabs/                 # 플레이어, 경비병, 데이터 물고기(Boid), 캡슐 프리팹
├── Scenes/                  # 메인 게임 씬
├── StarterAssets/           # 3인칭 캐릭터 컨트롤러 및 입력 시스템
└── Scripts/
    ├── Flocking/            # Flock.cs, FlockController.cs (Boids 군집)
    ├── Sensors/             # Aspect.cs, Sense.cs, Sight.cs, Touch.cs (감각 시스템)
    ├── FSM/                 # FSM.cs, GuardFSM.cs (경비병 상태 제어)
    └── Path/                # Path.cs, Waypoint 관리
```

---

## 📦 6. 사용한 외부 에셋 및 패키지 (External Assets & Packages)

* **Unity Packages**:
  * `Universal Render Pipeline (URP)`
  * `Cinemachine` (`com.unity.cinemachine`)
  * `AI Navigation` (`NavMeshSurface`, `NavMeshAgent`)
* **Asset Store & 3D Models**:
  * `Starter Assets - ThirdPerson` (Unity Technologies)
  * `Adobe Mixamo` (3D Humanoid Characters: `Ch18_nonPBR`, `Ch31_nonPBR` & Animations)
  * `Behavior Bricks` (PadaOne Games - Behavior Tree 구현용)

---

## 👤 7. 개발자 정보 (Author)

* **이름**: 허윤아 (20240960)
* **소속**: 덕성여자대학교 디지털소프트웨어공학부
