using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PlayerItem : MonoBehaviourPunCallbacks
{
    public Player player;
    public TMP_Text playerName;
    public Image playerAvatar;
    public Sprite[] avatars; // Lista de avatares disponibles
    public GameObject leftArrow, rightArrow;

    private ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();

    private void Start()
    {
        if (player == PhotonNetwork.LocalPlayer)
        {
            ApplyLocalChanges(); // Aplica cambios solo si es el jugador local
        }
    }

    public void SetPlayerInfo(Player _player)
    {
        player = _player;
        playerName.text = player.NickName;

        // Si no hay un avatar asignado en las propiedades, inicialízalo
        if (!player.CustomProperties.ContainsKey("playerAvatar"))
        {
            playerProperties["playerAvatar"] = 0; // Avatar por defecto (índice 0)
            player.SetCustomProperties(playerProperties);
        }

        UpdatePlayerItem(); // Actualiza la UI del jugador
    }

    public void ApplyLocalChanges()
    {
        leftArrow.SetActive(true);
        rightArrow.SetActive(true);
    }

    public void OnClickLeftArrow()
    {
        UpdateAvatarIndex(-1); // Disminuye el índice del avatar
    }

    public void OnClickRightArrow()
    {
        UpdateAvatarIndex(1); // Aumenta el índice del avatar
    }

    private void UpdateAvatarIndex(int change)
    {
        int currentAvatarIndex = (int)player.CustomProperties["playerAvatar"];
        currentAvatarIndex = (currentAvatarIndex + change + avatars.Length) % avatars.Length; // Crea un ciclo en los avatares
        playerProperties["playerAvatar"] = currentAvatarIndex;

        // Si el jugador es local, se actualiza sus propiedades
        if (player == PhotonNetwork.LocalPlayer)
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties); // Guarda los cambios en las propiedades del jugador local
        }
        else
        {
            player.SetCustomProperties(playerProperties);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (player == targetPlayer)
        {
            UpdatePlayerItem(); // Actualiza la UI del jugador si es el mismo que ha cambiado las propiedades
        }
    }

    void UpdatePlayerItem()
    {
        if (player.CustomProperties.ContainsKey("playerAvatar"))
        {
            int avatarIndex = (int)player.CustomProperties["playerAvatar"];
            if (avatarIndex >= 0 && avatarIndex < avatars.Length)
            {
                playerAvatar.sprite = avatars[avatarIndex]; // Cambia el avatar basado en el índice
            }
        }
    }
}
