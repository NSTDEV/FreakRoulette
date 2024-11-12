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
        if (other.CompareTag("Player"))
        {
            PhotonView playerPhotonView = other.GetComponent<PhotonView>();
            if (playerPhotonView != null && playerPhotonView.IsMine)
            {
                PlayerController playerController = other.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.IncreaseCandies();
                }
            }
        }
    }

    IEnumerator RespawnCandy()
    {
        while (true)
        {
            yield return new WaitForSeconds(respawnTime);

            Vector3 randomPosition = new Vector3(Random.Range(-7, 7), Random.Range(-7, 7), candyPrefab.transform.position.z);

            // Solo el MasterClient instancia el nuevo caramelo en red
            PhotonNetwork.Instantiate(candyPrefab.name, randomPosition, Quaternion.identity);
        }
    }
}
