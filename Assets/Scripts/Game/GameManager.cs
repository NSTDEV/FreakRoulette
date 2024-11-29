using UnityEngine;
using Photon.Pun;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }
    private TimerManager timer;
    private GameObject candyGenerator;

    [Header("Transition")]
    public Animator transitionAnimator;
    public float transitionDuration = 2f;

    [Header("Shadow Object")]
    public GameObject shadow;

    private bool isRoundTransitioning = false;
    private bool isCandyRound;
    private InhabilitatePlayer eliminatePlayer;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        timer = FindObjectOfType<TimerManager>();
        candyGenerator = FindObjectOfType<CandyGenerator>()?.gameObject;

        isCandyRound = true;
    }

    private void Update()
    {
        if (isRoundTransitioning) return;

        if (SceneManager.GetActiveScene().name != "Winner")
        {
            if (timer.GetTimer() <= 0)
            {
                StartCoroutine(SwitchRound(!isCandyRound)); // Alternar ronda
            }
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
        ToggleCandyGenerator(true);  // Activa la generación de caramelos solo en esta ronda
        shadow.SetActive(false);

        candyGenerator = FindObjectOfType<CandyGenerator>()?.gameObject;
        isCandyRound = true;
    }

    private void VersusRound()
    {
        SetTransitionState(false); // Salir de SceneEnter
        timer.SetTimerVersus();
        ToggleCandyGenerator(false); // Desactiva la generación de caramelos en esta ronda
        shadow.SetActive(true);

        eliminatePlayer = FindObjectOfType<InhabilitatePlayer>();
        isCandyRound = false;
        StartCoroutine(CheckHalfTime());
    }

    private IEnumerator CheckHalfTime()
    {
        float currentHalfTime = timer.versusDuration / 2;
        while (timer.GetTimer() > currentHalfTime)
        {
            yield return null;
        }

        Debug.Log("Intentando eliminar a un jugador...");
        eliminatePlayer.EliminatePlayerWithLowestPoints();
    }

    private void ToggleCandyGenerator(bool state)
    {
        if (candyGenerator != null)
        {
            candyGenerator.SetActive(state); // Activa o desactiva el objeto

            if (state && candyGenerator.TryGetComponent(out CandyGenerator generator))
            {
                generator.ResetGenerator();
            }
            else if (!state && candyGenerator.TryGetComponent(out CandyGenerator stopGenerator))
            {
                stopGenerator.StopGeneration(); // Detiene la generación de caramelos
            }

            Debug.Log($"Candy Generator {(state ? "Activado" : "Desactivado")}");
        }
        else
        {
            Debug.LogError("CandyGenerator no encontrado.");
        }
    }

    private void SetTransitionState(bool state)
    {
        transitionAnimator.SetBool("isSceneEnter", state);
    }

    public void ChangeToLobby()
    {
        StartCoroutine(TransitionTo("Lobby"));
    }

    private IEnumerator TransitionTo(string sceneName)
    {
        SetTransitionState(true);

        yield return new WaitForSeconds(transitionDuration);
        PhotonNetwork.LeaveRoom();

        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(sceneName);
    }

    // Función que detecta si solo queda un jugador
    public void CheckRemainingPlayers()
    {
        // Contamos los jugadores activos (no eliminados)
        var activePlayers = PhotonNetwork.PlayerList
            .Where(p => !(bool)p.CustomProperties["IsEliminated"])
            .ToArray();

        if (activePlayers.Length == 1)
        {
            photonView.RPC("LoadWinnerScene", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    public void LoadWinnerScene()
    {
        StartCoroutine(TransitionTo("Winner"));
    }
}
