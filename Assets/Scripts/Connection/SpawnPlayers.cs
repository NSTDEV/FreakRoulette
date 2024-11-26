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
        // Verificar si estamos conectados a Photon y si es el jugador local
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.LocalPlayer.IsLocal)
        {
            // Verificar si el jugador ya está instanciado en la escena
            if (GameObject.FindWithTag("Player") == null)
            {
                SpawnPlayer();
            }
        }
    }

    private void SpawnPlayer()
    {
        // Verificar si el jugador ya ha sido instanciado antes de crear uno nuevo
        if (GameObject.FindWithTag("Player") == null)
        {
            // Posición aleatoria dentro de los límites especificados
            Vector3 randomPosition = new Vector3(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY),
                playerPrefab.transform.position.z
            );

            // Instanciar al jugador usando PhotonNetwork.Instantiate
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

        // Asegurarse de destruir la instancia persistente del jugador correctamente
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            PhotonNetwork.Destroy(player);  // Usar PhotonNetwork.Destroy en lugar de Destroy
        }
    }
}
