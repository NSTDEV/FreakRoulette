using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviourPunCallbacks
{
    public TMP_Text timerText;
    public Animator transitionAnimator;
    public float transitionDuration = 1.5f;
    public float roundDuration = 40f;
    private float timer;
   
    public GameObject scoreTablePanel; // Referencia al panel donde se mostrarán los puntajes
    public TMP_Text playerScorePrefab; // Prefab de texto para cada jugador (debe ser un TextMeshProUGUI)
    private List<PlayerScore> playerScores = new List<PlayerScore>(); // Lista para almacenar la información de los jugadores
        

    private bool hasSceneChanged = false; // Bandera para evitar múltiples transiciones

    
    void Awake()
    {
        if (FindObjectsOfType<GameManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded; // Escucha cambios de escena
    }

    void Start()
    {
        timer = roundDuration;
    }

    void Update()
    {
        if (hasSceneChanged) return; // Evita ejecutar el cambio de escena más de una vez

        timer -= Time.deltaTime;
        timerText.text = timer.ToString("F0");

        if (timer <= 0)
        {
            timer = 0;
            hasSceneChanged = true; // Marca que ya se procesó el cambio
            SelectPlayerForVersus();
            StartCoroutine(StartGameWithTransition("Versus"));
        }
        UpdateScoreTable();
    }
    //----------------------------------//
    // Método para actualizar la tabla de puntuaciones
void UpdateScoreTable()
{
    playerScores.Clear(); // Limpiamos la lista antes de llenarla nuevamente

    // Recopilamos la información de los jugadores
    foreach (var player in PhotonNetwork.PlayerList)
    {
        if (player.CustomProperties.ContainsKey("Score"))
        {
            int score = (int)player.CustomProperties["Score"];
            playerScores.Add(new PlayerScore(player.NickName, score));
        }
    }

    // Ordenamos usando Bubble Sort
    BubbleSort(playerScores);

    // Limpiamos el panel y creamos nuevos elementos en orden
    foreach (Transform child in scoreTablePanel.transform)
    {
        Destroy(child.gameObject); // Eliminamos elementos previos
    }

    foreach (var playerScore in playerScores)
    {
        TMP_Text scoreText = Instantiate(playerScorePrefab, scoreTablePanel.transform);
        scoreText.text = $"{playerScore.playerName}: {playerScore.score}";
    }
}
    //----------------------------------------------------//
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // Limpia el evento
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Lobby")
        {
            Destroy(gameObject); // Destruye el GameManager al entrar al Lobby
        }
        else if (scene.name == "Versus")
        {
            // Asegura que todos los jugadores estén listos en la escena Versus
            photonView.RPC(nameof(NotifyVersusSceneLoaded), RpcTarget.All);
        }
    }

    void SelectPlayerForVersus()
    {
        Photon.Realtime.Player playerWithLowestScore = null;
        int lowestScore = int.MaxValue;

        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.ContainsKey("Score"))
            {
                int playerScore = (int)player.CustomProperties["Score"];
                if (playerScore < lowestScore)
                {
                    lowestScore = playerScore;
                    playerWithLowestScore = player;
                }
            }
        }

        if (playerWithLowestScore != null)
        {
            AssignVersusRole(playerWithLowestScore);
        }
    }

    void AssignVersusRole(Photon.Realtime.Player player)
    {
        player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
        {
            { "IsVersus", true }
        });

        photonView.RPC(nameof(MoveToVersus), RpcTarget.All, player.UserId);
    }

    [PunRPC]
    void MoveToVersus(string userId)
    {
        if (PhotonNetwork.LocalPlayer.UserId == userId)
        {
            // Verifica si el jugador ya está presente en la escena "Versus"
            GameObject existingPlayer = GameObject.FindWithTag("Player");
            if (existingPlayer != null)
            {
                Debug.Log("El jugador ya está presente en la escena Versus, evitando duplicación.");
                return; // Si ya existe el jugador, no lo duplicamos
            }

            // Si no existe, crea el jugador
            Debug.Log("Instanciando jugador en la escena Versus.");
            PhotonNetwork.Instantiate("PlayerPrefab", Vector3.zero, Quaternion.identity); // Ajusta la posición si es necesario

            // Simula un 50/50 para ganar o perder
            bool survived = Random.value > 0.5f;

            if (!survived)
            {
                Debug.Log("Has perdido el versus.");
                var localPlayerScript = GetLocalPlayerScript();
                if (localPlayerScript != null)
                {
                    localPlayerScript.Failed = true; // Activa el estado de muerte
                }
                StartCoroutine(HandlePlayerDeath());
            }
            else
            {
                StartCoroutine(HandleVersusScene());
            }
        }
        else
        {
            Debug.Log($"Jugador {userId} fue elegido para Versus. Permanecerás en la escena actual.");
        }
    }

    private PlayerController GetLocalPlayerScript()
    {
        GameObject localPlayer = GameObject.FindWithTag("Player");
        if (localPlayer != null)
        {
            return localPlayer.GetComponent<PlayerController>();
        }
        return null;
    }

    private IEnumerator HandleVersusScene()
    {
        yield return new WaitForSeconds(6f);

        foreach (var player in PhotonNetwork.PlayerList)
        {
            player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
            {
                { "Score", 0 }
            });
        }

        timer = roundDuration;

        photonView.RPC(nameof(ReturnToGameScene), RpcTarget.All); // Notifica a todos para regresar
    }

    [PunRPC]
    private void ReturnToGameScene()
    {
        StartCoroutine(StartGameWithTransition("Game"));
    }

    [PunRPC]
    private void NotifyVersusSceneLoaded()
    {
        // Notifica a todos los jugadores que la escena Versus ha cargado
        Debug.Log("La escena Versus ha cargado correctamente.");
    }

    public void LeaveToLobby()
    {
        // Asegura que todos los jugadores dejen la sala y vuelvan al lobby
        PhotonNetwork.LeaveRoom();
        StartCoroutine(StartGameWithTransition("Lobby"));
    }

    private IEnumerator StartGameWithTransition(string levelName)
    {
        if (transitionAnimator != null)
        {
            transitionAnimator.ResetTrigger("Start");
            transitionAnimator.SetTrigger("Start");
        }

        yield return new WaitForSeconds(transitionDuration);

        PhotonNetwork.LoadLevel(levelName);
        yield return null;

        if (transitionAnimator != null)
        {
            transitionAnimator.ResetTrigger("Start");
            transitionAnimator.Play("TransitionIn");
        }
    }

    private IEnumerator HandlePlayerDeath()
    {
        yield return new WaitForSeconds(3f);

        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Lobby");
    }

    //---------------------------------//
    // Clase para almacenar el nombre y la puntuación del jugador
public class PlayerScore
{
    public string playerName;
    public int score;

    public PlayerScore(string name, int score)
    {
        this.playerName = name;
        this.score = score;
    }
}
// Implementación del algoritmo de ordenamiento Bubble Sort
void BubbleSort(List<PlayerScore> list)
{
    int n = list.Count;
    for (int i = 0; i < n - 1; i++)
    {
        for (int j = 0; j < n - 1 - i; j++)
        {
            if (list[j].score < list[j + 1].score)
            {
                // Intercambiar los elementos
                PlayerScore temp = list[j];
                list[j] = list[j + 1];
                list[j + 1] = temp;
            }
        }
    }
}


}
