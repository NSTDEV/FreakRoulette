using TMPro;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Collections;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public TMP_InputField nameInput;
    public GameObject lobbyPanel, roomPanel, playButton;
    public TMP_Text roomName, playerName;
    public Transform roomListParent, playerListParent;
    public Animator transitionAnimator; // Animator con el trigger "Start"
    public float transitionDuration = 1.1f;

    [Header("Prefabs")]
    public RoomItem roomItemPrefab;
    public PlayerItem playerItemPrefab;

    [Header("Settings")]
    public int maxPlayers = 6;
    public float updateInterval = 1.5f;

    private float nextUpdateTime;
    private List<RoomItem> roomItems = new();
    private List<PlayerItem> playerItems = new();

    private void Awake()
    {
        string defaultName = PlayerPrefs.GetString("PlayerName", "Player");
        PhotonNetwork.LocalPlayer.NickName = defaultName;
        playerName.text = defaultName;
    }

    private void Start() => PhotonNetwork.JoinLobby();

    private void Update()
    {
        playButton.SetActive(PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom?.PlayerCount >= 1);
    }

    public void CreateRoom()
    {
        if (nameInput.text.Length > 0)
            PhotonNetwork.CreateRoom(nameInput.text, new RoomOptions { MaxPlayers = (byte)maxPlayers });
    }

    public void JoinRoom() => PhotonNetwork.JoinRoom(nameInput.text);

    public void LeaveRoom() => PhotonNetwork.LeaveRoom();

    public void StartGame()
    {
        StartCoroutine(StartGameWithTransition());
    }

    private IEnumerator StartGameWithTransition()
    {
        transitionAnimator.SetTrigger("Start");

        yield return new WaitForSeconds(transitionDuration);

        PhotonNetwork.LoadLevel("Game");
    }

    public override void OnJoinedRoom()
    {
        lobbyPanel.SetActive(false);
        roomPanel.SetActive(true);
        roomName.text = PhotonNetwork.CurrentRoom.Name;
        UpdatePlayerList();
    }

    public override void OnLeftRoom()
    {
        roomPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (Time.time < nextUpdateTime) return;

        ClearList(roomItems);
        foreach (RoomInfo room in roomList)
        {
            if (!room.RemovedFromList)
            {
                RoomItem newRoom = Instantiate(roomItemPrefab, roomListParent);
                newRoom.SetRoomName(room.Name);
                roomItems.Add(newRoom);
            }
        }
        nextUpdateTime = Time.time + updateInterval;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) => UpdatePlayerList();

    public override void OnPlayerLeftRoom(Player otherPlayer) => UpdatePlayerList();

    private void UpdatePlayerList()
    {
        if (PhotonNetwork.CurrentRoom == null) return;

        ClearList(playerItems);
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            PlayerItem newPlayer = Instantiate(playerItemPrefab, playerListParent);
            newPlayer.SetPlayerInfo(player);

            if (player == PhotonNetwork.LocalPlayer)
                newPlayer.ApplyLocalChanges();

            playerItems.Add(newPlayer);
        }
    }

    private void ClearList<T>(List<T> list) where T : MonoBehaviour
    {
        foreach (var item in list)
            Destroy(item.gameObject);

        list.Clear();
    }
}
