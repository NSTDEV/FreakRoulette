using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Linq;
using UnityEngine;

public class InhabilitatePlayer : MonoBehaviourPunCallbacks
{
    public Animator shadowAnimator;
    private string eliminationMessage;
    private bool eliminateExecuted = false;

    public void EliminatePlayerWithLowestPoints()
    {
        if (eliminateExecuted) return;

        var playersToConsider = PhotonNetwork.PlayerList
            .Where(p => (bool)p.CustomProperties["IsEliminated"] == false) // Filtra jugadores no eliminados
            .ToList();

        // Verificar si hay jugadores para eliminar
        if (playersToConsider.Count == 0)
        {
            Debug.Log("No hay jugadores restantes para eliminar.");
            return;
        }

        // Ordena por puntos (Candies) y selecciona el primero
        var playerToEliminate = playersToConsider
            .OrderBy(p => (int)p.CustomProperties["Candies"])
            .FirstOrDefault();

        if (playerToEliminate == null)
        {
            Debug.Log("No se encontró un jugador con puntos para eliminar.");
            return;
        }

        int lowestPoints = (int)playerToEliminate.CustomProperties["Candies"];
        bool isEliminated = Random.value <= 0.5f; // 50% de probabilidad de eliminación

        eliminationMessage = isEliminated
            ? $"{playerToEliminate.NickName} ha sido eliminado con {lowestPoints} puntos."
            : $"{playerToEliminate.NickName} ha sobrevivido con {lowestPoints} puntos.";

        photonView.RPC(nameof(DisplayEliminationMessageRPC), RpcTarget.AllBuffered);

        // Si el jugador es eliminado, procesamos la eliminación
        if (isEliminated)
        {
            photonView.RPC(nameof(HandlePlayerEliminationRPC), RpcTarget.AllBuffered, playerToEliminate.UserId);
        }
        else
        {
            photonView.RPC(nameof(StartFailedAnimationRPC), RpcTarget.AllBuffered);
        }

        // Verificamos si solo queda un jugador después de la eliminación
        GameManager.Instance.CheckRemainingPlayers(); // Verifica si solo queda un jugador y manda a la escena de ganador

        eliminateExecuted = true;
    }

    [PunRPC]
    private void HandlePlayerEliminationRPC(string playerId)
    {
        var player = PhotonNetwork.PlayerList.FirstOrDefault(p => p.UserId == playerId);
        if (player == null) return;

        // Iniciar animación de eliminación (por ejemplo, "Eating")
        photonView.RPC(nameof(StartEatingAnimationRPC), RpcTarget.AllBuffered);

        // Establecer la propiedad de eliminación
        player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "IsEliminated", true } });

        // Llamar al RPC para deshabilitar el movimiento
        photonView.RPC(nameof(DisablePlayerMovementRPC), RpcTarget.AllBuffered, playerId);
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
        if (player == null)
        {
            Debug.LogError($"No se encontró el jugador con ID: {playerId}");
            return;
        }

        if (player.TagObject == null)
        {
            Debug.LogWarning($"El `TagObject` no está configurado para el jugador: {player.NickName}");
            return;
        }

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
