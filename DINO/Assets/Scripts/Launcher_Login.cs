using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Text;

public class Launcher_Login : MonoBehaviourPunCallbacks
{
    public GameObject alertQuit;
    public GameObject setnameScreen;
    public InputField nameTxt;
    public Button connectBtn;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClick_ContinueBtn()   //nút Continue (Set name nhân vật)
    {
        setnameScreen.SetActive(true);
    }

    public void OnClick_ExitBtn()       //nút Quit Game
    {
        alertQuit.SetActive(true);
    }

    public void OnClick_NoBtn()         //nút No
    {
        alertQuit.SetActive(false);
    }

    public void OnClick_YesBtn()        //nút Yes
    {
        Application.Quit();
    }

    public void OnChange_NamePlayer()    //hàm gọi khi giá trị ở ô Text Box nhập tên thay đổi
    {
        if (nameTxt.text.Length > 2)    //kiểm tra tên nhập vào lớn hơn 2 kí tự
        {
            connectBtn.interactable = true; //button Connect sẽ enable //interactable: tương tác
        }
        else
            connectBtn.interactable = false;
    }

    public void OnClick_SetName()       
    {
        PhotonNetwork.NickName = nameTxt.text;
    }

    public void OnClick_ConnectBtn()
    {
        PhotonNetwork.ConnectUsingSettings(); //khi nhấn Connect_button sẽ kết nối tới Photon Server theo App ID tại PhotonServerSettings
        PhotonNetwork.LoadLevel(1);
    }

}
