using UnityEngine;
using TMPro;

public class SimpleTimer : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI timerText;

    private float elapsedTime = 0f;

    void Update()
    {
        // Increment time
        elapsedTime += Time.deltaTime;

        // Calculate time units
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);

        // Multiply the remainder by 100 to get two digits of milliseconds
        int milliseconds = Mathf.FloorToInt((elapsedTime * 100) % 100);

        // Update UI (Format: 00:00:00)
        // {0:00} = minutes, {1:00} = seconds, {2:00} = milliseconds
        timerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }
}