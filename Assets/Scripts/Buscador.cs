using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PlayerInfoLogger : MonoBehaviourPunCallbacks
{
    void Start()
    {
        // Verificamos si PhotonNetwork está conectado y listo
        if (PhotonNetwork.IsConnectedAndReady)
        {
            // Llamamos al método para mostrar la información de los jugadores
            LogPlayerInfo();
        }
    }

    // Método para registrar la información de todos los jugadores
    private void LogPlayerInfo()
    {
        // Iteramos sobre todos los jugadores conectados a la sala
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            // Obtener el nombre del jugador
            string playerName = player.NickName;

            // Intentar obtener la propiedad personalizada 'Candies' de cada jugador
            if (player.CustomProperties.ContainsKey("Candies"))
            {
                // Si la propiedad existe, la obtenemos
                int currentCandies = (int)player.CustomProperties["Candies"];

                // Mostrar la información en la consola
                Debug.Log($"Jugador: {playerName}, Caramelos: {currentCandies}");
            }
            else
            {
                // Si no se encuentra la propiedad 'Candies', asignamos un valor predeterminado
                Debug.LogWarning($"No se encontró la propiedad 'Candies' para el jugador {playerName}. Asignando valor predeterminado: 0");
                Debug.Log($"Jugador: {playerName}, Caramelos: 0");
            }
        }
    }
}


