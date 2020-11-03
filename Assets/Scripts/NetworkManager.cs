using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Connection status")]
    [SerializeField] Text connectionStatusText = null;


    [Header("Login UI Panel")]
    [SerializeField] InputField playerNameInput;
    [SerializeField] GameObject loginUIPanel;

    [Header("Game Options UI Panel")]
    [SerializeField] GameObject gameOptionsUIPanel;

    [Header("Inside Room UI Panel")]
    [SerializeField] GameObject insideRoomUIPanel;
    [SerializeField] Text roomInfoText;
    [SerializeField] GameObject playerListPrefab;
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject startGameButton;

    [Header("Room List UI Panel")]
    [SerializeField] GameObject roomListUIPanel;
    [SerializeField] GameObject roomListEntryPrefab;
    [SerializeField] Transform roomListContent;

    [Header("Create Room UI Panel")]
    [SerializeField] GameObject createRoomUIPanel;
    [SerializeField] InputField roomNameInputField;
    [SerializeField] InputField maxPlayerInputField;


    [Header("Join Random Room UI Panel")]
    [SerializeField] GameObject joinRandomRoomUIPanel;

    Dictionary<string, RoomInfo> cachedRoomList;
    Dictionary<string, GameObject> roomListGameObjects;
    Dictionary<int, GameObject> playerListGameObjects;


    #region Unity Methods
    void Start()
    {
        ActivatePanel(loginUIPanel.name);
        cachedRoomList = new Dictionary<string, RoomInfo>();
        roomListGameObjects = new Dictionary<string, GameObject>();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Update()
    {
        connectionStatusText.text = "Connection Status: " + PhotonNetwork.NetworkClientState;
    }
    #endregion

    #region Photon Callbacks
    public override void OnConnected()
    {
        Debug.Log("Connected to Internet");
    }
    public override void OnConnectedToMaster()
    {
        ActivatePanel(gameOptionsUIPanel.name);
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " Connected to Photon servers");
    }
    public override void OnCreatedRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " is created.");
    }
    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " joined to " + PhotonNetwork.CurrentRoom.Name);
        ActivatePanel(insideRoomUIPanel.name);

        UpdateStartButton();

        UpdateRoomText();

        if (playerListGameObjects == null)
        {
            playerListGameObjects = new Dictionary<int, GameObject>();
        }
        UpdatePlayerListView();

    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateRoomText();
        GameObject playerListGameObject = Instantiate(playerListPrefab, playerListContent);
        playerListGameObject.transform.localScale = Vector3.one;

        playerListGameObject.transform.Find("PlayerNameText").GetComponent<Text>().text = newPlayer.NickName;
        playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(newPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);

        playerListGameObjects.Add(newPlayer.ActorNumber, playerListGameObject);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {

        UpdateStartButton();
        UpdateRoomText();
        Destroy(playerListGameObjects[otherPlayer.ActorNumber].gameObject);
        playerListGameObjects.Remove(otherPlayer.ActorNumber);
    }
    public override void OnLeftRoom()
    {
        ActivatePanel(gameOptionsUIPanel.name);
        ClearPlayerListView();
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListView();

        for (int i = 0; i < roomList.Count; i++)
        {
            RoomInfo room = roomList[i];

            bool roomDeleted = !room.IsOpen || !room.IsVisible || room.RemovedFromList;

            if (roomDeleted && cachedRoomList.ContainsKey(room.Name))
            {
                cachedRoomList.Remove(room.Name);
            }
            else
            {
                if (cachedRoomList.ContainsKey(room.Name))
                {
                    cachedRoomList[room.Name] = room; //Updates existing room
                }
                else
                {
                    cachedRoomList.Add(room.Name, room);//Add room to list
                }
            }
        }
        foreach (RoomInfo room in cachedRoomList.Values)
        {
            GameObject roomListEntryGameObject = Instantiate(roomListEntryPrefab, roomListContent);
            roomListEntryPrefab.transform.localScale = Vector3.one;

            roomListEntryGameObject.transform.Find("RoomNameText").GetComponent<Text>().text = room.Name;
            roomListEntryGameObject.transform.Find("RoomPlayersText").GetComponent<Text>().text = room.PlayerCount + "/" + room.MaxPlayers;
            roomListEntryGameObject.transform.Find("JoinRoomButton").GetComponent<Button>().onClick.AddListener(() => OnJoinRoomButtonClicked(room.Name));

            roomListGameObjects.Add(room.Name, roomListEntryGameObject);
        }
    }
    public override void OnLeftLobby()
    {
        ClearRoomListView();
        cachedRoomList.Clear();
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log(message);
        string roomName = "Room" + Random.Range(1000, 10000);

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 20;
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }
    #endregion

    #region UI Callbacks
    public void OnStartGameButtonClicked()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        PhotonNetwork.LoadLevel("GameScene");
    }
    public void OnLoginButtonClicked()
    {
        string playerName = playerNameInput.text;
        if (!string.IsNullOrEmpty(playerName))
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.Log("Player name is invalid");
        }
    }
    public void OnLeaveRoomButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }
    public void OnShowRoomListButtonClicked()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }

        ActivatePanel(roomListUIPanel.name);
    }
    public void OnCancelButtonClicked()
    {
        ActivatePanel(gameOptionsUIPanel.name);
    }
    public void OnCreateRoomButtonClicked()
    {
        string roomName = roomNameInputField.text;
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "Room" + Random.Range(1000, 10000);
        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte)int.Parse(maxPlayerInputField.text);

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }
    public void OnBackButtonClicked()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        ActivatePanel(gameOptionsUIPanel.name);

    }
    public void OnJoinRandomRoomButtonClicked()
    {
        ActivatePanel(joinRandomRoomUIPanel.name);
        PhotonNetwork.JoinRandomRoom();
    }
    #endregion

    #region Private Methods
    private void UpdateStartButton()
    {
        startGameButton.SetActive(PhotonNetwork.LocalPlayer.IsMasterClient);
    }
    private void UpdateRoomText()
    {
        roomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name +
                    " Players/Max.players: " +
                    PhotonNetwork.CurrentRoom.PlayerCount + "/" +
                    PhotonNetwork.CurrentRoom.MaxPlayers;
    }
    private void ClearPlayerListView()
    {
        foreach (GameObject playerListGameObject in playerListGameObjects.Values)
        {
            Destroy(playerListGameObject);

        }
        playerListGameObjects.Clear();
        playerListGameObjects = null;
    }
    private void UpdatePlayerListView()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Player player = PhotonNetwork.PlayerList[i];

            GameObject playerListGameObject = Instantiate(playerListPrefab, playerListContent);
            playerListGameObject.transform.localScale = Vector3.one;

            playerListGameObject.transform.Find("PlayerNameText").GetComponent<Text>().text = player.NickName;
            playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);

            playerListGameObjects.Add(player.ActorNumber, playerListGameObject);

        }
    }
    private void ClearRoomListView()
    {
        foreach (GameObject roomListGameObject in roomListGameObjects.Values)
        {
            Destroy(roomListGameObject);
        }
        roomListGameObjects.Clear();
    }
    private void OnJoinRoomButtonClicked(string roomName)
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }

        PhotonNetwork.JoinRoom(roomName);
    }
    #endregion

    #region Public Methods
    
    public void ActivatePanel(string panelToBeActivated)
    {
        loginUIPanel.SetActive(panelToBeActivated.Equals(loginUIPanel.name));
        gameOptionsUIPanel.SetActive(panelToBeActivated.Equals(gameOptionsUIPanel.name));
        joinRandomRoomUIPanel.SetActive(panelToBeActivated.Equals(joinRandomRoomUIPanel.name));
        insideRoomUIPanel.SetActive(panelToBeActivated.Equals(insideRoomUIPanel.name));
        createRoomUIPanel.SetActive(panelToBeActivated.Equals(createRoomUIPanel.name));
        roomListUIPanel.SetActive(panelToBeActivated.Equals(roomListUIPanel.name));
    }

    #endregion

}
