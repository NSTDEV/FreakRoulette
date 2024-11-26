/*using UnityEngine;
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
}*/

using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;


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

    [Header("UI")]
    public GameObject playerPrefab; // Prefab para representar jugadores
    public Transform playersContainer; // Contenedor donde se colocarán los jugadores

    private List<PlayerInfo> playerInfos = new List<PlayerInfo>(); // Lista para almacenar la información de los jugadores

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
        if (scene.name == "Versus") // Si estamos en la escena Versus
        {
            SetupPlayersInNewScene();
        }

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ResetTimerRPC", RpcTarget.AllBuffered);
        }

        isTransitioning = false;
    }

    private void SetupPlayersInNewScene()
    {
        // Recolectamos la información de los jugadores
        playerInfos.Clear();
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            int candies = player.CustomProperties.ContainsKey("Candies") ? (int)player.CustomProperties["Candies"] : 0;
            string name = player.NickName;
            int avatarIndex = player.CustomProperties.ContainsKey("playerAvatar") ? (int)player.CustomProperties["playerAvatar"] : 0;

            playerInfos.Add(new PlayerInfo(name, candies, avatarIndex));
        }

        // Ordenamos a los jugadores por puntaje (Bubble Sort)
        BubbleSort(playerInfos);

        // Colocamos a los jugadores uno al lado del otro en la escena
        for (int i = 0; i < playerInfos.Count; i++)
        {
            PlayerInfo playerInfo = playerInfos[i];
            GameObject playerGO = Instantiate(playerPrefab, playersContainer);
            playerGO.transform.position = new Vector3(i * 3f, 0, 0); // Ajustar la posición según el índice

            PlayerController playerController = playerGO.GetComponent<PlayerController>();
            playerController.InitializePlayer();
        }
    }

    private void BubbleSort(List<PlayerInfo> players)
    {
        int n = players.Count;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n - i - 1; j++)
            {
                if (players[j].Candies > players[j + 1].Candies)
                {
                    var temp = players[j];
                    players[j] = players[j + 1];
                    players[j + 1] = temp;
                }
            }
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

    [PunRPC]
    private void ResetTimerRPC()
    {
        timer = SceneManager.GetActiveScene().name == "Game" ? roundDuration : 10f;
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

[System.Serializable]
public class PlayerInfo
{
    public string Name;
    public int Candies;
    public int AvatarIndex;

    public PlayerInfo(string name, int candies, int avatarIndex)
    {
        Name = name;
        Candies = candies;
        AvatarIndex = avatarIndex;
    }
}

