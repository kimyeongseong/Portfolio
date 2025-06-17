using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public bool GameStart;
    [SerializeField]
    private Transform cameraArm;
    [SerializeField]
    private Transform characterBody;
    [SerializeField]
    private GameObject[] changeModels;
    private Camera myCamera;
    Common player = new Common();
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private Transform modelParent;
    [SerializeField]
    private HpCtrl hpCtrl; public HpCtrl Hp => hpCtrl;

    public int ViewID => photonView.ViewID;
    private void Awake()
    {
        GameManager.Instance.set_Player(this);
        if (photonView.IsMine)
        {
            //  animator = GetComponentinchi<Animator>();
            myCamera = Camera.main;
            myCamera.gameObject.transform.SetParent(cameraArm.transform.GetChild(0));
            myCamera.gameObject.transform.localPosition = Vector3.zero;
            myCamera.gameObject.transform.localEulerAngles = Vector3.zero;
            Cursor.visible = false;                     //마우스 커서가 보이지 않게 함
            Cursor.lockState = CursorLockMode.Locked;   //마우스 커서를 고정시킴
        }
    }
    private void Start()
    {
        if (photonView.IsMine)
        {
            StartCoroutine(Player_Update());
        }
    }
    private bool isLock = false;
    private bool isLife = false;
    IEnumerator Player_Update()
    {
        while (true)
        {
            if (photonView.IsMine)
            {
                LookAround();
                Move();
                if (isLife)
                {
                    inputMouse0();
                    if (Input.GetKeyDown(KeyCode.Backspace))
                    {
                        //death();
                    }
                }
            }
            yield return null;
        }
    }
    public void death()
    {
        if (photonView.IsMine)
        {
            isLife = false;
            isLock = false;
            if (GameManager.Instance.IsHunt == false)
            {
                this.transform.SetPositionAndRotation(modelParent.position, modelParent.rotation);
                modelParent.SetParent(this.transform);
                modelParent.localPosition = Vector3.zero;
                modelParent.localEulerAngles = Vector3.zero;

                //    GameManager.Instance.photonView.RPC("runnerDie", RpcTarget.All);
            }
            else
            {
                //  GameManager.Instance.photonView.RPC("hunterDie", RpcTarget.All);
            }
            GameManager.Instance.debugText.text = "사 망";
        }
        else if (GameManager.Instance.IsHunt == false)
        {
            isLock = false;
            this.transform.SetPositionAndRotation(modelParent.position, modelParent.rotation);
            modelParent.SetParent(this.transform);
            modelParent.localPosition = Vector3.zero;
            modelParent.localEulerAngles = Vector3.zero;
        }
        modelParent.gameObject.SetActive(false);
    }
    public void killHunt()
    {
        GameManager.Instance.photonView.RPC("deathPlayer", RpcTarget.All, ViewID);
        //GameManager.Instance.photonView.RPC("killPlayer", RpcTarget.All, ViewID);
        //modelParent.gameObject.SetActive(false);
    }
    private void inputMouse0()
    {
        if (GameManager.Instance.huntID != photonView.ViewID && GameManager.Instance.IsReady && Input.GetMouseButtonDown(0))
        {
            isLock = !isLock;
            GameManager.Instance.debugText.text = isLock ? "잠김" : "풀림";
            if (isLock)
            {
                modelParent.SetParent(null);
            }
            else
            {
                this.transform.SetPositionAndRotation(modelParent.position, modelParent.rotation);
                modelParent.SetParent(this.transform);
                modelParent.localPosition = Vector3.zero;
                modelParent.localEulerAngles = Vector3.zero;
            }
            GameManager.Instance.photonView.RPC("holdPlayer", RpcTarget.All, ViewID);
        }
    }
    public void modelHold()
    {
        isLock = !isLock;
        if (isLock)
        {
            modelParent.SetParent(null);
        }
        else
        {
            this.transform.SetPositionAndRotation(modelParent.position, modelParent.rotation);
            modelParent.SetParent(this.transform);
            modelParent.localPosition = Vector3.zero;
            modelParent.localEulerAngles = Vector3.zero;
        }
    }
    public void changeModel()
    {
        //  Debug.Log("모델 변경");

        int changeNum = Random.Range(0, changeModels.Length);

        if (photonView.IsMine)
        {
            isLife = true;
            modelParent.gameObject.SetActive(true);

            GameManager.Instance.photonView.RPC("changeModel", RpcTarget.All, ViewID, changeNum);
        }
        changeModel(changeNum);
    }
    public void changeModel(int changeNum)
    {
        characterBody.gameObject.SetActive(false);
        modelParent.gameObject.SetActive(true);
        for (int i = 0; i < changeModels.Length; i++)
        {
            changeModels[i].SetActive(i == changeNum ? true : false);
        }
        if (isLock || modelParent.parent == null)
        {
            isLock = false;
            this.transform.SetPositionAndRotation(modelParent.position, modelParent.rotation);
            modelParent.SetParent(this.transform);
            modelParent.localPosition = Vector3.zero;
            modelParent.localEulerAngles = Vector3.zero;
        }
    }

    public void defualtModel()
    {
        characterBody.gameObject.SetActive(true);
        for (int i = 0; i < changeModels.Length; i++)
        {
            changeModels[i].SetActive(false);
        }
    }

    private void LookAround()
    {
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        Vector3 camAngle = cameraArm.rotation.eulerAngles;
        float x = camAngle.x - mouseDelta.y;
        if (x < 120f)
            x = Mathf.Clamp(x, -35f, 70f);
        else
            x = Mathf.Clamp(x, 325, 380f);
        cameraArm.localRotation = Quaternion.Euler(x, 0, 0);
        this.transform.rotation = Quaternion.Euler(0, this.transform.eulerAngles.y + mouseDelta.x, 0);
    }

    private void OnCollisionEnter(Collision collision)
    {
        player._isground = true;
    }
    private void Move()
    {
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        bool isMove = moveInput.magnitude != 0;
        if (player._isground)
            animator.SetBool("isMove", isMove);
        if (isMove)
        {
            Vector3 lookForward = new Vector3(cameraArm.forward.x, 0f, cameraArm.forward.z).normalized;
            Vector3 lookRight = new Vector3(cameraArm.right.x, 0f, cameraArm.right.z).normalized;
            Vector3 moveDir = lookForward * moveInput.y + lookRight * moveInput.x;
            characterBody.forward = lookForward;
            transform.position += moveDir * Time.deltaTime * player.speed;
        }
        if (Input.GetKeyDown(KeyCode.Space) && player._isground)
        {
            player._isground = false;
            GetComponent<Rigidbody>().AddForce(Vector3.up * player.jumpforce, ForceMode.Impulse);
        }
    }
    private void OnApplicationQuit()
    {
 //       killHunt();
    }
}
