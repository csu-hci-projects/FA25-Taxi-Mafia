using UnityEngine;
using TMPro; // if using TextMeshPro
using UnityEngine.UI; // if using legacy Text

public class TimerController : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI timerText; // use this if TMP
    // public Text timerText; // uncomment and use instead if using legacy UI Text

    [Header("Settings")]
    public float startTime = 0f;      // initial time in seconds
    public bool countDown = false;    // false = count up, true = count down
    public float countdownFrom = 60f; // only used if countDown == true

    private float currentTime;
    private bool running;

    void Awake()
    {
        currentTime = countDown ? countdownFrom : startTime;
        UpdateTimerUI();
    }

    void Update()
    {



        if (!running) return;

        float delta = Time.deltaTime;
        currentTime += (countDown ? -delta : delta);

        if (countDown && currentTime <= 0f)
        {
            currentTime = 0f;
            StopTimer();
            OnTimerFinished();
        }

        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        // Ensure we don't display negative values (can happen with floating point math)
        float displayTime = Mathf.Max(0f, currentTime);
        // If timerText is not assigned, nothing to do here
        if (timerText == null) return;

        // Small epsilon to treat very small values as zero
        const float zeroEpsilon = 0.0001f;
        bool isZero = displayTime <= zeroEpsilon;
        // Hide the TextMeshProUGUI when time is zero
        timerText.gameObject.SetActive(!isZero);

        if (isZero)
        {
            // If hidden, no need to update text
            timerText.text = string.Empty;
            return;
        }
        int minutes = Mathf.FloorToInt(displayTime / 60f);
        int seconds = Mathf.FloorToInt(displayTime % 60f);
        // Centiseconds = hundredths of a second (0-99)
        int centiseconds = Mathf.FloorToInt((displayTime - Mathf.Floor(displayTime)) * 100f);
        centiseconds = Mathf.Clamp(centiseconds, 0, 99);
        // Format as MM:SS:CS (centiseconds)
        timerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, centiseconds);
    }

    public void StartTimer()
    {
        running = true;
    }

    public void PauseTimer()
    {
        running = false;
    }

    public void StopTimer()
    {
        running = false;
        // reset to start value
        currentTime = countDown ? countdownFrom : startTime;
        UpdateTimerUI();
    }

    public float GetCurrentTime() => currentTime;

    protected virtual void OnTimerFinished()
    {
        Debug.Log("Timer finished.");
        // Hook additional behavior here or override in a subclass
    }
}
