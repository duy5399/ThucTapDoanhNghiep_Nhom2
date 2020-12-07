using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Text;
using System;

public class Launcher_Lobby : MonoBehaviourPunCallbacks
{
    public GameObject loadingScreen;
    public GameObject connectedScreen;
    public GameObject disconnectedScreen;
    public GameObject roomListScreen;
    public GameObject inRoomScreen;
    public GameObject awardScreen;
    public Player player;
    public float timeBack;

    // Start is called before the first frame update
    void Start()
    {
        timeBack = 3.5f;
        roomListScreen = GameObject.Find("Canvas").transform.Find("RoomScreen").transform.Find("RoomList").gameObject;
        //playersListScreen = GameObject.Find("Canvas").transform.Find("RoomScreen").transform.Find("PlayersList").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        inRoomScreen = GameObject.Find("Canvas").transform.Find("RoomScreen").gameObject;
        awardScreen = GameObject.Find("Canvas").transform.Find("AwardScreen").gameObject;
        OnUpdatePlayerList();
        OnBackToTheLobby();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(TypedLobby.Default); //khi kết nối thành công tới Photon Server (Master) thì sẽ kết nối vào Lobby (sảnh chờ) 
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        loadingScreen.SetActive(false);
        disconnectedScreen.SetActive(true); //không kết nỗi bởi lí do gì thì hiện màn hình disconnect
    }

    public override void OnJoinedLobby() //hàm được gọi khi dòng PhotonNetwork.JoinLobby(TypedLobby.Default); thực hiện thành công
    {
        if(loadingScreen.activeSelf)        //activeSelf: đang kích hoạt
            loadingScreen.SetActive(false);
        if (disconnectedScreen.activeSelf)  
            disconnectedScreen.SetActive(false);
        connectedScreen.SetActive(true);    //màn hình chính Lobby sẽ có Create Room và Join Room
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        var listroom = new StringBuilder();
        base.OnRoomListUpdate(roomList);
        foreach (var room in roomList)
        {
            if(room.PlayerCount > 0)
                listroom.Append("Name: " + room.Name + "\t\t" + room.PlayerCount + "/" + room.MaxPlayers + " player");
        }
        roomListScreen.transform.Find("Text").GetComponent<Text>().text = listroom.ToString() + "\n";
        
    }

    public void OnUpdatePlayerList()
    {
        inRoomScreen.transform.Find("PlayersList").transform.Find("TxtRoomName").GetComponent<Text>().text = "Room Name: " + PhotonNetwork.CurrentRoom.Name.ToString();
        var listplayers = new StringBuilder();
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            listplayers.Append(player.NickName + "\n");
        }
        inRoomScreen.transform.Find("PlayersList").transform.Find("TxtPlayersList").GetComponent<Text>().text = listplayers.ToString() + "\n";
    }

    public void OnBackToTheLobby()
    {
        if(awardScreen.activeSelf == true)
        {
            awardScreen.transform.Find("AlertBackTxt").GetComponent<Text>().text = "Back to the lobby in " + Math.Round(timeBack, 0) + " seconds";
            timeBack -= Time.deltaTime;
            if(timeBack <= 0)
            {
                awardScreen.SetActive(false);
                inRoomScreen.SetActive(true);

            }
        }
        else
        {
            timeBack = 3.5f;
            PhotonNetwork.CurrentRoom.IsOpen = true;       //khóa phòng chơi
            PhotonNetwork.CurrentRoom.IsVisible = true;    //ẩn phòng chơi
        }
    }

    public void OnClick_BackToLobbyBtn()  //nút Leave
    {
        awardScreen.SetActive(false);
        inRoomScreen.SetActive(true);
    }
}
