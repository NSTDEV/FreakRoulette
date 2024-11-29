using TMPro;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

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

    private void Awake() => instance = this;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        view = GetComponent<PhotonView>();
        InitializePlayer();
    }

    private void Update()
    {
        if (view.IsMine)
        {
            CheckPlayerEliminated();

            if (canMove)
            {
                ProcessInputs();
            }
        }
    }

    private void FixedUpdate()
    {
        if (canMove)
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

            // Asigna un avatar para el jugador (si es necesario)
            int avatarIndex = GetAvatarIndex();
            photonView.RPC(nameof(RPC_UpdateAvatar), RpcTarget.AllBuffered, avatarIndex);

            // Inicializa las propiedades del jugador si no están presentes
            InitializeCustomProperties();
        }
        else
        {
            // Asegúrate de que otros jugadores también tengan las propiedades necesarias
            InitializeCustomProperties(view.Owner);
        }
    }

    private void InitializeCustomProperties(Player player = null)
    {
        player = player ?? PhotonNetwork.LocalPlayer;

        if (!player.CustomProperties.ContainsKey("Candies"))
        {
            player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "Candies", 0 } });
        }

        if (!player.CustomProperties.ContainsKey("IsEliminated"))
        {
            player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "IsEliminated", false } });
        }
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

    private void CheckPlayerEliminated()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("IsEliminated") &&
            (bool)PhotonNetwork.LocalPlayer.CustomProperties["IsEliminated"])
        {
            DisableMovement();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((bool)PhotonNetwork.LocalPlayer.CustomProperties["IsEliminated"])
        {
            return;
        }

        if (other.CompareTag("Candy") && view.IsMine || PhotonNetwork.IsMasterClient)
        {
            CollectCandy(other.gameObject);
        }
    }

    private void CollectCandy(GameObject candy)
    {
        RPC_IncreaseCandies();

        if (candy != null) // Verifica que el caramelo aún existe
        {
            // Obtenemos el script Candy del objeto
            Candy candyScript = candy.GetComponent<Candy>();
            if (candyScript != null)
            {
                candyScript.TriggerDestruction();
                photonView.RPC(nameof(RPC_DestroyCandy), RpcTarget.AllBuffered, candy.GetPhotonView().ViewID);
            }
        }
    }

    [PunRPC]
    private void RPC_DestroyCandy(int viewID)
    {
        PhotonView candyPhotonView = PhotonView.Find(viewID);

        if (candyPhotonView != null)
        {
            if (candyPhotonView.IsMine || PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(candyPhotonView.gameObject); // Destruir el objeto de manera sincronizada
            }
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

        // Actualizar propiedades personalizadas
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
        if (targetPlayer != null && targetPlayer == view.Owner && changedProps.TryGetValue("PlayerName", out var newName))
        {
            playerName.text = (string)newName;
        }
    }

    public void DisableMovement()
    {
        canMove = false; // Desactiva el movimiento
        animatorController.SetBool("Failed", true);
        rb.velocity = Vector2.zero;
    }
}
