using UnityEngine;
using Photon.Pun;
using System.Linq;

public class WeaponFallowMouse : MonoBehaviourPunCallbacks
{
    public Transform player; // Referencia al jugador
    public float orbitRadius = 1.5f; // Radio del círculo
    private new PhotonView photonView;

    void Start()
    {
        photonView = GetComponent<PhotonView>();

        // Verificar que photonView no sea null
        if (photonView == null)
        {
            Debug.LogError("El componente PhotonView no está asignado en este objeto.");
            return;
        }

        // Si player no está asignado manualmente, intenta buscarlo automáticamente
        if (player == null)
        {
            GameObject localPlayer = GameObject.FindWithTag("Player"); // Asegúrate de que el jugador tenga la etiqueta "Player"
            if (localPlayer != null)
            {
                player = localPlayer.transform;
            }
            else
            {
                Debug.LogError("No se encontró un objeto con la etiqueta 'Player'.");
            }
        }
    }

    void Update()
    {
        if (player == null)
        {
            Debug.LogError("Referencia a 'player' es nula.");
            return;
        }

        // Verificar si photonView es mío antes de proceder
        if (!photonView.IsMine) return;

        Vector3 mousePosition = GetMouseWorldPosition();
        if (mousePosition == Vector3.zero) return;

        Vector3 direction = (mousePosition - player.position).normalized;
        Vector3 orbitPosition = player.position + direction * orbitRadius;
        transform.position = orbitPosition;

        // Solo enviar RPC si photonView no es null
        if (photonView != null)
        {
            photonView.RPC(nameof(UpdateWeaponRotation), RpcTarget.AllBuffered, direction);
        }
    }

    [PunRPC]
    private void UpdateWeaponRotation(Vector3 direction)
    {
        if (photonView.IsMine)
        {
            ChangeGunDirection(direction);
        }
    }

    private void ChangeGunDirection(Vector3 direction)
    {
        // Calcular ángulo para la rotación
        float angle = Mathf.Atan2(-direction.y, -direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Ajustar escala horizontal según la dirección del mouse
        Vector3 localScale = transform.localScale;
        localScale.x = direction.x < 0 ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
        transform.localScale = localScale;

        // Corregir el ángulo si el arma está invertida horizontalmente
        if (direction.x > 0)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, angle + 180f);
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (Camera.main == null)
        {
            Debug.LogError("No se encontró una cámara principal (Camera.main).");
            return Vector3.zero;
        }

        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = Mathf.Abs(Camera.main.transform.position.z - player.position.z); // Ajustar al plano del jugador
        return Camera.main.ScreenToWorldPoint(mouseScreenPosition);
    }
}
