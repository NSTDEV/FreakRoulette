using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Linq;
using System.Collections;

public class InhabilitatePlayer : MonoBehaviourPunCallbacks
{
    public Animator shadowAnimator;
    private string eliminationMessage;

    public void EliminatePlayerWithLowestPoints()
    {
        // Filtrar jugadores no eliminados y ordenarlos por sus puntos
        var playersToConsider = PhotonNetwork.PlayerList
            .Where(p => !p.CustomProperties.ContainsKey("IsEliminated") || !(bool)p.CustomProperties["IsEliminated"])
            .OrderBy(p => (int)p.CustomProperties["Candies"])
            .ToList();

        // Si solo queda un jugador, no se elimina a nadie más
        if (playersToConsider.Count <= 1)
        {
            Debug.Log("Queda un solo jugador. No se eliminará a nadie.");
            return;
        }

        // Seleccionar al jugador con menor puntaje
        var playerToEliminate = playersToConsider.First();
        int lowestPoints = (int)playerToEliminate.CustomProperties["Candies"];

        Debug.Log($"Jugador con menor puntaje: {playerToEliminate.NickName} ({lowestPoints} puntos)");

        // Eliminar al jugador seleccionado
        photonView.RPC(nameof(HandlePlayerEliminationRPC), RpcTarget.AllBuffered, playerToEliminate.UserId);

        // Verificar si quedan más jugadores
        GameManager.Instance.CheckRemainingPlayers();
    }

    private void CheckRemainingPlayers()
    {
        // Filtra los jugadores no eliminados
        var remainingPlayers = PhotonNetwork.PlayerList
            .Where(p => !p.CustomProperties.ContainsKey("IsEliminated") || !(bool)p.CustomProperties["IsEliminated"])
            .ToList();

        // Si solo queda un jugador, manda a la escena de "Winner"
        if (remainingPlayers.Count == 1)
        {
            photonView.RPC(nameof(SendToWinnerScene), RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void SendToWinnerScene()
    {
        // Aquí podrías agregar la lógica para cambiar a la escena de "Winner"
        Debug.Log("¡Ha quedado un solo jugador! Cambiando a la escena 'Winner'.");
        // SceneManager.LoadScene("Winner"); // Usa esto si estás utilizando SceneManager de Unity
        PhotonNetwork.LoadLevel("Winner"); // Usar esto si estás trabajando con Photon para cargar la escena
    }

    [PunRPC]
    public void HandlePlayerEliminationRPC(string playerId)
    {
        // Buscar al jugador a eliminar basado en su ID
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.UserId == playerId)
            {
                // Verificar si el jugador es el local y deshabilitar su movimiento
                if (player.TagObject is GameObject playerObject)
                {
                    var playerController = playerObject.GetComponent<PlayerController>();
                    if (playerController != null)
                    {
                        playerController.DisableMovement();
                    }
                }
                break;
            }
        }
    }

    [PunRPC]
    private void StartEatingAnimationRPC()
    {
        shadowAnimator.SetTrigger("EatTime");
        StartCoroutine(WaitForAnimation());
    }

    [PunRPC]
    private void StartFailedAnimationRPC()
    {
        shadowAnimator.SetTrigger("Failed");
        StartCoroutine(WaitForAnimation());
    }

    private IEnumerator WaitForAnimation()
    {
        yield return new WaitForSeconds(1.5f);
        shadowAnimator.SetTrigger("BackToIdle");
    }

    [PunRPC]
    public void DisablePlayerMovementRPC(string playerId)
    {
        var player = PhotonNetwork.PlayerList.FirstOrDefault(p => p.UserId == playerId);
        if (player == null) return;

        if (player.TagObject is GameObject playerObj)
        {
            var controller = playerObj.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.DisableMovement();
                Debug.Log($"Movimiento deshabilitado para el jugador: {player.NickName}");
            }
            else
            {
                Debug.LogError($"El objeto del jugador {player.NickName} no tiene un `PlayerController`.");
            }
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
