using TMPro;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public static PlayerController instance;

    [Header("Movimiento")]
    public float moveSpeed = 5f;
    private Vector2 mInput;
    private Rigidbody2D rb;
    private bool canMove = true; // Nueva variable para controlar el movimiento

    [Header("UI")]
    public TMP_Text candyText, playerName;
    public Animator animatorController;
    private int currentCandies;

    [Header("Avatar")]
    public SpriteRenderer playerAvatarImage;
    public Sprite[] avatars;

    private PhotonView view;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        view = GetComponent<PhotonView>();
        InitializePlayer();
    }

    private void Update()
    {
        if (view.IsMine && canMove) // Solo procesar inputs si el movimiento está habilitado
        {
            ProcessInputs();
        }
    }

    private void FixedUpdate()
    {
        if (canMove) // Solo mover al jugador si puede moverse
        {
            rb.velocity = mInput * moveSpeed;
        }

        animatorController.SetBool("Walking", rb.velocity.sqrMagnitude > 0.01f);

        if (mInput.x != 0)
        {
            playerAvatarImage.flipX = mInput.x > 0;
        }
    }

    private void ProcessInputs()
    {
        mInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
    }

    private void InitializePlayer()
    {
        playerName.text = view.IsMine
            ? PlayerPrefs.GetString("PlayerName", "Player")
            : view.Owner.NickName;

        if (view.IsMine)
        {
            PhotonNetwork.LocalPlayer.NickName = playerName.text;
            int avatarIndex = GetAvatarIndex();
            photonView.RPC(nameof(RPC_UpdateAvatar), RpcTarget.AllBuffered, avatarIndex);
        }

        RPC_SyncCandies(0);
    }

    private int GetAvatarIndex()
    {
        return PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("playerAvatar", out var avatarIndex)
            ? (int)avatarIndex
            : 0;
    }

    [PunRPC]
    public void RPC_UpdateAvatar(int avatarIndex)
    {
        if (avatarIndex >= 0 && avatarIndex < avatars.Length)
        {
            playerAvatarImage.sprite = avatars[avatarIndex];
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Candy"))
        {
            CollectCandy(other.gameObject);
        }
    }

    private void CollectCandy(GameObject candy)
    {
        RPC_IncreaseCandies();

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonView candyPhotonView = candy.GetComponent<PhotonView>();

            if (candyPhotonView != null && candyPhotonView.IsMine)
            {
                photonView.RPC(nameof(RPC_DestroyCandy), RpcTarget.All, candyPhotonView.ViewID);
            }
        }
    }

    [PunRPC]
    private void RPC_DestroyCandy(int viewID)
    {
        PhotonView candyPhotonView = PhotonView.Find(viewID);

        if (candyPhotonView.gameObject != null)
        {
            Destroy(candyPhotonView.gameObject); // Eliminar el objeto de manera segura
        }
        else
        {
            Debug.LogWarning("El caramelo ya fue destruido o no existe.");
        }
    }

    [PunRPC]
    public void RPC_IncreaseCandies()
    {
        currentCandies++;

        photonView.RPC(nameof(RPC_SyncCandies), RpcTarget.All, currentCandies);

        ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable()
    {
        { "Candies", currentCandies }
    };
        PhotonNetwork.LocalPlayer.SetCustomProperties(newProperties);
    }

    [PunRPC]
    private void RPC_SyncCandies(int candies)
    {
        currentCandies = candies;
        candyText.text = currentCandies.ToString();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer == view.Owner && changedProps.TryGetValue("PlayerName", out var newName))
        {
            playerName.text = (string)newName;
        }
    }

    public bool Failed
    {
        get => animatorController.GetBool("Failed");
        set => animatorController.SetBool("Failed", value);
    }

    public void DisableMovement()
    {
        canMove = false; // Desactiva el movimiento
    }

    public void EnableMovement()
    {
        canMove = true; // Activa el movimiento
    }
}
