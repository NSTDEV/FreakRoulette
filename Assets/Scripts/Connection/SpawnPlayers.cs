using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class SpawnPlayers : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;

    [Header("Spawn Limits")]
    public float minX, maxX, minY, maxY;

    private void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.LocalPlayer.IsLocal)
        {
            SpawnPlayer();
        }
    }

    private void SpawnPlayer()
    {
        // Verificar si el jugador ya ha sido instanciado
        if (GameObject.FindWithTag("Player") == null)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY),
                playerPrefab.transform.position.z
            );

            GameObject spawnedPlayer = PhotonNetwork.Instantiate(playerPrefab.name, randomPosition, Quaternion.identity);
            DontDestroyOnLoad(spawnedPlayer); // Evitar que el jugador se destruya al cambiar de escena

            // Configurar el avatar del jugador si existe
            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("playerAvatar", out object avatarIndexObj))
            {
                int avatarIndex = (int)avatarIndexObj;
                PlayerController playerController = spawnedPlayer.GetComponent<PlayerController>();

                if (playerController != null)
                {
                    playerController.photonView.RPC(nameof(PlayerController.RPC_UpdateAvatar), RpcTarget.AllBuffered, avatarIndex);
                }
            }
            else
            {
                Debug.LogWarning("No se encontró la propiedad 'playerAvatar' en el jugador local.");
            }
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Jugador unido a la sala.");
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        Debug.Log("Jugador salió de la sala.");
    }
}
