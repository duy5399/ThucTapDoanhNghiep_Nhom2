using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class UIhandler : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public GameObject roomListScreen;
    public GameObject alertQuit;
    public InputField createRoomTxt;
    public InputField joinRoomTxt;
    public Button createBtn;
    public Button joinBtn;
    public Button startBtn;
    public Button leaveBtn;
    public Button backToLobbyBtn;

    public void SpawnPlayer()  //tạo - sản sinh nhân vật khi vào game 
    {
        Vector3 position = new Vector3(Random.Range(-8f, 10f), 7, 0);
        PhotonNetwork.Instantiate(playerPrefab.name, position, playerPrefab.transform.rotation);    //Instantiate: khởi tạo
    }

    public void OnClick_LeaveBtn()  //nút Leave
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel(1);
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
        Application.Quit();
    }

    public void OnClick_ReconnectBtn()
    {
        PhotonNetwork.ConnectUsingSettings(); //khi nhấn Connect_button sẽ kết nối tới Photon Server theo App ID tại PhotonServerSettings
        PhotonNetwork.LoadLevel(1);
    }

    public void OnChange_CreateNameRoom()    //hàm gọi khi giá trị ở ô Text Box nhập tên phòng thay đổi (tạo phòng)
    {
        if (createRoomTxt.text.Length > 0)    //kiểm tra tên nhập vào lớn hơn 2 kí tự
        {
            createBtn.interactable = true; //button CreateRoomBtn sẽ enable //interactable: tương tác
        }
        else
            createBtn.interactable = false;
    }

    public void OnChange_JoinNameRoom()    //hàm gọi khi giá trị ở ô Text Box nhập tên phòng thay đổi (tạo phòng)
    {
        if (joinRoomTxt.text.Length > 0)    //kiểm tra tên nhập vào lớn hơn 2 kí tự
        {
            joinBtn.interactable = true; //button CreateRoomBtn sẽ enable //interactable: tương tác
        }
        else
            joinBtn.interactable = false;
    }

    public void OnClick_CreateRoom()    //xảy ra khi nhấn Create Room Button
    {
        PhotonNetwork.CreateRoom(createRoomTxt.text, new RoomOptions { MaxPlayers = 10 }, null);  //tạo phòng mới với tên nhập ở TextBox với số lượng người chới tối đa là 10    
    }

    public void OnClick_JoinRoom()  //xảy ra khi nhấn Join Room Button
    {
        PhotonNetwork.JoinRoom(joinRoomTxt.text, null); //tham gia phòng với tên nhập ở TextBox
    }

    public override void OnJoinedRoom()
    {
        //print("Room Joned Sucess");
        PhotonNetwork.LoadLevel(2); //load tới SampleScene
    }

    public void OnClick_StartMatch()    //xảy ra khi nhấn Create Room Button
    {
        SpawnPlayer();
        roomListScreen = GameObject.Find("Canvas").transform.Find("RoomScreen").gameObject;
        roomListScreen.SetActive(false);
        //PhotonNetwork.LoadLevel(3);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        print("Room Failed " + returnCode + " Message " + message); //hàm check báo JoinRoomFailed khi chạy thử game
    }

    public void OnClick_BackToLobbyBtn()  //nút Leave
    {
        PhotonNetwork.LoadLevel(1);
    }
}
