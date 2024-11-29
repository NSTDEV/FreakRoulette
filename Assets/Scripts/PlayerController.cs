using TMPro;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPunCallbacks
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;
    private Vector2 mInput;
    private Rigidbody2D rb;
    private bool canMove = true; // Variable para controlar el movimiento

    [Header("UI")]
    public TMP_Text candyText, playerName;
    public Animator animatorController;
    private int currentCandies;

    [Header("Avatar")]
    public SpriteRenderer playerAvatarImage;
    public Sprite[] avatars;

    private PhotonView view;

    // Referencia al componente AudioSource
    private AudioSource audioSource;

    // AudioClip para el sonido a reproducir
    public AudioClip clip;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        view = GetComponent<PhotonView>();
        InitializePlayer();

        // Obtener el componente AudioSource en el objeto
        audioSource = GetComponent<AudioSource>();

        // Asegurarse de que el AudioSource tenga un clip asignado
        if (clip != null)
        {
            audioSource.clip = clip; // Asignar el AudioClip
        }

        if (view.IsMine)
        {
            photonView.RPC("RPC_UpdateCandyText", RpcTarget.AllBuffered, currentCandies);
            PhotonNetwork.LocalPlayer.TagObject = gameObject; // Asignar TagObject aquí
        }
    }

    private void Update()
    {
        if (view.IsMine && canMove)
        {
            ProcessInputs();
        }
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            rb.velocity = mInput * moveSpeed;
            animatorController.SetBool("Walking", rb.velocity.sqrMagnitude > 0.01f);
        }
        else
        {
            rb.velocity = Vector2.zero;
            animatorController.SetBool("Walking", false);
        }

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
        EnableMovement();
        playerName.text = view.IsMine
            ? PlayerPrefs.GetString("PlayerName", "Player")
            : view.Owner.NickName;

        if (view.IsMine)
        {
            PhotonNetwork.LocalPlayer.NickName = playerName.text;

            int avatarIndex = GetAvatarIndex();
            photonView.RPC(nameof(RPC_UpdateAvatar), RpcTarget.AllBuffered, avatarIndex);

            InitializeCustomProperties();
        }
        else
        {
            InitializeCustomProperties(view.Owner);
        }
    }

    private void InitializeCustomProperties(Player player = null)
    {
        player = player ?? PhotonNetwork.LocalPlayer;
        var defaultProps = new ExitGames.Client.Photon.Hashtable
        {
            { "Candies", 0 },
            { "IsEliminated", false }
        };

        player.SetCustomProperties(defaultProps);
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
        if ((bool)PhotonNetwork.LocalPlayer.CustomProperties["IsEliminated"])
        {
            return;
        }

        if (other.CompareTag("Candy") && view.IsMine)
        {
            CollectCandy(other.gameObject);
        }
    }

    private void CollectCandy(GameObject candy)
    {
        RPC_IncreaseCandies();
        audioSource.Play(); // Reproduce el sonido
        if (candy != null)
        {
            PhotonView candyPhotonView = candy.GetComponent<PhotonView>();
            candyPhotonView.RPC("TriggerDestruction", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    public void RPC_IncreaseCandies()
    {
        currentCandies++;
        photonView.RPC("RPC_UpdateCandyText", RpcTarget.AllBuffered, currentCandies);

        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
        {
            { "Candies", currentCandies }
        });
    }

    [PunRPC]
    public void RPC_UpdateCandyText(int updatedCandies)
    {
        candyText.text = updatedCandies.ToString();
    }

    public void EnableMovement()
    {
        canMove = true;
        animatorController.SetBool("Failed", false);
        animatorController.SetBool("Walking", mInput.sqrMagnitude > 0.01f);
    }

    public void DisableMovement()
    {
        canMove = false;
        animatorController.SetBool("Failed", true);
        Debug.Log("Movimiento deshabilitado para el jugador.");
    }
}
