
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpawnPlayers : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public float minX, maxX, minY, maxY;

    void Start()
    {
        Vector3 randomPosition = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), playerPrefab.transform.position.z);
        GameObject spawnedPlayer = PhotonNetwork.Instantiate(playerPrefab.name, randomPosition, Quaternion.identity);

        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("playerAvatar", out object avatarIndexObj))
        {
            int avatarIndex = (int)avatarIndexObj;

            PlayerController playerController = spawnedPlayer.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.SetAvatar(avatarIndex);
            }
        }
        else
        {
            Debug.LogError("No se encontró la propiedad 'playerAvatar' en el jugador local.");
        }
    }

}
