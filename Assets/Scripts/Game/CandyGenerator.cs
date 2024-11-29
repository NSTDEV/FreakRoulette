using UnityEngine;
using Photon.Pun;
using System.Collections;

public class CandyGenerator : MonoBehaviour
{
    public GameObject candyPrefab; // Prefab del caramelo asignado desde el editor
    public float respawnTime = 1.5f; // Tiempo entre spawns
    private bool isGenerating = false;

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ResetGenerator();
        }
    }

    public void ResetGenerator()
    {
        DestroyAllCandies();
        StopAllCoroutines();
        StartCoroutine(RespawnCandy());
    }

    private IEnumerator RespawnCandy()
    {
        if (isGenerating) yield break;
        isGenerating = true;

        while (isGenerating)
        {
            yield return new WaitForSeconds(respawnTime);

            // Genera una posición aleatoria manteniendo el z del prefab
            Vector3 randomPosition = new Vector3(
                Random.Range(-6, 6),
                Random.Range(-3, 3),
                candyPrefab.transform.position.z // Usar el valor de z del prefab
            );

            if (!IsCandyNearby(randomPosition))
            {
                // Instancia el caramelo en la posición correcta
                GameObject newCandy = PhotonNetwork.Instantiate(candyPrefab.name, randomPosition, Quaternion.identity);

                // Sincroniza la animación si el prefab tiene un componente Candy
                Candy candy = newCandy.GetComponent<Candy>();
                if (candy != null)
                {
                    candy.Initialize(randomPosition);
                }
            }
        }
    }

    private bool IsCandyNearby(Vector3 position)
    {
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(position, 2.5f);
        foreach (var obj in nearbyObjects)
        {
            if (obj.CompareTag("Candy"))
            {
                return true;
            }
        }
        return false;
    }

    public void StopGeneration()
    {
        isGenerating = false;
        PhotonView.Get(this).RPC("RPC_StopGeneration", RpcTarget.AllBuffered);
        DestroyAllCandies();
    }

    [PunRPC]
    public void RPC_StopGeneration()
    {
        isGenerating = false;
    }

    private void DestroyAllCandies()
    {
        GameObject[] candies = GameObject.FindGameObjectsWithTag("Candy");
        foreach (GameObject candyObj in candies)
        {
            Candy candy = candyObj.GetComponent<Candy>();
            PhotonView candyPhotonView = candyObj.GetComponent<PhotonView>();

            if (candy != null)
            {
                candy.TriggerDestruction(); // Activa la lógica de destrucción del caramelo
            }

            if (candyPhotonView != null)
            {
                PhotonNetwork.Destroy(candyPhotonView.gameObject); // Destruye el objeto en la red
            }
        }
    }
}
