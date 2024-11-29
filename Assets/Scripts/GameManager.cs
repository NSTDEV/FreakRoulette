using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

public class GameManager : MonoBehaviourPunCallbacks
{
    private static GameManager instance;

    [Header("Timer")]
    public float roundDuration = 40f;
    public TMP_Text timerText;
    private float timer;

    [Header("Transition")]
    public Animator transitionAnimator;
    public float transitionDuration = 1.5f;

    private bool isTransitioning = false;
    private string eliminationMessage = "";

     // Referencia al componente AudioSource
    private AudioSource audioSource;

    // AudioClip para el sonido a reproducir
    public AudioClip clip;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            PhotonView photonView = GetComponent<PhotonView>();
            if (photonView == null)
            {
                Debug.LogWarning("PhotonView no encontrado en el GameManager. Añade un PhotonView al GameManager.");
                Destroy(gameObject);
                return;
            }
        }
        else if (instance != this)
        {
            Destroy(gameObject); // Destruye duplicados
            return;
        }
    }

    void Start()
    {
        timer = roundDuration;

         // Obtener el componente AudioSource en el objeto
        audioSource = GetComponent<AudioSource>();

        // Asegurarse de que el AudioSource tenga un clip asignado
        if (clip != null)
        {
            audioSource.clip = clip; // Asignar el AudioClip
        }
    }

    void Update()
    {
        if (isTransitioning) return;

        timer -= Time.deltaTime;
        timerText.text = Mathf.Max(0, timer).ToString("F0");

        if (timer <= 0)
        {
            HandleSceneTransition();
        }
    }

    private void HandleSceneTransition()
    {
        if (isTransitioning) return;

        isTransitioning = true;
        string currentScene = SceneManager.GetActiveScene().name;

        if (transitionAnimator == null)
        {
            Debug.LogError("transitionAnimator no está asignado.");
            isTransitioning = false;
            return;
        }

        switch (currentScene)
        {
            case "Game":
                if (PhotonNetwork.IsMasterClient)
                {
                    EliminatePlayerWithLowestPoints();
                }

                if (PhotonNetwork.IsConnected && photonView != null)
                {
                    photonView.RPC("StartSceneWithTransitionRPC", RpcTarget.All, "Versus");
                }
                else
                {
                    Debug.LogError("PhotonNetwork no está conectado o photonView es null.");
                    isTransitioning = false;
                }
                break;

            case "Versus":
                if (PhotonNetwork.IsConnected && photonView != null)
                {
                    photonView.RPC("DisplayEliminationMessageRPC", RpcTarget.AllBuffered);
                    photonView.RPC("StartSceneWithTransitionRPC", RpcTarget.All, "Game");
                }
                else
                {
                    Debug.LogError("PhotonNetwork no está conectado o photonView es null.");
                    isTransitioning = false;
                }
                break;

            default:
                Debug.LogWarning("Escena desconocida: " + currentScene);
                isTransitioning = false;
                break;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ResetTimerRPC", RpcTarget.AllBuffered);
        }

        isTransitioning = false;
    }

    [PunRPC]
    private void ResetTimerRPC()
    {
        timer = SceneManager.GetActiveScene().name == "Game" ? roundDuration : 10f;
    }

    public void ChangeToLobby()
    {
        StopAllCoroutines();
        PhotonNetwork.LeaveRoom();
        StartCoroutine(TransitionToLobby());
    }

    private IEnumerator TransitionToLobby()
    {
        if (transitionAnimator != null)
        {
            transitionAnimator.SetTrigger("SceneEnter");
            audioSource.Play(); // Reproduce el sonido
            yield return new WaitForSeconds(transitionDuration);
        }
        else
        {
            Debug.LogError("transitionAnimator no está asignado.");
        }

        SceneManager.LoadScene("Lobby");

        if (instance != null && instance.gameObject != null)
        {
            Destroy(gameObject);
            instance = null;
        }
    }

    [PunRPC]
    public void StartSceneWithTransitionRPC(string sceneName)
    {
        if (transitionAnimator != null)
        {
            transitionAnimator.SetTrigger("SceneEnter");
            StartCoroutine(LoadSceneWithDelay(sceneName));
        }
        else
        {
            Debug.LogError("transitionAnimator no está asignado.");
        }
    }

    private IEnumerator LoadSceneWithDelay(string sceneName)
    {
        yield return new WaitForSeconds(transitionDuration);

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LoadLevel(sceneName);
        }
        else
        {
            Debug.LogError("PhotonNetwork no está conectado.");
        }

        transitionAnimator.SetTrigger("SceneExit");
    }

    public void EliminatePlayerWithLowestPoints()
    {
        Player playerToEliminate = null;
        int lowestPoints = int.MaxValue;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties != null && player.CustomProperties.ContainsKey("Candies"))
            {
                int playerPoints = (int)player.CustomProperties["Candies"];

                if (playerPoints < lowestPoints)
                {
                    lowestPoints = playerPoints;
                    playerToEliminate = player;
                }
            }
        }

        if (playerToEliminate != null)
        {
            eliminationMessage = $"{playerToEliminate.NickName} ha sido eliminado con {lowestPoints} puntos.";
            photonView.RPC("HandlePlayerEliminationRPC", RpcTarget.AllBuffered, playerToEliminate.UserId);
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
        if (playerToEliminate != null)
        {
            if (playerToEliminate.TagObject is PhotonView playerView)
            {
                PlayerController playerMovement = playerView.GetComponent<PlayerController>();
                playerMovement?.DisableMovement();
            }

            var customProperties = new ExitGames.Client.Photon.Hashtable
            {
                { "IsEliminated", true }
            };
            playerToEliminate.SetCustomProperties(customProperties);

            Debug.Log($"{playerToEliminate.NickName} ha sido eliminado.");
        }
    }

    [PunRPC]
    private void DisplayEliminationMessageRPC()
    {
        if (!string.IsNullOrEmpty(eliminationMessage))
        {
            Debug.Log(eliminationMessage);
        }
    }
}
