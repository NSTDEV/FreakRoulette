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

        if (candyAnimator != null)
        {
            candyAnimator.SetTrigger("Collected");
            StartCoroutine(CandyDestruction());
        }
        else
        {
            StartCoroutine(CandyDestruction());
        }
    }

    private IEnumerator CandyDestruction()
    {
        if (candyAnimator != null)
        {
            candyAnimator.SetTrigger("Collected");
            yield return new WaitForSeconds(1f);
        }
        Destroy(gameObject);
    }
}
