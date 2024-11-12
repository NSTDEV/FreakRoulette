using TMPro;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using ExitGames.Client.Photon;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField nameInput;
    public GameObject lobbyPanel, roomPanel, playButton;
    public TMP_Text roomName, playerName;
    public int maxPlayers = 3;

    public RoomItem roomItemPrefab;
    List<RoomItem> roomItemsList = new List<RoomItem>();
    public Transform contentObject;

    public float timeBetweenUpdates = 1.5f;
    float nextUpdateTime;

    public List<PlayerItem> playerItemsList = new List<PlayerItem>();
    public PlayerItem playerItemPrefab;
    public Transform playerItemParent;

    private void Awake()
    {
        PhotonNetwork.LocalPlayer.NickName = PlayerPrefs.GetString("PlayerName", "Player");
        playerName.text = PhotonNetwork.LocalPlayer.NickName;
    }

    private void Start()
    {
        PhotonNetwork.JoinLobby();
    }

    private void Update()
    {
        // Muestra el botón de jugar solo si eres el MasterClient y hay 2 o más jugadores
        playButton.SetActive(PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount >= 2);
    }

    public void CreateRoom()
    {
        if (nameInput.text.Length >= 1)
        {
            PhotonNetwork.CreateRoom(nameInput.text, new RoomOptions() { MaxPlayers = maxPlayers, BroadcastPropsChangeToAll = true });
        }
    }

    public void JoinRoom()
    {
        if (nameInput.text.Length >= 1)
        {
            PhotonNetwork.JoinRoom(nameInput.text);
        }
    }

    public void OnClickJoinRoom()
    {
        PhotonNetwork.JoinRoom(roomItemPrefab.roomName.text);
    }

    public void OnClickLeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void OnClickPlayButton()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            PlayerItem playerItem = playerItemsList.Find(item => item.player == player);
            if (playerItem != null)
            {
                playerItem.SetPlayerInfo(player);
            }
        }
        PhotonNetwork.LoadLevel("Game");
    }

    public override void OnJoinedRoom()
    {
        lobbyPanel.SetActive(false);
        roomPanel.SetActive(true);
        roomName.text = PhotonNetwork.CurrentRoom.Name;

        UpdatePlayerList();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (Time.time >= nextUpdateTime)
        {
            UpdateRoomList(roomList);
            nextUpdateTime = Time.time + timeBetweenUpdates;
        }
    }

    void UpdateRoomList(List<RoomInfo> list)
    {
        foreach (RoomItem item in roomItemsList)
        {
            Destroy(item.gameObject);
        }
        roomItemsList.Clear();

        foreach (RoomInfo room in list)
        {
            RoomItem newRoom = Instantiate(roomItemPrefab, contentObject);
            newRoom.SetRoomName(room.Name);
            roomItemsList.Add(newRoom);
        }
    }

    public override void OnLeftRoom()
    {
        roomPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    void UpdatePlayerList()
    {
        foreach (PlayerItem item in playerItemsList)
        {
            Destroy(item.gameObject);
        }
        playerItemsList.Clear();

        if (PhotonNetwork.CurrentRoom == null)
        {
            return;
        }

        foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            PlayerItem newPlayerItem = Instantiate(playerItemPrefab, playerItemParent);
            newPlayerItem.SetPlayerInfo(player.Value);

            if (player.Value == PhotonNetwork.LocalPlayer)
            {
                newPlayerItem.ApplyLocalChanges();
            }

            playerItemsList.Add(newPlayerItem);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();
    }
}
