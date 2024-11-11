using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PlayerItem : MonoBehaviourPunCallbacks
{
    Player player;
    public TMP_Text playerName;
    public Image playerAvatar;
    public Sprite[] avatars;
    public Image backgroundImage;
    public Color highlightColor;
    public GameObject leftArrow, rightArrow;

    ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();

    private void Start()
    {
        backgroundImage = GetComponent<Image>();
    }

    public void SetPlayerInfo(Player _player)
    {
        playerName.text = _player.NickName;
        player = _player;

        // Verifica si `playerAvatar` ya está configurado, si no, inicialízalo en 0
        if (!player.CustomProperties.ContainsKey("playerAvatar"))
        {
            playerProperties["playerAvatar"] = 0;
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
        }

        UpdatePlayerItem(player);
    }

    public void ApplyLocalChanges()
    {
        backgroundImage.color = highlightColor;
        leftArrow.SetActive(true);
        rightArrow.SetActive(true);
    }

    public void OnClickLeftArrow()
    {
        int currentAvatarIndex = (int)playerProperties["playerAvatar"];
        playerProperties["playerAvatar"] = (currentAvatarIndex == 0) ? avatars.Length - 1 : currentAvatarIndex - 1;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }

    public void OnClickRightArrow()
    {
        int currentAvatarIndex = (int)playerProperties["playerAvatar"];
        playerProperties["playerAvatar"] = (currentAvatarIndex == avatars.Length - 1) ? 0 : currentAvatarIndex + 1;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (player == targetPlayer)
        {
            UpdatePlayerItem(targetPlayer);
        }
    }

    void UpdatePlayerItem(Player player)
    {
        // Solo asigna el avatar si el índice es válido
        if (player.CustomProperties.ContainsKey("playerAvatar"))
        {
            int avatarIndex = (int)player.CustomProperties["playerAvatar"];
            if (avatarIndex >= 0 && avatarIndex < avatars.Length)
            {
                playerAvatar.sprite = avatars[avatarIndex];
                playerProperties["playerAvatar"] = avatarIndex;
            }
        }
    }
}
