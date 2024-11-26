using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;

public class ScoreboardManager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public Transform scoreboardContainer; // Contenedor del scoreboard en el canvas
    public GameObject playerScorePrefab; // Prefab para los elementos de la lista (con el nombre y puntaje)
    public TMP_Text eliminationMessageText; // Para mostrar el mensaje de eliminación

    private List<PlayerScore> playerScores = new List<PlayerScore>();

    void Start()
    {
        // Inicializa la lista de puntajes al comenzar
        UpdateScoreboard();
    }

    void Update()
    {
                // Verificar si las referencias están asignadas
        if (playerScorePrefab == null)
        {
            Debug.LogError("playerScorePrefab no está asignado en el Inspector.");
            return;
        }

        if (scoreboardContainer == null)
        {
            Debug.LogError("scoreboardContainer no está asignado en el Inspector.");
            return;
        }
        // Actualiza el tablero de puntajes continuamente (puedes optimizar esto dependiendo del juego)
        UpdateScoreboard();
    }

    // Llamado cuando se recibe una actualización de las propiedades personalizadas de un jugador
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey("Candies"))
        {
            UpdateScoreboard();
        }
    }

    private void UpdateScoreboard()
    {
        // Limpiar el contenedor antes de agregar nuevos elementos
        foreach (Transform child in scoreboardContainer)
        {
            Destroy(child.gameObject);
        }

        // Crear una lista de PlayerScore con los puntajes de cada jugador
        playerScores.Clear();
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.ContainsKey("Candies"))
            {
                int candies = (int)player.CustomProperties["Candies"];
                playerScores.Add(new PlayerScore(player.NickName, candies));
            }
        }

        // Ordenar la lista de puntajes utilizando el método Bubble Sort (en orden descendente)
        BubbleSort(playerScores);

        // Crear los elementos UI para cada jugador
        foreach (var playerScore in playerScores)
        {
            GameObject playerScoreObject = Instantiate(playerScorePrefab, scoreboardContainer);
            TMP_Text playerNameText = playerScoreObject.transform.Find("PlayerName").GetComponent<TMP_Text>();
            TMP_Text playerScoreText = playerScoreObject.transform.Find("PlayerScore").GetComponent<TMP_Text>();

            playerNameText.text = playerScore.PlayerName;
            playerScoreText.text = playerScore.Score.ToString();
        }
    }

    // Método Bubble Sort para ordenar los puntajes de los jugadores en orden descendente
    private void BubbleSort(List<PlayerScore> scores)
    {
        int n = scores.Count;
        bool swapped;
        do
        {
            swapped = false;
            for (int i = 0; i < n - 1; i++)
            {
                if (scores[i].Score < scores[i + 1].Score)
                {
                    // Intercambiar los elementos si están en el orden incorrecto
                    PlayerScore temp = scores[i];
                    scores[i] = scores[i + 1];
                    scores[i + 1] = temp;
                    swapped = true;
                }
            }
            n--; // Disminuir el rango de comparación en cada iteración
        } while (swapped);
    }

    // Clase interna para almacenar el nombre y puntaje del jugador
    private class PlayerScore
    {
        public string PlayerName { get; private set; }
        public int Score { get; private set; }

        public PlayerScore(string playerName, int score)
        {
            PlayerName = playerName;
            Score = score;
        }
    }

    // Llamado para mostrar el mensaje de eliminación (si hay)
    [PunRPC]
    public void DisplayEliminationMessage(string message)
    {
        if (eliminationMessageText != null)
        {
            eliminationMessageText.text = message;
        }
    }
}



/*using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ScoreboardManager : MonoBehaviour
{
    public GameObject playerScorePrefab;  // Prefab para las filas de puntaje.
    public Transform scoreboardContainer; // Contenedor donde se instancian las filas.

    private List<GameObject> playerScoreObjects = new List<GameObject>(); // Lista para almacenar las filas de puntaje.

    void Start()
    {
        // Inicializa la tabla de puntajes al principio.
        UpdateScoreboard();
        Debug.Log("Hola");
    }

   // Método para actualizar toda la tabla de puntajes
public void UpdateScoreboard()
{
    // Limpia la tabla de puntajes
    foreach (GameObject scoreObject in playerScoreObjects)
    {
        Destroy(scoreObject);
    }
    playerScoreObjects.Clear();

    // Obtén los jugadores desde Photon Network
    var players = PhotonNetwork.PlayerList;

    // Verifica que los jugadores están siendo obtenidos correctamente
    Debug.Log($"Updating scoreboard with {players.Length} players.");

    // Crea una fila para cada jugador
    foreach (var player in players)
    {
        GameObject playerScore = Instantiate(playerScorePrefab, scoreboardContainer);

        // Asocia el nombre y puntaje
        var playerScoreScript = playerScore.GetComponent<PlayerScore>();
        playerScoreScript.SetPlayerName(player.NickName);

        // Obtén el puntaje del jugador
        int playerScoreValue = GetPlayerScore(player);
        Debug.Log($"Player: {player.NickName} - Score: {playerScoreValue}");  // Verifica que el puntaje se esté recuperando correctamente

        playerScoreScript.SetScore(playerScoreValue);

        // Guarda la fila en la lista
        playerScoreObjects.Add(playerScore);
    }

    // Asegúrate de que el contenedor se actualice visualmente
    LayoutRebuilder.ForceRebuildLayoutImmediate(scoreboardContainer.GetComponent<RectTransform>());
}


    // Obtener el puntaje del jugador desde las propiedades personalizadas.
 public int GetPlayerScore(Photon.Realtime.Player player)
{
    if (player.CustomProperties.ContainsKey("score"))
    {
        int score = (int)player.CustomProperties["score"];
        Debug.Log($"{player.NickName}'s current score: {score}");  // Verifica que se esté recuperando el puntaje correctamente
        return score;
    }
    return 0;
}

    // Establecer el puntaje del jugador.
public void SetPlayerScore(Photon.Realtime.Player player, int newScore)
{
    Hashtable properties = new Hashtable();
    properties.Add("score", newScore);
    player.SetCustomProperties(properties);
    Debug.Log($"{player.NickName}'s score set to {newScore}");  // Verifica que el puntaje se esté actualizando
    UpdateScoreboard();
}

    // Este método actualiza el puntaje de un jugador específico en la tabla.
   public void UpdatePlayerScore(Photon.Realtime.Player player, int newScore)
{
    // Encuentra la fila correspondiente al jugador en la lista de objetos de puntaje.
    GameObject playerScoreObject = playerScoreObjects.Find(obj => obj.GetComponent<PlayerScore>().Player == player);

    if (playerScoreObject != null)
    {
        // Actualiza el puntaje en la fila correspondiente.
        playerScoreObject.GetComponent<PlayerScore>().SetScore(newScore);
    }
}
}
*/