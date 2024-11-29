using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicaLoading : MonoBehaviour
{
   // Variable para referenciar el archivo de audio
    public AudioClip musicClip;

    // Componente AudioSource
    private AudioSource audioSource;

    void Start()
    {
        // Obtener el componente AudioSource
        audioSource = GetComponent<AudioSource>();

        // Verificar si el AudioSource está presente
        if (audioSource != null)
        {
            // Asignar el clip de audio al AudioSource
            audioSource.clip = musicClip;

            // Reproducir el audio
            audioSource.Play();

            // Configurar el audio para que se repita en bucle si es necesario
            //audioSource.loop = true; 
        }
        else
        {
            Debug.LogError("No se ha encontrado el componente AudioSource en el GameObject.");
        }
    }
}
