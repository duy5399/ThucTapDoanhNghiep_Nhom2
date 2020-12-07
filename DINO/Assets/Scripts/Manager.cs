using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Manager : MonoBehaviour
{
    public GameObject alertQuit;

    // Start is called before the first frame update
    void Start()
    {
        //SpawnPlayer();
    }

    public void OnClick_LeaveBtn()  //nút Leave
    {
        //PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel(2);
    }

    public void OnClick_ExitBtn()   //nút Quit Game
    {
        alertQuit.SetActive(true);
    }

    public void OnClick_NoBtn()     //nút No
    {
        alertQuit.SetActive(false);
    }

    public void OnClick_YesBtn()    //nút Yes
    {
        Application.Quit();         //đóng chương trình
    }

    public void OnClick_CountinueBtn()  //nút Continue
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel(1);
    }

}
