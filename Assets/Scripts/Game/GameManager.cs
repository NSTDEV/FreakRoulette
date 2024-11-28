using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }
    private TimerManager timer;

    [Header("Transition")]
    public Animator transitionAnimator;
    public float transitionDuration = 2f;
    private bool isSceneEnter;

    [Header("Shadow Object")]
    public GameObject shadow;

    private string eliminationMessage;
    private bool eliminateExecuted = false;
    private bool isRoundTransitioning = false;
    private bool isCandyRound;

    void Start()
    {
        timer = FindObjectOfType<TimerManager>();
        isCandyRound = true;
        CollectingRound();
    }

    private void Update()
    {
        if (isRoundTransitioning) return;

        if (timer.GetTimer() <= 0)
        {
            StartCoroutine(SwitchRound(!isCandyRound)); // Alternar ronda
        }
    }

    private IEnumerator SwitchRound(bool isCandy)
    {
        isRoundTransitioning = true;

        SetTransitionState(true);
        yield return new WaitForSeconds(transitionDuration);

        if (isCandy)
        {
            CollectingRound();
        }
        else
        {
            VersusRound();
        }

        isRoundTransitioning = false;
    }

    private void CollectingRound()
    {
        SetTransitionState(false); // Salir de SceneEnter
        timer.SetTimerCollecting();
        ToggleCandyGenerator(true);
        shadow.SetActive(false);
        isCandyRound = true;
    }

    private void VersusRound()
    {
        SetTransitionState(false); // Salir de SceneEnter
        timer.SetTimerVersus();
        ToggleCandyGenerator(false);
        shadow.SetActive(true);

        if (!eliminateExecuted && timer.GetTimer() <= (timer.versusDuration / 2))
        {
            EliminatePlayerWithLowestPoints();
            eliminateExecuted = true;
        }

        isCandyRound = false;
    }

    private void SetTransitionState(bool state)
    {
        transitionAnimator.SetBool("isSceneEnter", state);
    }

    private void ToggleCandyGenerator(bool state)
    {
        Debug.Log($"Candy Generator {(state ? "Activado" : "Desactivado")}");
    }

    public void EliminatePlayerWithLowestPoints()
    {
        var playerToEliminate = PhotonNetwork.PlayerList
            .Where(p => p.CustomProperties.ContainsKey("Candies"))
            .OrderBy(p => (int)p.CustomProperties["Candies"])
            .FirstOrDefault();

        if (playerToEliminate == null)
        {
            Debug.Log("No se encontró un jugador con puntos para eliminar.");
            return;
        }

        int lowestPoints = (int)playerToEliminate.CustomProperties["Candies"];
        bool isEliminated = Random.value <= 0.5f;
        eliminationMessage = isEliminated
            ? $"{playerToEliminate.NickName} ha sido eliminado con {lowestPoints} puntos."
            : $"{playerToEliminate.NickName} ha sobrevivido con {lowestPoints} puntos.";

        photonView.RPC(nameof(DisplayEliminationMessageRPC), RpcTarget.AllBuffered);
        if (isEliminated)
        {
            photonView.RPC(nameof(HandlePlayerEliminationRPC), RpcTarget.AllBuffered, playerToEliminate.UserId);
        }
    }

    [PunRPC]
    private void HandlePlayerEliminationRPC(string playerId)
    {
        var player = PhotonNetwork.PlayerList.FirstOrDefault(p => p.UserId == playerId);
        if (player == null) return;

        player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "IsEliminated", true } });

        if (player.TagObject is PhotonView view &&
            view.GetComponent<PlayerController>() is PlayerController controller)
        {
            controller.DisableMovement();
        }

        Debug.Log($"{player.NickName} ha sido eliminado.");
    }

    [PunRPC]
    private void DisplayEliminationMessageRPC()
    {
        if (!string.IsNullOrEmpty(eliminationMessage))
        {
            Debug.Log(eliminationMessage);
        }
    }

    public void ChangeToLobby()
    {
        StartCoroutine(TransitionToLobby());
    }

    private IEnumerator TransitionToLobby()
    {
        SetTransitionState(true);
        yield return new WaitForSeconds(transitionDuration);
        SceneManager.LoadScene("Lobby");
        PhotonNetwork.LeaveRoom();
    }
}