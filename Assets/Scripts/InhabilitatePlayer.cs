using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using UnityEngine;

public class InhabilitatePlayer : MonoBehaviourPunCallbacks
{
    private string eliminationMessage;
    public bool eliminateExecuted = false;

    private void Start()
    {
        Debug.Log("Shadow ACTIVE");
    }

    public void EliminatePlayerWithLowestPoints()
    {
        // Buscar al jugador con menos puntos
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
        Debug.Log("Buscando jugador con menos puntos...");
        Debug.Log($"Jugador eliminado: {playerToEliminate.NickName} con {lowestPoints} puntos.");


        eliminateExecuted = true;
    }

    [PunRPC]
    private void HandlePlayerEliminationRPC(string playerId)
    {
        var player = PhotonNetwork.PlayerList.FirstOrDefault(p => p.UserId == playerId);
        if (player == null) return;

        player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "IsEliminated", true } });

        // Aquí, actualizamos el estado del movimiento en todos los clientes
        photonView.RPC(nameof(DisablePlayerMovementRPC), RpcTarget.AllBuffered, player.UserId);

        Debug.Log($"{player.NickName} ha sido eliminado.");
    }

    [PunRPC]
    public void DisablePlayerMovementRPC(string playerId)
    {
        var player = PhotonNetwork.PlayerList.FirstOrDefault(p => p.UserId == playerId);
        if (player == null) return;

        if (player.TagObject is GameObject playerObj && playerObj.GetComponent<PlayerController>() is PlayerController controller)
        {
            controller.DisableMovement(); // Deshabilitar el movimiento
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
