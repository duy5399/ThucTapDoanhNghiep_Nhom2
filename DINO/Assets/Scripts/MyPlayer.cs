using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System;
using System.Text;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityStandardAssets.CrossPlatformInput;

public class MyPlayer : MonoBehaviourPun, IPunObservable
{
    public PhotonView pv;   //đối tượng trên mạng

    public bool isGrounded;

    private Animator anim;

    public AudioSource[] musicSource;

    public Joystick joystick;     // 
    public float moveSpeed = 3f;  //tốc độ di chuyển      
    public float jumpForce = 475;   //độ nhảy

    private Vector3 smoothMove;     //sẽ dùng gán giá trị vị trí di chuyển của người chơi khác
    private Quaternion smoothMoveRotation;  //dùng để gán vị trí xoay mặt của người chơi khác

    private GameObject sceneCamera; //Main Camera
    public GameObject playerCamera; //Player Camera

    public SpriteRenderer sr;
    public Rigidbody2D rb;          

    public Text nameText;           //gán tên của nhân vật vào UI Text này

    public float offset;

    public float kickDelay = 0.3f;
    public bool kicking = false;

    public GameObject kickPrefab;
    public Transform kickSpawnRight;   //vị trí Kick xuất hiện bên phải
    public Transform kickSpawnLeft;   //vị trí Kick xuất hiện bên trái

    private bool isDead = false;

    private float smoothHeal;
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public Image blood;             //máu của nhân vật

    public float timeRespawn = 2f;  //thời gian hổi sinh

    public GameObject alertDeath;  //bảng thông báo chết và hồi sinh

    public int playerCount;         //số người chơi có trong phòng
    public GameObject scoreBoard;   //bảng điểm

    public GameObject menu;

    public GameObject awardScreen;

    // Start is called before the first frame update
    void Start()
    {
        //PhotonNetwork.SendRate = 20;          //Xác định số lần PhotonNetwork gửi đi 1 gói dữ liệu- càng cao càng ít độ trễ cho game nhưng tốn tài nguyên
        //PhotonNetwork.SerializationRate = 15; //xác định số lần hàm OnPhotonSerialize được gọi - càng cao càng ít độ trễ cho game nhưng tốn tài nguyên
        if (photonView.IsMine)  //PhotonView: đối tượng trên mạng (xác định bằng viewID)
        {
            isGrounded = true;
            rb = GetComponent<Rigidbody2D>();
            PhotonNetwork.LocalPlayer.SetScore(0);      //khởi tạo điểm nhân vật bằng 0
            nameText.text = PhotonNetwork.NickName;     //hiện tên Player của mình
            nameText.color = Color.yellow;

            anim = GetComponent<Animator>();
            joystick = FindObjectOfType<Joystick>();    //tìm Fixed Joytick gán vào joystick

            alertDeath = GameObject.Find("Canvas").transform.Find("AlertDeath").gameObject;    //tìm UI AlertDeath gán vào alertScreen
            alertDeath.SetActive(false);
            scoreBoard = GameObject.Find("Canvas").transform.Find("ScoreBoard").gameObject;     //tìm UI ScoreBoard gán vào scoreBoard
            menu = GameObject.Find("Canvas").transform.Find("Menu").gameObject;                 //tìm UI Menu gán vào menu
            //awardScreen = GameObject.Find("Canvas").transform.Find("AwardScreen").gameObject;

            //winnerScreen = GameObject.Find("Canvas").transform.Find("AlertWinner").gameObject;

            //Camera tập trung vào Player
            playerCamera = GameObject.Find("Main Camera");   //tìm đối tượng Main Camera

            sceneCamera.SetActive(false);   //tắt Main Camera   
            playerCamera.SetActive(true);   //bật Player Camera


        }
        else
        {
            nameText.text = pv.Owner.NickName;  //pv: 1 đối tượng trên mạng qua PhontonView - bất cứ ai là chủ sở hữu pv thì sẽ lấy nametext của pv đó
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(photonView.IsMine)  //PhotonView: đối tượng trên mạng (xác định bằng viewID)
        {
            CheckDead();
            ProcessInputs();    //hàm di chuyển nhân vật
            Jump();             //hàm nhảy
            Kick();             //hàm tấn công (Kick)
            Respawn();          //hàm chết và hồi sinh
            ShowScore();        //hàm để xem bảng điểm
            ShowMenu();
            Victory();
            //pv.RPC("Victory", RpcTarget.OthersBuffered);
        }
        else
        {
            SmoothMovement();   //nếu không phải người chơi cục bộ - PC1 có Player Y và Duy không điều khiển Player Y mà là một người khác (Hoài)
                                //=>vị trí Player Y sẽ di chuyển từ một người khác (Hoài điều khiển từ PC2)
            SmoothHealth();
        }
    }    

    private void SmoothMovement()   //hàm cập nhập vị trí của người chơi khác khi smoothMove và smoothMoveRotation  được update vị trí mới từ hàm OnPhotonSerializeView();
    {
        //transform: biến đổi, thay đổi
        //position: vị trí
        //rotation: xoay
        transform.position = Vector3.Lerp(transform.position, smoothMove, Time.deltaTime * 10);
        //transform.rotation = Quaternion.Lerp(transform.rotation, smoothMoveRotation, Time.deltaTime * 10);  //Trong Unity, Quaternion được sử dụng để biểu diễn phép quay của mọi đối tượng
    }

    private void SmoothHealth()   //hàm cập nhập vị trí của người chơi khác khi smoothMove và smoothMoveRotation  được update vị trí mới từ hàm OnPhotonSerializeView();
    {
        blood.fillAmount = smoothHeal/maxHealth;    //khi nhân vận tốc với Time.delta thì object di chuyển ko phụ thuộc vào FPS của game
    }

    private void ProcessInputs()    //hàm điều khiển nhân vật di chuyển
    {
        var moveHorizontal = new Vector3(joystick.Horizontal, 0);    //di chuyển theo chiều ngang //GetAxisRaw: trả về -1 0 1
        transform.position += moveHorizontal * moveSpeed * Time.deltaTime;      //Time.delta là khoảng thời gian giữa 2 frame
        if(joystick.Horizontal >= 0.1f)
        {
            sr.flipX = false;
            pv.RPC("OnDirectionChange_RIGHT", RpcTarget.OthersBuffered);
        }
        if (joystick.Horizontal <= -0.1f)
        {
            sr.flipX = true;
            pv.RPC("OnDirectionChange_LEFT", RpcTarget.OthersBuffered);
        }
        anim.SetFloat("Speed", Mathf.Abs(joystick.Horizontal));
        //var moveVertical = new Vector3(0, Input.GetAxisRaw("Vertical"));        //di chuyển theo chiều dọc
        //transform.position += moveVertical * moveSpeed * Time.deltaTime;

        //Vector3 difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;  //tính vị trí giữa con trỏ và nhân vật
        //float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;  //Mathf.Atan2: trả về số đo góc được tính bằng đơn vị radians sao cho tan của góc đó chính bằng thương số của hai tham số truyền vào
        //transform.rotation = Quaternion.Euler(0f, 0f, rotationZ + offset);  //nếu không thêm offset thì nhân vật xoay mặt theo trục x


    }

    [PunRPC]
    public void OnDirectionChange_LEFT()
    {
        sr.flipX = true;
    }

    [PunRPC]
    public void OnDirectionChange_RIGHT()
    {
        sr.flipX = false;
    }

    void OnCollisionEnter2D(Collision2D collision)  //di chuyển trên mặt đất
    {
        if (collision.gameObject.tag == "Ground")
        {
            if (photonView.IsMine)
            {
                isGrounded = true;
            }
        }
        //if (collision.gameObject.name == "AKick(Clone)")           //đối tượng va chạm là đạn
        //{
        //    Destroy(collision.gameObject);          //phá hủy đạn
        //    if (photonView.IsMine)
        //    {
        //        musicSource[1].Play();
        //        pv.RPC("TakeDamage", RpcTarget.AllBuffered);    //gọi hàm trừ máu nhân vật //gọi hàm từ xa cho các máy khác (others)
        //        //Gửi RPC cho những người khác và thực hiện nó ngay lập tức trên máy khách này.Người chơi mới nhận được RPC khi họ tham gia khi được đệm(cho đến khi khách hàng này rời đi).
        //        if (blood.fillAmount <= 0f)
        //        {
        //            Add_Score(collision.gameObject);
        //        }
        //    }
        //}
    }

    void OnCollisionExit2D(Collision2D collision)   //khi chân không chạm mặt đất
    {
        if (collision.gameObject.tag == "Ground")
        {
            if (photonView.IsMine)
            {
                isGrounded = false;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "AKick")          //đối tượng va chạm là đạn
        {
            if (photonView.IsMine && collision.gameObject.GetComponent<PhotonView>().Owner != photonView.Owner)
            {
                musicSource[1].Play();
                TakeDamage();
                //pv.RPC("TakeDamage", RpcTarget.AllBuffered);    //gọi hàm trừ máu nhân vật //gọi hàm từ xa cho các máy khác (others)
                //Gửi RPC cho những người khác và thực hiện nó ngay lập tức trên máy khách này.Người chơi mới nhận được RPC khi họ tham gia khi được đệm(cho đến khi khách hàng này rời đi).
                if (blood.fillAmount <= 0f)
                {
                    AddScore(collision.gameObject);
                }
            }
        }

        if(collision.gameObject.tag == "DeathZone")          //đối tượng va chạm là đáy
        {
            if (photonView.IsMine)
            {
                musicSource[1].Play();
                isDead = true;
                if(photonView.Owner.GetScore() > 0)
                {
                    int temp = photonView.Owner.GetScore();
                    photonView.Owner.SetScore(temp-1);
                }
            }
            
        }
    }


    public void Jump()
    {
        if (CrossPlatformInputManager.GetButtonDown("Jump") && isGrounded == true)
        {
            rb.AddForce(Vector2.up * jumpForce);
        }
        anim.SetBool("Grounded", isGrounded);
    }

    public void Kick()
    {
        GameObject AKick;
        if (CrossPlatformInputManager.GetButtonDown("Kick") && !kicking)
        {
            if (sr.flipX == true)
            {
                AKick = PhotonNetwork.Instantiate(kickPrefab.name, kickSpawnLeft.position, Quaternion.identity);   //Instantiate: khởi tạo Bullet
                AKick.GetComponent<PhotonView>().RPC("changeDirection", RpcTarget.AllBuffered);
            }
            else
            {
                AKick = PhotonNetwork.Instantiate(kickPrefab.name, kickSpawnRight.position, Quaternion.identity);   //Instantiate: khởi tạo Bullet
            }
            kicking = true;
            musicSource[0].Play();
            kickDelay = 0.3f;
            anim.SetBool("Kick", true);
        }
        if (kicking)
        {
            if(kickDelay > 0)
            {
                kickDelay -= Time.deltaTime;
            }
            else
            {
                kicking = false;
            }
        }
        anim.SetBool("Kick", kicking);
    }

    //[PunRPC]    //Remote Procedure Calls - gọi các thủ tục từ xa
    public void TakeDamage()                        //hàm trừ máu 
    {
        currentHealth -= 10f;
        blood.fillAmount = currentHealth/maxHealth;    //khi nhân vận tốc với Time.delta thì object di chuyển ko phụ thuộc vào FPS của game
    }

    public void AddScore(GameObject a)
    {
        a.gameObject.GetPhotonView().Owner.AddScore(1); //chủ sở hữu của viên đạn được cộng điểm
    }

    public void CheckDead()
    {
        if(blood.fillAmount <= 0f)  //khi máu về 0
            isDead = true;
    }

    public void Respawn()     //hàm để giết(xóa nhân vật) và hồi sinh
    {
        if(isDead)      //khi trong trạng thái chết
        {
            alertDeath.SetActive(true);    //bật thông báo chết và đang hồi sinh
            alertDeath.transform.Find("Text").GetComponent<Text>().text ="YOU DIED\n" +"Respawning " + Math.Round(timeRespawn, 1) +" seconds";
            gameObject.transform.position = new Vector3(0, 500, 0);     //đưa nhân vật ra khỏi màn hình không thể nhìn
            timeRespawn -= Time.deltaTime;
            if (timeRespawn < 0)        //khi thời gian hồi sinh hết
            {
                alertDeath.SetActive(false);   //tắt thông báo chết và đang hồi sinh                    //đặt lại đạn cho nhân vật
                Vector3 Position = new Vector3(UnityEngine.Random.Range(-8f, 10f), 7, 0);  //đưa nhân vật vào lại màn hình (vị trí ngẫu nhiên)
                gameObject.transform.position = Position;
                timeRespawn = 2f;               //đặt lại thời gian hồi sinh 
                pv.RPC("ResetBlood", RpcTarget.AllBuffered);               
            }
        }
    }


    [PunRPC]
    public void ResetBlood()  //hàm đặt lại máu của nhân vật
    {
        isDead = false;
        currentHealth = 100f;
        blood.fillAmount = 1f;
    }

    public void ShowScore()    //hàm để xem bảng điểm
    {
        UpdateScoreBoard();     //gọi hàm cập nhật bảng điểm
        if (Input.GetKeyDown(KeyCode.Tab))  //nhấn giữ tab hiện bảng điểm
        {
            scoreBoard.SetActive(true);
        }
        else if(Input.GetKeyUp(KeyCode.Tab))     //thả tab ẩn bảng điểm
        {
            scoreBoard.SetActive(false);
        }
    }
    
    public void UpdateScoreBoard()      //hàm cập nhật bảng điểm
    {
        playerCount = PhotonNetwork.PlayerList.Length;      //đếm số lượng người chơi trong phòng
        
        var playerList = new StringBuilder();   //StringBuilder là kiểu đối tượng động, cho phép bạn mở rộng số lượng kí tự của một chuỗi. Khi bạn thay đổi nội dung chuỗi, nó không tạo một đối tượng mới trong bộ nhớ giống kiểu String, mà nó mở rộng bộ nhớ để lưu trữ giá trị chuỗi mới thay thế.
        //Đặc điểm của StringBuilder là:
        //Cho phép thao tác trực tiếp trên chuỗi ban đầu.
        //Có khả năng tự mở rộng vùng nhớ khi cần thiết.
        foreach (var player in PhotonNetwork.PlayerList)    //lấy từng người chơi trong phòng
        {
            //append: chắp thêm
            playerList.Append("Name: " + player.NickName + "\tScore: "  + player.GetScore() + "\n");    //lấy tên và điểm số của người chơi gán vào playerList (StringBuilder)
        }
        scoreBoard.transform.Find("Text").GetComponent<Text>().text = "Player Online: " + playerCount.ToString() +"\n" + playerList.ToString();   //tìm Text và in số nhân vật đếm được
    }

    public void ShowMenu()     //hàm hiện Menu (rời khỏi phòng - thoát game)
    {
        if (Input.GetKeyDown(KeyCode.Escape))   //nhấn giữ Escape hiện Menu
        {
            menu.SetActive(true);
        }
        else if (Input.GetKeyUp(KeyCode.Escape))    //thả Escape ẩn Menu
        {
            menu.SetActive(false);
        }
    }

    [PunRPC]
    public void SetAward(string nameWinner)
    {
        awardScreen = GameObject.Find("Canvas").transform.Find("AwardScreen").gameObject;
        awardScreen.SetActive(true);                 //hiện màn hình giành cho người chiến thắng
        awardScreen.transform.Find("NameWinnerTxt").GetComponent<Text>().text = nameWinner;
        foreach(var player in PhotonNetwork.PlayerList)
        {
            if (player.NickName.ToString() != nameWinner)
            {
                PhotonNetwork.DestroyPlayerObjects(player);
            }
        }
        Destroy(gameObject);
    }

    public void Victory()       //hàm kiểm tra điểm số (người chiến thắng)
    {
        if(photonView.Owner.GetScore() >= 2)    //nếu điểm số đạt số điểm yêu cầu
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;       //khóa phòng chơi
            PhotonNetwork.CurrentRoom.IsVisible = false;    //ẩn phòng chơi   
            //PhotonNetwork.LoadLevel(4);  
            pv.RPC("SetAward", RpcTarget.AllBuffered, photonView.Owner.NickName.ToString());     //gọi hàm cho những người còn lại (người thất bại)
            SetAward(photonView.Owner.NickName);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)  //hàm dùng để gửi vị trí của mình khi di chuyển đến những người chơi khác và nhận vị trí của những người chơi khác khi họ di chuyển nhân vật
    {                                                                               //PhotonStream stream: dữ liệu chúng ta gửi
        if(stream.IsWriting)       //ta gửi (send) dữ liệu - vị trí đi là đang writing
        {
            stream.SendNext(transform.position);
            stream.SendNext(currentHealth);
            //stream.SendNext(transform.rotation);
            //stream.SendNext(transform.localScale);
        }
        else if(stream.IsReading)  //ta nhận (Receive) dữ liệu - vị trí của người khác là đang reading
        {
            smoothMove = (Vector3)stream.ReceiveNext(); //cập nhập mỗi lần và hàm smoothMovement(); sẽ hiện vị trí Player Y (của Hoài điều khiển) lên PC1 của Duy (Duy điều khiển Player X)
            smoothHeal = (float)stream.ReceiveNext();
            //this.transform.localScale = (Vector3)stream.ReceiveNext();
            //smoothMoveRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
