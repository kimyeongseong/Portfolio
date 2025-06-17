# Portfolio
DangerOutside
https://youtu.be/YmX-osyXW1

  1	PlayFab 이메일 로그인/회원가입	EditorLoginManager.Login(), Register(), GetServerDatas()

  2	STAGE 저장 및 불러오기 정보	EditorManager 전반 – Start(), OnSaveStage(), StageDropdownChanged() 등

  3	STAGE 맵 타일 배치 및 초기 설정 세팅	EditorTileManager.SetTiles(int x,int y) + SetCamera()

  4	에디터 실시간 상호작용	EditorTileManager.SetCheckerPos() / FindEmptyTiles()

  5	건물 배치	EditorBuildingManager의 OnBuildingCreate(), OnMouseDrag(), OnMouseUp(), OnDetail()

  6	유닛 배치 & 속성 편집	EditorCitizenManager의 OnCitizenCreate(), 드래그 루틴, OnDetail()

  7	타일 속성 변경	EditorTileManager.OnTileColorClick() / SetColorChange()

  8	스테이지 데이터 로드&동기화	EditorDataBase (싱글턴 패턴)

