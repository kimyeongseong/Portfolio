# Portfolio

------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

**[DangerOutside](https://www.youtube.com/shorts/YmX-osyXW10?feature=share)**



  1	PlayFab 이메일 로그인/회원가입	EditorLoginManager.Login(), Register(), GetServerDatas()

  2	STAGE 저장 및 불러오기 정보	EditorManager 전반 – Start(), OnSaveStage(), StageDropdownChanged() 등

  3	STAGE 맵 타일 배치 및 초기 설정 세팅	EditorTileManager.SetTiles(int x,int y) + SetCamera()

  4	에디터 실시간 상호작용	EditorTileManager.SetCheckerPos() / FindEmptyTiles()

  5	건물 배치	EditorBuildingManager의 OnBuildingCreate(), OnMouseDrag(), OnMouseUp(), OnDetail()

  6	유닛 배치 & 속성 편집	EditorCitizenManager의 OnCitizenCreate(), 드래그 루틴, OnDetail()

  7	타일 속성 변경	EditorTileManager.OnTileColorClick() / SetColorChange()

  8	스테이지 데이터 로드&동기화	EditorDataBase (싱글턴 패턴)
 
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
**[MUTE](https://youtu.be/y38XHhIzL0Y?si=Gl-Ovg8Akp7h3ICM)**



  1 기획서
  
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

**GGJ_2021(빙떡마트)**

  1	네트워크 게임 사이클 관리GameManager.cs
  
  싱글턴 패턴으로 전역 접근성 보장- RPC (SetGameStart, selectHunt, gameStart 등)로 헌터·러너 역할 배정, 게임 대기·준비·진행·종료 상태 전환 전부 동기화
  
  2	매치메이킹·로비LobbyManager.cs, NetworkMgr.cs
  
  마스터 서버 자동 재접속 로직, 랜덤 룸 참가 실패 시 새 룸 생성- 인원 수 UI (changeConnectCount)와 연결 상태 텍스트로 UX 개선
  
  3	플레이어 생성·관리GameManager.cs, PlayerManager.cs
  
  PhotonNetwork.Instantiate로 네트워크 플레이어 동적 생성 후 리스트 관리- 플레이어 사망/퇴장 시 런너·헌터 카운트 갱신 및 승패 처리
  
  4	위장(모델 변신) 시스템PlayerManager.cs
  
  changeModel()에서 랜덤 스킨 선택 후 RPC로 모든 클라이언트 동기화 → 숨바꼭질 핵심 재미 요소- 모델 분리·붙이기(modelHold)로 잡았다/풀어줬다 표현
  
  5	플레이어 컨트롤PlayerManager.cs, Common.cs
  
  카메라 암(Cinemachine 없이 직접) + 마우스 입력으로 TPS 시야 회전- 이동·점프·중력 간단 구현, 공용 파라미터(speed, jumpforce) 구조화
  
  6	실시간 UI 동기화GameManager.cs
  
  OnPhotonSerializeView로 타이머·남은 인원 텍스트를 실시간 전송/수신하여 로컬·원격 모두 동일 화면 유지
  
  ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
**[용자식당](https://youtu.be/y4u6sUdqxXU)**
  
1	Floor / FloorSystem	타일·가구·층 관리, A* 네비게이션, 저장/로드

2	BusinessSystem	고객·웨이터·주문 시트 큐, 재고 체크

3	OrderSheet	주문 생성·취소·서빙 상태관리, Food·Inventory 연동, 미리보기 스프라이트

4	GameMode	모드 전환, 영업 타이머

5	Inventory / Food / FoodMenu	돈·재고·만족도, 음식 데이터 래핑·랜덤 주문

6	SaveSlotSystem	슬롯별 JSON 세이브/로드, PlayerPrefs·클라우드(GPGS) 동기화, 디폴트 세이브 배포, 추출 툴

7	EndingSystem	엔딩 조건 체크·씬 전환

8	SceneLoader	인덱스/이름 기반 씬 전환 래퍼

9	LobbySceneSelector	세이브 존재 여부로 프롤로그/메인 씬 결정

10	ObjectScroller	배경 무한 스크롤
