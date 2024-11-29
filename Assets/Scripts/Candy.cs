using System.Collections;
using Photon.Pun;
using UnityEngine;

public class Candy : MonoBehaviourPunCallbacks
{
    public Animator candyAnimator;
    private BoxCollider2D candyCollider; // Declaración del collider

    void Start()
    {
        candyCollider = GetComponent<BoxCollider2D>(); // Obtener el componente BoxCollider2D
    }

    public void Initialize(Vector3 initPosition)
    {
        transform.position = initPosition; // Configura la posición inicial del caramelo
    }

    [PunRPC]
    public void TriggerDestruction()
    {
        candyCollider.enabled = false; // Desactiva el collider
        StartCoroutine(CandyDestruction());
    }

    private IEnumerator CandyDestruction()
    {
        if (candyAnimator != null)
        {
            candyAnimator.SetTrigger("Collected"); // Activa la animación de "recolectado"
            yield return new WaitForSeconds(1f); // Espera a que termine la animación
        }
        Destroy(gameObject); // Elimina el caramelo del cliente local
    }
}
