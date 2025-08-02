using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private float startingTime = 10f; // e.g. 60 seconds

    private float currentTime;
    private bool isRunning = false;

    public bool IsTimeUp => currentTime <= 0f;

    private void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isRunning = false;

            //TODO:Needs better logic, just restarting now
            //for mvp purposes
            GameManager.Instance.RestartGame(); 
        }

        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    public void StartTimer()
    {
        currentTime = startingTime;
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        currentTime = startingTime;
        UpdateTimerUI();
    }

    public float GetRemainingTime()
    {
        return currentTime;
    }
}
