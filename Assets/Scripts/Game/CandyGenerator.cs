using UnityEngine;
using Photon.Pun;
using System.Collections;

public class CandyGenerator : MonoBehaviour
{
    public GameObject candyPrefab;
    public float respawnTime = 1.5f;

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(RespawnCandy());
        }
    }

    private IEnumerator RespawnCandy()
    {
        while (true)
        {
            yield return new WaitForSeconds(respawnTime);
            Vector3 randomPosition = new Vector3(Random.Range(-6, 6), Random.Range(-3, 3), candyPrefab.transform.position.z);
            PhotonNetwork.Instantiate(candyPrefab.name, randomPosition, Quaternion.identity);
        }
    }
}
