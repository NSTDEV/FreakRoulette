using TMPro;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public TMP_InputField nameInput;
    public GameObject lobbyPanel, roomPanel, playButton;
    public TMP_Text roomName, playerName;
    public Transform roomListParent, playerListParent;
    public Animator transitionAnimator;
    public float transitionDuration = 1.1f;

    [Header("Prefabs")]
    public RoomItem roomItemPrefab;
    public PlayerItem playerItemPrefab;

    [Header("Settings")]
    public int maxPlayers = 6;
    public float updateInterval = 1.5f;

    private float nextUpdateTime;
    private readonly List<RoomItem> roomItems = new();
    private readonly List<PlayerItem> playerItems = new();

    private void Awake()
    {
        string defaultName = PlayerPrefs.GetString("PlayerName", "Player");
        PhotonNetwork.LocalPlayer.NickName = defaultName;
        playerName.text = defaultName;
    }

    private void Start() => PhotonNetwork.JoinLobby();

    private void Update() => playButton.SetActive(PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom?.PlayerCount > 0);

    public void CreateRoom()
    {
        if (!string.IsNullOrWhiteSpace(nameInput.text))
        {
            PhotonNetwork.CreateRoom(nameInput.text, new RoomOptions { MaxPlayers = (byte)maxPlayers });
        }
    }

    public void JoinRoom()
    {
        if (!string.IsNullOrWhiteSpace(nameInput.text))
        {
            PhotonNetwork.JoinRoom(nameInput.text);
        }
    }

    public void LeaveRoom() => PhotonNetwork.LeaveRoom();

    public void StartGame() => StartCoroutine(StartGameWithTransition());

    private IEnumerator StartGameWithTransition()
    {
        transitionAnimator.SetTrigger("SceneEnter");
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

        RefreshList(roomItems, roomListParent);

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

        RefreshList(playerItems, playerListParent);

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            PlayerItem newPlayer = Instantiate(playerItemPrefab, playerListParent);
            newPlayer.SetPlayerInfo(player); // Solo se asigna la información básica
            playerItems.Add(newPlayer);
        }
    }

    private void RefreshList<T>(List<T> list, Transform parent) where T : MonoBehaviour
    {
        foreach (T item in list)
            Destroy(item.gameObject);

        list.Clear();
    }
    // Método Bubble Sort que ordena los jugadores por nombre alfabéticamente
private void BubbleSort(List<PlayerItemInfo> players)
{
    int n = players.Count;
    bool swapped;
    do
    {
        swapped = false;
        for (int i = 0; i < n - 1; i++)
        {
            if (string.Compare(players[i].PlayerName, players[i + 1].PlayerName) > 0) // Comparar alfabéticamente
            {
                // Intercambiar los elementos si no están en el orden correcto
                PlayerItemInfo temp = players[i];
                players[i] = players[i + 1];
                players[i + 1] = temp;
                swapped = true;
            }
        }
        n--; // Reducir la longitud para evitar comparar elementos ya ordenados
    } while (swapped);
}

// Clase interna para almacenar el jugador y su nombre
private class PlayerItemInfo
{
    public Player Player { get; private set; }
    public string PlayerName { get; private set; }

    public PlayerItemInfo(Player player, string playerName)
    {
        Player = player;
        PlayerName = playerName;
    }
}
}
