using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicButtonConnect : MonoBehaviour
{
    // Referencia al componente AudioSource
    private AudioSource audioSource;

    // Referencia al clip de sonido
    public AudioClip clickSound;

    void Start()
    {
        // Obtener el componente AudioSource que está en el mismo GameObject
        audioSource = GetComponent<AudioSource>();

        // Verificar si el clip de sonido está asignado
        if (clickSound == null)
        {
            Debug.LogWarning("No se ha asignado un sonido al botón.");
        }
    }

    // Método para asignar al botón y reproducir el sonido cuando se hace clic
    public void PlayButtonClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);  // Reproduce el sonido una sola vez
        }
    }
}
