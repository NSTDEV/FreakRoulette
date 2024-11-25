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
    public Sprite[] avatars;
    public GameObject leftArrow, rightArrow;

    private ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();

    private void Start()
    {
        if (player == PhotonNetwork.LocalPlayer)
        {
            ApplyLocalChanges();
        }
    }

    public void SetPlayerInfo(Player _player)
    {
        player = _player;
        playerName.text = player.NickName;

        if (!player.CustomProperties.TryGetValue("playerAvatar", out _))
        {
            SetAvatarIndex(0);
        }

        UpdatePlayerItem();
    }

    private void ApplyLocalChanges()
    {
        leftArrow.SetActive(true);
        rightArrow.SetActive(true);
    }

    public void OnClickLeftArrow() => UpdateAvatarIndex(-1);

    public void OnClickRightArrow() => UpdateAvatarIndex(1);

    private void UpdateAvatarIndex(int change)
    {
        int currentAvatarIndex = GetAvatarIndex();
        currentAvatarIndex = (currentAvatarIndex + change + avatars.Length) % avatars.Length;

        SetAvatarIndex(currentAvatarIndex);
    }

    private int GetAvatarIndex() => (int)(player.CustomProperties["playerAvatar"] ?? 0);

    private void SetAvatarIndex(int index)
    {
        playerProperties["playerAvatar"] = index;
        player.SetCustomProperties(playerProperties);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (player == targetPlayer && changedProps.ContainsKey("playerAvatar"))
        {
            UpdatePlayerItem();
        }
    }

    private void UpdatePlayerItem()
    {
        int avatarIndex = GetAvatarIndex();
        if (avatarIndex >= 0 && avatarIndex < avatars.Length)
        {
            playerAvatar.sprite = avatars[avatarIndex];
        }
    }
}
