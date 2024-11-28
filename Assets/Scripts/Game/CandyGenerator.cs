using UnityEngine;
using Photon.Pun;
using System.Collections;

public class CandyGenerator : MonoBehaviour
{
    public GameObject candyPrefab;
    public Animator candyAnimator;
    public float respawnTime = 1.5f;
    private bool isGenerating = false; // Bandera para controlar si está generando caramelos

    void Start()
    {
        // Asegúrate de que solo el Master Client inicie la generación de caramelos
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(RespawnCandy());
        }
    }

    // Llama a este método cuando se reinicie la generación de caramelos
    public void ResetGenerator()
    {
        // Detener la generación actual y empezar de nuevo
        StopAllCoroutines(); // Detén cualquier corrutina existente
        StartCoroutine(RespawnCandy()); // Reinicia la generación de caramelos
    }

    private IEnumerator RespawnCandy()
    {
        if (isGenerating) yield break; // Evita iniciar otra corrutina
        isGenerating = true;

        while (isGenerating)
        {
            yield return new WaitForSeconds(respawnTime);

            Vector3 randomPosition = new Vector3(Random.Range(-6, 6), Random.Range(-3, 3), candyPrefab.transform.position.z);

            // Verificar si hay un caramelo en una posición cercana
            Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(randomPosition, 0.5f);
            bool candyExists = false;

            foreach (var obj in nearbyObjects)
            {
                if (obj.CompareTag("Candy"))
                {
                    candyExists = true;
                    break;
                }
            }

            if (!candyExists)
            {
                PhotonNetwork.Instantiate(candyPrefab.name, randomPosition, Quaternion.identity);
            }
        }
    }

    // Detiene la generación de caramelos en todos los clientes
    [PunRPC]
    public void RPC_StopGeneration()
    {
        isGenerating = false;
    }

    public void StopGeneration()
    {
        isGenerating = false;
        PhotonView.Get(this).RPC("RPC_StopGeneration", RpcTarget.All);
    }

    // Este método se llama cuando el generador se desactiva
    private void OnDisable()
    {
        DestroyAllCandies(); // Elimina todos los caramelos cuando se desactiva
    }

    private void DestroyAllCandies()
    {
        // Encuentra todos los caramelos y destrúyelos en la red
        GameObject[] candies = GameObject.FindGameObjectsWithTag("Candy");
        foreach (GameObject candy in candies)
        {
            PhotonView candyView = candy.GetComponent<PhotonView>();
            if (candyView != null)
            {
                if (!candyView.IsMine && PhotonNetwork.IsMasterClient)
                {
                    candyView.TransferOwnership(PhotonNetwork.MasterClient); // Transfiere la propiedad al MasterClient
                }
                PhotonNetwork.Destroy(candy); // Destruye el caramelo
            }
        }
    }
}
