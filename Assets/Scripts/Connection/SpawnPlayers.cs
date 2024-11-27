/*using UnityEngine;
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
           // DontDestroyOnLoad(spawnedPlayer); // Evitar que el jugador se destruya al cambiar de escena

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
}*/
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class SpawnPlayers : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;           // Prefab del jugador
    public GameObject playerSpawnContainer;   // Contenedor de jugadores donde se anidarán (PlayerSpawn)
    private GameObject spawnedPlayer;         // Referencia al jugador instanciado

    [Header("Spawn Limits")]
    public float minX, maxX, minY, maxY;

    private void Start()
    {
        // Verificar que la referencia a playerSpawnContainer esté configurada correctamente
        if (playerSpawnContainer == null)
        {
            Debug.LogError("La referencia a playerSpawnContainer no está asignada en el Inspector.");
            return;
        }

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
            // Generar una posición aleatoria dentro de los límites
            Vector3 randomPosition = new Vector3(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY),
                playerPrefab.transform.position.z
            );

            // Instanciar al jugador
            spawnedPlayer = PhotonNetwork.Instantiate(playerPrefab.name, randomPosition, Quaternion.identity);

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

    private void LateUpdate()
    {
        // Solo asignar el padre si el jugador ha sido instanciado y playerSpawnContainer está disponible
        if (spawnedPlayer != null && playerSpawnContainer != null)
        {
            // Anidar el jugador bajo el objeto "PlayerSpawn"
            spawnedPlayer.transform.SetParent(playerSpawnContainer.transform);
            Debug.Log($"Jugador {spawnedPlayer.name} ahora es hijo de {playerSpawnContainer.name}");
            spawnedPlayer = null; // Solo hacerlo una vez
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



