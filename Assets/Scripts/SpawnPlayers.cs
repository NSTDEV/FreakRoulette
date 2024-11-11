using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpawnPlayers : MonoBehaviour
{
    public GameObject[] playerPrefabs;
    public float minX, maxX, minY, maxY;

    void Start()
    {
        Vector2 randomPosition = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
        
        // Verificar que "playerAvatar" existe en CustomProperties
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("playerAvatar"))
        {
            int avatarIndex = (int)PhotonNetwork.LocalPlayer.CustomProperties["playerAvatar"];
            if (avatarIndex >= 0 && avatarIndex < playerPrefabs.Length)
            {
                GameObject playerToSpawn = playerPrefabs[avatarIndex];
                GameObject spawnedPlayer = PhotonNetwork.Instantiate(playerToSpawn.name, randomPosition, Quaternion.identity);

                // Opcional: Log para verificar el nombre del prefab
                Debug.Log("Jugador instanciado: " + playerToSpawn.name);
            }
            else
            {
                Debug.LogError("Índice de avatar fuera de rango.");
            }
        }
        else
        {
            Debug.LogError("Custom property 'playerAvatar' no encontrada.");
        }
    }
}
