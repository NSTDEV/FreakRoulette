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
    public float updateInterval = 3f;

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
    public void SetRoomName(string name)
    {
        roomName.text = name;  // Asigna el nombre al texto en la UI
    }

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

    //public void LeaveRoom() => PhotonNetwork.LeaveRoom();
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();  // Deja la sala
        PhotonNetwork.JoinLobby();  // Vuelve al lobby para obtener la lista de salas
    }

    public void StartGame() => StartCoroutine(StartGameWithTransition());

    private IEnumerator StartGameWithTransition()
    {
        transitionAnimator.SetBool("isSceneEnter", true);
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

    /*public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // Primero, limpiar la lista de la UI para evitar duplicados
        RefreshList(roomItems, roomListParent);

        if (roomList == null || roomList.Count == 0) return;

        // Ahora, agregar las nuevas salas a la UI
        foreach (RoomInfo room in roomList)
        {
            if (!room.RemovedFromList)
            {
                // Crear un nuevo ítem para cada sala que no haya sido eliminada
                RoomItem newRoom = Instantiate(roomItemPrefab, roomListParent);
                newRoom.SetRoomName(room.Name);  // Asigna el nombre de la sala
                roomItems.Add(newRoom);  // Añadir el nuevo ítem a la lista
            }
        }

        // Actualizar el tiempo para la próxima actualización
        nextUpdateTime = Time.time + updateInterval;
    }*/
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // No hacer nada si la lista de salas está vacía
        if (roomList == null || roomList.Count == 0) return;

        // Primer paso: eliminar las salas que ya no están activas
        List<RoomItem> roomsToRemove = new List<RoomItem>();
        foreach (RoomItem room in roomItems)
        {
            bool exists = roomList.Exists(r => r.Name == room.roomName.text && !r.RemovedFromList);
            if (!exists) // Si la sala ha sido eliminada
            {
                roomsToRemove.Add(room);
            }
        }

        // Eliminar las salas que ya no existen
        foreach (RoomItem room in roomsToRemove)
        {
            roomItems.Remove(room);
            Destroy(room.gameObject);  // Eliminar el objeto de la UI
        }

        // Ahora agregamos las nuevas salas
        foreach (RoomInfo room in roomList)
        {
            // Si la sala no está marcada como eliminada y no se encuentra ya en la lista
            if (!room.RemovedFromList && !roomItems.Exists(r => r.roomName.text == room.Name))
            {
                RoomItem newRoom = Instantiate(roomItemPrefab, roomListParent);
                newRoom.SetRoomName(room.Name);  // Asignar el nombre de la sala
                roomItems.Add(newRoom);  // Añadir la nueva sala a la lista
            }
        }

        // Actualizamos el tiempo para la próxima actualización
        nextUpdateTime = Time.time + updateInterval;
    }




    public override void OnPlayerEnteredRoom(Player newPlayer) => UpdatePlayerList();

    public override void OnPlayerLeftRoom(Player otherPlayer) => UpdatePlayerList();

    private void UpdatePlayerList()
    {
        // Debug.Log("Actualización de la lista de salas. Número de salas: " + roomList.Count);
        if (PhotonNetwork.CurrentRoom == null) return;

        // Limpiar la lista y la UI
        RefreshList(playerItems, playerListParent);

        // Crear una lista temporal con los jugadores y sus propiedades
        List<PlayerItemInfo> playersInfo = new List<PlayerItemInfo>();

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            // Obtener el nombre del jugador y agregarlo a la lista
            string playerName = player.NickName;

            playersInfo.Add(new PlayerItemInfo(player, playerName));
        }

        // Ordenar los jugadores alfabéticamente por su nombre usando Bubble Sort
        BubbleSort(playersInfo);

        // Instanciar los elementos en el orden correcto
        foreach (PlayerItemInfo playerInfo in playersInfo)
        {
            PlayerItem newPlayer = Instantiate(playerItemPrefab, playerListParent);
            newPlayer.SetPlayerInfo(playerInfo.Player); // Asignar la información básica del jugador
            playerItems.Add(newPlayer);
        }
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
    private void RefreshList<T>(List<T> list, Transform parent) where T : MonoBehaviour
    {
        foreach (T item in list)
            Destroy(item.gameObject);

        list.Clear();
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("Conectado al servidor master de Photon.");
        PhotonNetwork.JoinLobby();  // Únete al lobby para obtener la lista de salas
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Te has unido al lobby correctamente.");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError("Desconectado de Photon: " + cause.ToString());
    }

}
