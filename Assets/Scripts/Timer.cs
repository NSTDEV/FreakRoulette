using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public float timer = 0;
    public TMP_Text timerText;

    void Start()
    {
        timer = Time.realtimeSinceStartup;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        timerText.text = "" + timer.ToString("F0");

        if (timer <= 0)
        {
            timer = 0;
        }
    }
}
