using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;
    public static GameManager Instance { get; private set; }

    [Header("Timer")]
    public float roundDuration = 40f;
    public float versusDuration = 10f;
    public TMP_Text timerText;
    private float timer;

    [Header("Transition")]
    public Animator transitionAnimator;
    public float transitionDuration = 1.5f;

    private bool isTransitioning;
    private string eliminationMessage;

    void Awake()
    {
        // Asegurarse de que solo haya una instancia
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destruir la instancia duplicada
            return;
        }

        Instance = this; // Asignamos la instancia
        DontDestroyOnLoad(gameObject); // Aseguramos que no se destruya al cambiar de escena

        SceneManager.sceneLoaded += OnSceneLoaded; // Asegúrate de registrar el evento
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;

        // Inicializamos el temporizador correctamente dependiendo de la escena
        if (sceneName == "Game")
        {
            ResetTimer(roundDuration);
        }
        else if (sceneName == "Versus")
        {
            ResetTimer(versusDuration);
        }

        isTransitioning = false; // Restablecer transición
    }

    private void Start()
    {
        // Solo se ejecuta en la primera escena (si no hay un GameManager persistente)
        if (Instance == this)
        {
            ResetTimer(roundDuration);
        }
    }

    private void Update()
    {
        if (isTransitioning) return;

        UpdateTimer();
        if (timer <= 0) HandleSceneTransition();
    }

    private void UpdateTimer()
    {
        timer -= Time.deltaTime;
        timerText.text = Mathf.Max(0, timer).ToString("F0");
    }

    private void HandleSceneTransition()
    {
        if (isTransitioning) return;

        isTransitioning = true;
        string currentScene = SceneManager.GetActiveScene().name;

        if (!IsAnimatorValid()) return;

        if (currentScene == "Game")
        {
            photonView.RPC(nameof(StartSceneWithTransitionRPC), RpcTarget.All, "Versus");
        }
        else if (currentScene == "Versus")
        {
            if (PhotonNetwork.IsMasterClient) EliminatePlayerWithLowestPoints();
            photonView.RPC(nameof(DisplayEliminationMessageRPC), RpcTarget.AllBuffered);
            photonView.RPC(nameof(StartSceneWithTransitionRPC), RpcTarget.All, "Game");
        }
        else
        {
            Debug.LogWarning($"Escena desconocida: {currentScene}");
            isTransitioning = false;
        }
    }

    [PunRPC]
    private void ResetTimerRPC()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        ResetTimer(sceneName == "Game" ? roundDuration : 10f);
    }

    private void ResetTimer(float duration)
    {
        timer = duration;
        Debug.Log($"Timer reiniciado a {timer} segundos.");
    }

    public void ChangeToLobby()
    {
        StopAllCoroutines();
        PhotonNetwork.LeaveRoom();
        StartCoroutine(TransitionToLobby("Lobby"));
    }

    private IEnumerator TransitionToLobby(string sceneName)
    {
        yield return PlayTransitionAnimation();
        SceneManager.LoadScene(sceneName);
        CleanupSingleton();
    }

    private IEnumerator PlayTransitionAnimation()
    {
        if (IsAnimatorValid())
        {
            transitionAnimator.SetTrigger("SceneEnter");
            yield return new WaitForSeconds(transitionDuration);
        }
    }

    private bool IsAnimatorValid()
    {
        if (transitionAnimator == null)
        {
            Debug.LogError("transitionAnimator no está asignado.");
            return false;
        }
        return true;
    }

    private void CleanupSingleton()
    {
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject); // Destruir solo si es una instancia diferente
        }
        Instance = null;
    }

    [PunRPC]
    public void StartSceneWithTransitionRPC(string sceneName)
    {
        Debug.Log($"Transición a la escena {sceneName} iniciada.");
        if (!IsAnimatorValid()) return;

        // Cargar la escena de forma controlada
        StartCoroutine(LoadSceneWithDelay(sceneName));
    }

    private IEnumerator LoadSceneWithDelay(string sceneName)
    {
        yield return PlayTransitionAnimation();

        // Cargar la escena usando Photon sin perder el estado
        if (PhotonNetwork.IsConnected)
        {
            // Llama a un RPC para realizar cambios en la escena sin recargarla
            photonView.RPC(nameof(TransitionSceneStateRPC), RpcTarget.All, sceneName);
        }
        else
        {
            Debug.LogError("PhotonNetwork no está conectado.");
        }

        transitionAnimator.SetTrigger("SceneExit");
    }

    [PunRPC]
    private void TransitionSceneStateRPC(string sceneName)
    {
        // Realiza lo que necesites para "Versus" o "Game"
        if (sceneName == "Versus")
        {
            Debug.Log("Entrando a la escena Versus...");
        }
        else if (sceneName == "Game")
        {
            Debug.Log("Entrando a la escena Game...");
        }
    }

    public void EliminatePlayerWithLowestPoints()
    {
        Player playerToEliminate = PhotonNetwork.PlayerList
            .Where(p => p.CustomProperties.ContainsKey("Candies"))
            .OrderBy(p => (int)p.CustomProperties["Candies"])
            .FirstOrDefault();

        if (playerToEliminate != null)
        {
            int lowestPoints = (int)playerToEliminate.CustomProperties["Candies"];

            if (Random.value <= 0.5f)
            {
                eliminationMessage = $"{playerToEliminate.NickName} ha sido eliminado con {lowestPoints} puntos.";
                photonView.RPC(nameof(HandlePlayerEliminationRPC), RpcTarget.AllBuffered, playerToEliminate.UserId);
            }
            else
            {
                eliminationMessage = $"{playerToEliminate.NickName} ha sobrevivido con {lowestPoints} puntos.";
            }

            photonView.RPC(nameof(DisplayEliminationMessageRPC), RpcTarget.AllBuffered);
        }
        else
        {
            Debug.Log("No se encontró un jugador con puntos para eliminar.");
        }
    }

    [PunRPC]
    private void HandlePlayerEliminationRPC(string playerId)
    {
        Player playerToEliminate = PhotonNetwork.PlayerList.FirstOrDefault(p => p.UserId == playerId);
        if (playerToEliminate == null) return;

        if (playerToEliminate.TagObject is PhotonView playerView &&
            playerView.GetComponent<PlayerController>() is PlayerController playerController)
        {
            playerController.DisableMovement();
        }

        playerToEliminate.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
        {
            { "IsEliminated", true }
        });

        Debug.Log($"{playerToEliminate.NickName} ha sido eliminado.");
    }

    [PunRPC]
    private void DisplayEliminationMessageRPC()
    {
        if (!string.IsNullOrEmpty(eliminationMessage))
        {
            Debug.Log(eliminationMessage);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
