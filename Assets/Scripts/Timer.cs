using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    private float timer;

    [SerializeField] 
    private TextMeshProUGUI firstMinute;
    [SerializeField] 
    private TextMeshProUGUI secondMinute;
    [SerializeField] 
    private TextMeshProUGUI separator;
    [SerializeField] 
    private TextMeshProUGUI firstSecond;
    [SerializeField] 
    private TextMeshProUGUI secondSecond;

    void Awake()
    {
        timer = 0;
        SetTextDisplay(true);
    }

    void Update()
    {
        timer += Time.deltaTime;
        GameManager.Instance.Time = timer;
        UpdateTimerDisplay(timer);
    }

    private void UpdateTimerDisplay(float time) {
        if (time < 0) {
            time = 0;
        }

        if (time > 3660) {
            Debug.LogError("Timer cannot display values above 3660 seconds");
            ErrorDisplay();
            return;
        }

        float minutes = Mathf.FloorToInt(time / 60);
        float seconds = Mathf.FloorToInt(time % 60);

        string currentTime = string.Format("{00:00}{01:00}", minutes, seconds);
        firstMinute.text = currentTime[0].ToString();
        secondMinute.text = currentTime[1].ToString();
        firstSecond.text = currentTime[2].ToString();
        secondSecond.text = currentTime[3].ToString();
    }

    private void ErrorDisplay() {
        firstMinute.text = "8";
        secondMinute.text = "8";
        firstSecond.text = "8";
        secondSecond.text = "8";
    }

    private void SetTextDisplay(bool enabled) {
        firstMinute.enabled = enabled;
        secondMinute.enabled = enabled;
        separator.enabled = enabled;
        firstSecond.enabled = enabled;
        secondSecond.enabled = enabled;
    }
}
