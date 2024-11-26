using UnityEngine;
using Photon.Pun;

public class WeaponOrbitPlayer : MonoBehaviourPunCallbacks
{
    public Transform player; // Referencia al jugador
    public float orbitRadius = 1.5f; // Radio del círculo
    private PhotonView photonView;

    void Start()
    {
        photonView = GetComponent<PhotonView>();

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
        if (!photonView.IsMine || player == null) return; // Evitar errores si player es nulo

        Vector3 mousePosition = GetMouseWorldPosition();

        // Calcular dirección desde el jugador hacia el mouse
        Vector3 direction = (mousePosition - player.position).normalized;

        // Mantener el arma exactamente en el perímetro del círculo
        Vector3 orbitPosition = player.position + direction * orbitRadius;
        transform.position = orbitPosition;

        // Rotar el arma para que apunte hacia afuera del círculo
        float angle = Mathf.Atan2(-direction.y, -direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Invertir escala horizontal si el mouse está a la izquierda
        Vector3 localScale = transform.localScale;
        localScale.x = direction.x < 0 ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
        transform.localScale = localScale;

        ChangeGunDirection(direction);
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
