/*using UnityEngine;
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
}*/

/*using UnityEngine;
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
        // Actualiza el tablero de puntajes continuamente
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
            // Instanciar un nuevo prefab para cada jugador
            GameObject playerScoreObject = Instantiate(playerScorePrefab, scoreboardContainer);
            TMP_Text playerNameText = playerScoreObject.transform.Find("PlayerName").GetComponent<TMP_Text>();
            TMP_Text playerScoreText = playerScoreObject.transform.Find("PlayerScore").GetComponent<TMP_Text>();

            playerNameText.text = playerScore.PlayerName;
            playerScoreText.text = playerScore.Score.ToString();
        }
                Debug.Log("Actualizando puntajes...");
        foreach (var playerScore in playerScores)
        {
            Debug.Log($"{playerScore.PlayerName}: {playerScore.Score}");
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
*/

using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

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

        // Actualiza el tablero de puntajes continuamente
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
            // Instanciar un nuevo prefab para cada jugador
            GameObject playerScoreObject = Instantiate(playerScorePrefab, scoreboardContainer);
            TMP_Text playerNameText = playerScoreObject.transform.Find("PlayerName").GetComponent<TMP_Text>();
            TMP_Text playerScoreText = playerScoreObject.transform.Find("PlayerScore").GetComponent<TMP_Text>();

            playerNameText.text = playerScore.PlayerName;
            playerScoreText.text = playerScore.Score.ToString();
        }
        
        Debug.Log("Actualizando puntajes...");
        foreach (var playerScore in playerScores)
        {
            Debug.Log($"{playerScore.PlayerName}: {playerScore.Score}");
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
