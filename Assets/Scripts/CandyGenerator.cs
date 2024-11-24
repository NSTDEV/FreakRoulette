using UnityEngine;
using Photon.Pun;
using System.Collections;

public class CandyGenerator : MonoBehaviour
{
    public GameObject candyPrefab;
    public float rotationSpeed = 5f;
    public float respawnTime = 1.5f;

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(RespawnCandy());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Asegurarse de que solo el jugador que colisiona aumente los caramelos
        if (other.CompareTag("Player") && other.GetComponent<PhotonView>().IsMine)
        {
            // Incrementa los caramelos solo si el jugador es local
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.RPC_IncreaseCandies();
            }
        }
    }

    // Coroutine para generar caramelos periódicamente
    private IEnumerator RespawnCandy()
    {
        while (true)
        {
            yield return new WaitForSeconds(respawnTime);
            Vector3 randomPosition = new Vector3(Random.Range(-7, 7), Random.Range(-7, 7), candyPrefab.transform.position.z);
            PhotonNetwork.Instantiate(candyPrefab.name, randomPosition, Quaternion.identity);
        }
    }
}
