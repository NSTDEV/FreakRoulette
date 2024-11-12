using TMPro;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public static PlayerController instance;
    public PhotonView view;
    public float mSpeed;
    float speedX, speedY;
    Vector2 mVector;

    public TMP_Text candyText, playerName;
    int currentCandies;
    Rigidbody2D rb;

    public SpriteRenderer playerAvatarImage;
    public Sprite[] avatars;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        candyText.text = currentCandies.ToString();
        view = GetComponent<PhotonView>();

        if (view.IsMine)
        {
            int avatarIndex = (int)PhotonNetwork.LocalPlayer.CustomProperties["playerAvatar"];
            photonView.RPC("RPC_UpdateAvatar", RpcTarget.AllBuffered, avatarIndex);
        }

        if (view.IsMine)
        {
            PhotonNetwork.LocalPlayer.NickName = PlayerPrefs.GetString("PlayerName", "Player");
            playerName.text = PhotonNetwork.LocalPlayer.NickName;
        }
        else
        {
            playerName.text = view.Owner.NickName;
        }

        Debug.Log("Nombre del jugador asignado: " + playerName.text);
    }

    void Update()
    {
        if (view.IsMine)
        {
            ProcessInputs();
        }
    }

    void FixedUpdate()
    {
        rb.velocity = mVector * mSpeed;
    }

    void ProcessInputs()
    {
        speedX = Input.GetAxisRaw("Horizontal");
        speedY = Input.GetAxisRaw("Vertical");

        mVector = new Vector2(speedX, speedY).normalized;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Candy" && view.IsMine)
        {
            view.RPC("RPC_IncreaseCandies", RpcTarget.AllBuffered);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(other.gameObject);
        }
    }

    public void IncreaseCandies()
    {
        if (view.IsMine)
        {
            view.RPC("RPC_IncreaseCandies", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void RPC_UpdateAvatar(int avatarIndex)
    {
        if (avatarIndex >= 0 && avatarIndex < avatars.Length)
        {
            playerAvatarImage.sprite = avatars[avatarIndex];
        }
        else
        {
            Debug.LogError("Índice de avatar fuera de rango.");
        }
    }

    [PunRPC]
    void RPC_IncreaseCandies()
    {
        currentCandies++;
        candyText.text = currentCandies.ToString();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer == view.Owner && changedProps.ContainsKey("NickName"))
        {
            playerName.text = targetPlayer.NickName;
        }
    }

    public void SetAvatar(int avatarIndex)
    {
        if (avatarIndex >= 0 && avatarIndex < avatars.Length)
        {
            playerAvatarImage.sprite = avatars[avatarIndex];
        }
        else
        {
            Debug.LogError("Índice de avatar fuera de rango.");
        }
    }
}