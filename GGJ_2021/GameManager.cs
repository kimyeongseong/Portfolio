using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{

    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                var obj = FindObjectOfType<GameManager>();
                if (obj != null)
                {
                    instance = obj;
                }
                else
                {
                    var newSingleton = new GameObject("GameManager").AddComponent<GameManager>();
                    instance = newSingleton;
                }
            }
            return instance;
        }
        private set
        {
            instance = value;
        }
    }
    /// <summary>
    /// 다른 매니저 변수들
    /// </summary>
    public InGameUI inGameUI;
    public Playermg playermg;
    /// <summary>
    /// 리스트 변수들
    /// </summary>
    public List<PlayerManager> playerList;
    private PlayerManager playerCtrl;
    public GameObject playerCross;
    public GameObject playerPrefab;
    public List<GameObject> objectList;
    public List<Vector3> spawn_Psos;
    /// <summary>
    /// 인게임 변수
    /// </summary>
    public GameObject hunter_obj;
    [SerializeField]
    private float waitTime;
    [SerializeField]
    private float readyTime;
    [SerializeField]
    private float gameTime;
    // public int playerCount=0;
    public PhotonView photonview;

    [SerializeField]
    public Text debugText;
    [SerializeField]
    public Text TimeText;
    [SerializeField]
    public Text runnerCountText;
    private bool isLife;
    private bool isReady; public bool IsReady => isReady;
    private bool isHuntStart; public bool IsHuntStart => isHuntStart;
    private int hunterCount;
    private int runnerCount;

    [PunRPC]
    public void SetGameStart()
    {
        if (playerCtrl == null)
        {
            Crt_Player();
        }
        Debug.Log("hellof");
        //todo 인원수 4명이 되면 실행되도록 수정할것
        if (PhotonNetwork.IsMasterClient && isReady == false)
        {
            debugText.text = "마스터";
            waitTime = 3;
        }
        else
        {
            debugText.text = "게스트";
        }

    }
    public override void OnLeftRoom()
    {
        photonview.RPC("playerOutRoom", RpcTarget.All, playerCtrl.ViewID);
        if (photonView.IsMine)
        {
            PhotonNetwork.LeaveLobby();
        }
    }
    [PunRPC]
    public void playerOutRoom(int id)
    {
        Debug.Log("누가 나갔어" + id);

        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i].ViewID == id)
            {
                PlayerManager outPlayer = playerList[i];
                playerList.Remove(outPlayer);
                if (PhotonNetwork.IsMasterClient)
                {
                    if (id == huntID)
                    {
                        hunterDie();
                    }
                    else
                    {
                        runnerDie();
                    }
                    PhotonNetwork.Destroy(outPlayer.gameObject);

                }
                break;
            }
        }

    }
    [SerializeField]
    private GameObject fadeObj;
    [PunRPC]
    public void huntFade(bool isFade)
    {
        if (huntID == playerCtrl.ViewID)
        {
            fadeObj.SetActive(isFade);
        }
    }


    /// <summary>
    /// 네트워크 플레이어 유저 생성 및 리스트에 추가
    /// </summary>
    public void Crt_Player()
    {
        GameObject Player_Prb = PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity);
        playerCtrl = Player_Prb.GetComponent<PlayerManager>();
        //   photonview.RPC("set_Player", RpcTarget.All, Player_Prb); 안되는 방법
    }

    // [PunRPC]
    public void set_Player(PlayerManager otherPlayer)
    {
        Debug.Log("플레이어 생성");
        playerList.Add(otherPlayer);

        if (PhotonNetwork.IsMasterClient)
        {
            waitTime = 20;

            if (isLife == false && playerList.Count >= 2)
            {
                isLife = true;
                StartCoroutine(gameCycle());
            }
        }

    }

    /// <summary>
    /// 게임 오버시 호출 함수
    /// </summary>
    public void SetGameOver()
    {

    }
    /// <summary>
    /// 코루틴
    /// </summary>
    /// <param name="limit"></param>제한시간
    /// <returns></returns>
    //[PunRPC]
    IEnumerator gameCycle()
    {
        while (playerList.Count > 1)
        {
            runnerCount = hunterCount = 0;
            //다른 플레이어 대기시간 3초
            Debug.Log("사잌클 시작");
            while (waitTime > 0)
            {
                yield return new WaitForEndOfFrame();
                waitTime -= Time.deltaTime;
            }
            Debug.Log("헌터설정");
            hunterCount = 1;
            runnerCount = playerList.Count - 1;
            runnerCountText.text = $"{runnerCount} 남음";
            int huntId = playerList[(int)(Random.Range(0f, playerList.Count * 2f) * 0.5f)].ViewID;
            photonView.RPC("selectHunt", RpcTarget.All, huntId);


            readyTime = 10;//술래정하고 5초뒤 시작
            while (readyTime > 0)
            {
                yield return new WaitForEndOfFrame();
                readyTime -= Time.deltaTime;
            }
            photonView.RPC("gameStart", RpcTarget.All);
            isHuntStart = true;
            gameTime = 180;//게임시간 180초
            while (gameTime > 0 && isHuntStart)
            {
                yield return new WaitForEndOfFrame();
                gameTime -= Time.deltaTime;
            }
            waitTime = 20;

        }
        isLife = false;

    }
    [PunRPC]
    public void gameStart()
    {
        if (isHunt)
        {
            huntFade(false);
        }
        debugText.text = isHunt ? playerCtrl.Hp.hp.ToString() : "도망자";
    }
    private bool isHunt; public bool IsHunt => huntID == playerCtrl.ViewID;
    public int huntID;
    [PunRPC]
    public void selectHunt(int huntNum)
    {
        isReady = true;
        huntID = huntNum;
        isHunt = playerCtrl.ViewID == huntNum;
        debugText.text = isHunt ? "헌터" : "도망자";
        playerCtrl.Hp.hp = isHunt ? 8 + playerList.Count * 2 : 10;

        playerCross.SetActive(isHunt);
        //    photonView.RPC("huntFadeOn", RpcTarget.All, true);
        huntFade(IsHunt);
        changeObject();
    }

    public void changeObject()
    {
        if (isHunt == false)
        {
            playerCtrl.changeModel();
        }
    }
    [PunRPC]
    public void changeModel(int viewNum, int modelNum)
    {
        if (playerCtrl.ViewID != viewNum)
        {
            for (int i = 0; i < playerList.Count; i++)
            {
                if (playerList[i].ViewID == viewNum)//다른사람 눈에 체인지 모델
                {
                    Debug.Log($"{playerList[i].ViewID} 모델 변경 : {viewNum}");
                    playerList[i].changeModel(modelNum);
                }
            }
        }
    }
    [PunRPC]
    public void deathPlayer(int viewNum)
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i].ViewID == viewNum)
            {
                playerList[i].death();
                if (huntID == viewNum)
                {
                    hunterDie();
                }
                else
                {
                    runnerDie();
                }
                break;
            }
        }
    }
    // [PunRPC]
    public void runnerDie()
    {
        Debug.Log("런너 사망 왔어");
        if (PhotonNetwork.IsMasterClient)
        {
            --runnerCount;
            if (runnerCount <= 0)
            {
                //게임패배
                photonview.RPC("gameOver", RpcTarget.All);
                gameEnd();

            }
        }
    }
    //[PunRPC]
    public void hunterDie()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            --hunterCount;
            if (PhotonNetwork.IsMasterClient)
            {
                //게임 승리
                photonview.RPC("gameClear", RpcTarget.All);
                gameEnd();

            }
        }
    }
    [PunRPC]
    public void holdPlayer(int viewNum)
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i] != playerCtrl && playerList[i].ViewID == viewNum)
            {
                playerList[i].modelHold();
                break;
            }
        }
    }

    [PunRPC]
    public void gameOver()
    {
        if (IsHunt)
        {
            debugText.text = "헌터 승리!";
        }
        else
        {
            debugText.text = "도망자 패배...";
        }
        isReady = false;
        isHunt = false;
        for (int i = 0; i < playerList.Count; i++)
        {
            playerList[i].defualtModel();
        }
    }
    [PunRPC]
    public void gameClear()
    {
        if (IsHunt)
        {
            debugText.text = "헌터 패배...";
        }
        else
        {
            debugText.text = "도망자 승리!";
        }
        isReady = false;
        isHunt = false;
        for (int i = 0; i < playerList.Count; i++)
        {
            playerList[i].defualtModel();
        }
    }
    public void gameEnd()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            isHuntStart = false;
        }
    }


    /// <summary>
    /// 게임 시작 체크 사람 수 와 대기 상태 일 때
    /// </summary>
    /// <returns></returns>

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(waitTime);
            stream.SendNext(readyTime);
            stream.SendNext(gameTime);

            stream.SendNext(isHuntStart);
            stream.SendNext(isReady);
            stream.SendNext(isLife);

            stream.SendNext(runnerCount);
            stream.SendNext(hunterCount);
        }
        //클론이 통신을 받는 
        else
        {
            waitTime = (float)stream.ReceiveNext();
            readyTime = (float)stream.ReceiveNext();
            gameTime = (float)stream.ReceiveNext();

            isHuntStart = (bool)stream.ReceiveNext();
            isReady = (bool)stream.ReceiveNext();
            isLife = (bool)stream.ReceiveNext();

            runnerCount = (int)stream.ReceiveNext();
            hunterCount = (int)stream.ReceiveNext();
        }
        //통신을 보내는 
        if (isHuntStart)
        {
            TimeText.text = $"{gameTime:N2}";
        }
        else if (isReady)
        {
            TimeText.text = $"준비 {readyTime:N2}";
        }
        else if (isLife)
        {
            TimeText.text = $"대기 {waitTime:N2}";
        }
        runnerCountText.text = $"{runnerCount} 남음";


    }
}