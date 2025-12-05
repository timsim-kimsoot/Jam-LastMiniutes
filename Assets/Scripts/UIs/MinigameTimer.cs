using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MinigameTimerUI : MonoBehaviour
{
    [Header("Timer UI")]
    [SerializeField] Image timerFillImage;
    [SerializeField] TMP_Text timerText;

    [Header("Emotion Sprites")]
    [SerializeField] Image emotionImage;
    [SerializeField] Sprite emotionHappy;
    [SerializeField] Sprite emotionWorried;
    [SerializeField] Sprite emotionPanic;

    void Update()
    {
        if (MinigameManager.Instance == null || MinigameManager.Instance.ActiveGame == null)
            return;

        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        float remaining = MinigameManager.Instance.ActiveGame.GetRemainingTime();
        float limit = MinigameManager.Instance.ActiveGame.GetTimeLimit();

        float t = Mathf.Clamp01(remaining / limit);

        if (timerFillImage != null)
            timerFillImage.fillAmount = t;

        if (timerText != null)
            timerText.text = Mathf.Ceil(remaining).ToString();

        if (emotionImage != null)
        {
            if (t > 0.5f)
                emotionImage.sprite = emotionHappy;
            else if (t > 0.25f)
                emotionImage.sprite = emotionWorried;
            else
                emotionImage.sprite = emotionPanic;
        }
    }
}
