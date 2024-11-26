using UnityEngine;
using TMPro;

public class PlayerScore : MonoBehaviour
{
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI playerScoreText;

    private Photon.Realtime.Player player;

    public Photon.Realtime.Player Player
    {
        get { return player; }
        set { player = value; }
    }

    public void SetPlayerName(string name)
    {
        playerNameText.text = name;
    }

    public void SetScore(int score)
    {
        playerScoreText.text = score.ToString();  // Actualiza el puntaje en el UI
    }
}

