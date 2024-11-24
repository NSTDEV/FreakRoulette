using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviourPunCallbacks
{
    public TMP_Text timerText;
    public Animator transitionAnimator;
    public float transitionDuration = 1.5f;
    public float roundDuration = 40f;
    private float timer;

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
    }

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
}
