using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviourPunCallbacks
{
    private static GameManager instance; // Singleton

    [Header("Timer")]
    public float roundDuration = 40f;
    public TMP_Text timerText;
    private float timer;

    [Header("Transition")]
    public Animator transitionAnimator;
    public float transitionDuration = 1.5f;

    private bool isTransitioning = false; // Flag para evitar transiciones múltiples

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Evita que se destruya al cambiar de escena
            SceneManager.sceneLoaded += OnSceneLoaded; // Escuchar cuando se carga una nueva escena
        }
        else if (instance != this)
        {
            Destroy(gameObject); // Destruir duplicados
            return;
        }
    }

    void Start()
    {
        timer = roundDuration;
    }

    void Update()
    {
        if (isTransitioning) return; // Evitar actualizaciones mientras se está en transición

        timer -= Time.deltaTime;
        timerText.text = Mathf.Max(0, timer).ToString("F0");

        if (timer <= 0)
        {
            HandleSceneTransition();
        }
    }

    private void HandleSceneTransition()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Si ya se está en transición, no hacer nada
        if (isTransitioning) return;

        isTransitioning = true; // Iniciar transición

        switch (currentScene)
        {
            case "Game":
                StartCoroutine(StartSceneWithTransition("Versus"));
                break;

            case "Versus":
                StartCoroutine(StartSceneWithTransition("Game"));
                break;

            default:
                Debug.LogWarning("Escena desconocida: " + currentScene);
                break;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game" && timer <= 0)
        {
            // Reinicia el temporizador si vuelves a la escena "Game"
            timer = roundDuration;
        }
        // Reiniciar el temporizador cuando entres en la escena "Versus"
        else if (scene.name == "Versus")
        {
            timer = 10f; // Reiniciar temporizador para "Versus"
        }

        // Eliminar GameManager al entrar en la escena de Lobby
        if (scene.name == "Lobby")
        {
            Destroy(gameObject); // Eliminar el GameManager persistente al entrar en el Lobby
        }

        isTransitioning = false; // Finalizar la transición
    }

    public void ChangeToLobby()
    {
        StopAllCoroutines(); // Detener cualquier corrutina en curso

        if (transitionAnimator != null)
        {
            transitionAnimator.SetTrigger("SceneEnter"); // Activar la animación de transición
        }

        PhotonNetwork.LeaveRoom(); // Salir de la sala de Photon
        StartCoroutine(TransitionToLobby());
    }

    private IEnumerator TransitionToLobby()
    {
        yield return new WaitForSeconds(transitionDuration); // Esperar a que termine la animación
        SceneManager.LoadScene("Lobby"); // Cargar la escena de Lobby
    }

    public IEnumerator StartSceneWithTransition(string sceneName)
    {
        if (transitionAnimator != null)
        {
            transitionAnimator.SetTrigger("SceneEnter"); // Activar animación
            yield return new WaitForSeconds(transitionDuration); // Esperar la duración de la transición

            PhotonNetwork.LoadLevel(sceneName); // Cambiar de escena con Photon
            transitionAnimator.SetTrigger("SceneExit"); // Activar animación de salida
        }
    }
}
