using TMPro;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public static PlayerController instance;

    [Header("Movimiento")]
    public float moveSpeed = 5;
    private Vector2 mInput;
    private Rigidbody2D rb;

    [Header("UI")]
    public TMP_Text candyText, playerName;
    private int currentCandies;

    [Header("Avatar")]
    public SpriteRenderer playerAvatarImage;
    public Sprite[] avatars;

    private PhotonView view;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        view = GetComponent<PhotonView>();

        InitializePlayer();
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
        rb.velocity = mInput * moveSpeed;
    }

    void ProcessInputs()
    {
        mInput.x = Input.GetAxisRaw("Horizontal");
        mInput.y = Input.GetAxisRaw("Vertical");

        mInput.Normalize();
    }

    private void InitializePlayer()
    {
        if (view.IsMine)
        {
            playerName.text = PlayerPrefs.GetString("PlayerName", "Player");
            PhotonNetwork.LocalPlayer.NickName = playerName.text;

            int avatarIndex = GetAvatarIndex();
            photonView.RPC(nameof(RPC_UpdateAvatar), RpcTarget.AllBuffered, avatarIndex);
        }
        else
        {
            playerName.text = view.Owner.NickName;
        }

        candyText.text = currentCandies.ToString();
        Debug.Log("Nombre del jugador asignado: " + playerName.text);
    }

    private int GetAvatarIndex()
    {
        return PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("playerAvatar", out var avatarIndex)
            ? (int)avatarIndex
            : 0; // Si no se encuentra el avatar, usar 0 por defecto
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (view.IsMine && other.CompareTag("Candy"))
        {
            CollectCandy(other.gameObject);
        }
    }

    private void CollectCandy(GameObject candy)
    {
        view.RPC(nameof(RPC_IncreaseCandies), RpcTarget.AllBuffered);

        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(candy);
    }

    [PunRPC]
    public void RPC_UpdateAvatar(int avatarIndex)
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
    public void RPC_IncreaseCandies()
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
}
