using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpawnPlayers : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;

    [Header("Spawn Limits")]
    public float minX, maxX, minY, maxY;

    private void Start()
    {
        // Verificar si estamos conectados a Photon y si es el jugador local
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.LocalPlayer.IsLocal)
        {
            SpawnPlayer();
        }
    }

    private void SpawnPlayer()
    {
        // Posición aleatoria dentro de los límites especificados
        Vector3 randomPosition = new Vector3(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY),
            playerPrefab.transform.position.z
        );

        PhotonNetwork.Instantiate(playerPrefab.name, randomPosition, Quaternion.identity);
    }

    public override void OnLeftRoom()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && player.GetComponent<PhotonView>().IsMine)
        {
            PhotonNetwork.Destroy(player);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        // Aquí inicializas al jugador recién entrado
        InitializeNewPlayer(newPlayer);
    }

    private void InitializeNewPlayer(Player newPlayer)
    {
        if (!newPlayer.CustomProperties.ContainsKey("IsEliminated"))
        {
            newPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "IsEliminated", false } });
        }
    }
}
