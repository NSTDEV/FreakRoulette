using System.Collections;
using Photon.Pun;
using UnityEngine;

public class Candy : MonoBehaviourPunCallbacks
{
    Candy instance;
    public Animator candyAnimator;
    private BoxCollider2D candyCollider; // Declaración del collider

    private void Awake()
    {
        instance = this;
        candyCollider = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        candyCollider = GetComponent<BoxCollider2D>();
    }

    public void Initialize(Vector3 initPosition)
    {
        transform.position = initPosition; // Configura la posición inicial del caramelo
    }

    [PunRPC]
    public void TriggerDestruction()
    {
        if (candyAnimator != null)
        {
            candyCollider.enabled = false;

            candyAnimator.SetTrigger("Collected");
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
