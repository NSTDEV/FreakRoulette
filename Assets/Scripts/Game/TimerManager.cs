using TMPro;
using UnityEngine;
using Photon.Pun;

public class TimerManager : MonoBehaviourPun
{
    [Header("Timer Settings")]
    public float collectDuration = 40f;
    public float versusDuration = 10f;
    private float timer;
    public TMP_Text timerText;

    public event System.Action OnTimerEnded;  // Evento para cuando el temporizador termina

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            timer = collectDuration;
        }
    }

    void Update()
    {
        timer -= Time.deltaTime;
        timerText.text = Mathf.Max(0, timer).ToString("F0");

        if (timer <= 0 && OnTimerEnded != null)  // Comprobar si el temporizador ha terminado
        {
            OnTimerEnded.Invoke();
            timer = 0;
        }
    }

    public float SetTimerVersus()
    {
        Debug.Log($"Timer establecido a: {timer}");
        return timer = versusDuration;
    }

    public float SetTimerCollecting()
    {
        Debug.Log($"Timer establecido a: {timer}");
        return timer = collectDuration;
    }

    public float GetTimer()
    {
        return timer;
    }
}
