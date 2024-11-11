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
    }

    // Método llamado solo en el jugador local
    public void IncreaseCandies()
    {
        if (view.IsMine)
        {
            view.RPC("RPC_IncreaseCandies", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void RPC_IncreaseCandies()
    {
        currentCandies++;
        candyText.text = currentCandies.ToString();
    }

    // Escuchar y actualizar el nombre si cambia después de la conexión
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer == view.Owner && changedProps.ContainsKey("NickName"))
        {
            playerName.text = targetPlayer.NickName;
        }
    }
}
