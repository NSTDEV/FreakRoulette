using UnityEngine;
using Photon.Pun;
using System.Collections;

public class CandyGenerator : MonoBehaviour
{
    public GameObject candyPrefab;
    public float rotationSpeed = 0.5f;
    public float respawnTime = 5f; // Tiempo de espera para que aparezca un nuevo caramelo

    private bool isCandyCollected = false; // Para verificar si el caramelo fue recogido

    void Start()
    {
        StartCoroutine(RespawnCandy());
    }

    void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed, Space.Self);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView playerPhotonView = other.GetComponent<PhotonView>();

            // Asegurarse de que solo el jugador local que recoge el caramelo lo incremente
            if (playerPhotonView != null && playerPhotonView.IsMine)
            {
                PlayerController playerController = other.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.IncreaseCandies();
                }
            }

            // Destruir el caramelo original en todos los clientes
            PhotonNetwork.Destroy(gameObject); // Destruye el caramelo actual en todos los clientes

            // Marcar que el caramelo fue recogido
            isCandyCollected = true;
        }
    }

    IEnumerator RespawnCandy()
    {
        while (true)
        {
            if (isCandyCollected)
            {
                yield return new WaitForSeconds(respawnTime);

                Vector2 randomPosition = new Vector2(Random.Range(-5, 5), Random.Range(-5, 5));
                PhotonNetwork.Instantiate(candyPrefab.name, randomPosition, Quaternion.identity);

                isCandyCollected = false;
            }

            yield return null;
        }
    }
}
