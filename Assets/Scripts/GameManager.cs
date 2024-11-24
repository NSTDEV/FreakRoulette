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

        // Destruye el GameManager si la escena es "Lobby"
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            Destroy(gameObject);
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
        // Asignar la propiedad "IsVersus" al jugador seleccionado
        player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
        {
            { "IsVersus", true }
        });

        // Notificar a todos los jugadores quién fue seleccionado
        photonView.RPC(nameof(MoveToVersus), RpcTarget.All, player.UserId);
    }

    [PunRPC]
    void MoveToVersus(string userId)
    {
        if (PhotonNetwork.LocalPlayer.UserId == userId)
        {
            // Solo el jugador seleccionado cambia a la escena "Versus"
            StartCoroutine(StartGameWithTransition("Versus"));
        }
        else
        {
            Debug.Log($"Jugador {userId} fue elegido para Versus. Permanecerás en la escena actual.");
        }
    }

    public void LeaveToLobby()
    {
        StartCoroutine(StartGameWithTransition("Lobby"));
    }

    private IEnumerator StartGameWithTransition(string levelName)
    {
        transitionAnimator.ResetTrigger("Start");
        transitionAnimator.SetTrigger("Start");

        yield return new WaitForSeconds(transitionDuration);

        PhotonNetwork.LoadLevel(levelName);

        yield return null; // Espera un frame para garantizar que la nueva escena cargue completamente

        if (transitionAnimator != null)
        {
            transitionAnimator.ResetTrigger("Start");
            transitionAnimator.Play("TransitionIn");
        }
    }
}
