using TMPro;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    [Header("Timer Settings")]
    public float collectDuration = 40f;
    public float versusDuration = 10f;
    private float timer;
    public TMP_Text timerText;

    public event System.Action OnTimerEnded;  // Evento para cuando el temporizador termina

    void Start()
    {
        timer = collectDuration;
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

    public void SetTimer(float collectDuration)
    {
        timer = collectDuration;
    }

    public float SetTimerVersus()
    {
        return timer = versusDuration;
    }

    public float SetTimerCollecting()
    {
        return timer = collectDuration;
    }

    public float GetTimer()
    {
        return timer;
    }
}
