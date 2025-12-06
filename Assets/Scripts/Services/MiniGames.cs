using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    [Header("Refs")]
    public MinigameBase[] minigamePrefabs;
    public Transform miniGameContainer;

    int currentPhase = 0;
    float difficulty = 0f;
    float difficultyIncrease = 0.25f;

    MinigameBase activeGame;

    float currentGameTimeLimit;

    public MinigameBase ActiveGame => activeGame;

    [Header("Transition UI")]
    [SerializeField] CanvasGroup transitionGroup;
    [SerializeField] TMP_Text transitionText;
    [SerializeField] float fadeDuration = 0.3f;
    [SerializeField] float readyHold = 0.5f;
    [SerializeField] float goHold = 0.35f;

    void Start()
    {
        if (transitionGroup != null)
        {
            transitionGroup.alpha = 1f;
        }

        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        while (true)
        {
            yield return PlayPhase();
            currentPhase++;

            if (currentPhase >= 4)
                currentPhase = 0;
        }
    }

    IEnumerator PlayPhase()
    {
        MinigameBase selected = PickMinigameForPhase(currentPhase);

        activeGame = Instantiate(selected, miniGameContainer);
        activeGame.OnWin += HandleWin;
        activeGame.OnFail += HandleFail;

        activeGame.enabled = false;

        activeGame.Init(difficulty);
        currentGameTimeLimit = activeGame.GetTimeLimit();

        yield return ShowReadyIntro();

        activeGame.enabled = true;

        while (activeGame != null)
        {
            yield return null;
        }

        yield return FadeToBlack();
    }

    MinigameBase PickMinigameForPhase(int phase)
    {
        return minigamePrefabs[phase];
    }

    void HandleWin()
    {
        Debug.Log("EPIC VICTORY!");
        difficulty += difficultyIncrease;
        Cleanup();
    }

    void HandleFail()
    {
        Debug.Log("STUPID FAIL!");
        Cleanup();
    }

    void Cleanup()
    {
        if (activeGame != null)
        {
            Destroy(activeGame.gameObject);
            activeGame = null;
        }
    }

    IEnumerator ShowReadyIntro()
    {
        if (transitionGroup == null) yield break;

        transitionGroup.gameObject.SetActive(true);
        transitionGroup.alpha = 1f;

        if (transitionText != null)
            transitionText.text = "READY";

        yield return new WaitForSeconds(readyHold);

        if (transitionText != null)
            transitionText.text = "GO!";

        yield return new WaitForSeconds(goHold);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            transitionGroup.alpha = a;
            yield return null;
        }

        transitionGroup.alpha = 0f;
        if (transitionText != null)
            transitionText.text = "";
    }

    IEnumerator FadeToBlack()
    {
        if (transitionGroup == null) yield break;

        transitionGroup.gameObject.SetActive(true);
        if (transitionText != null)
            transitionText.text = "";

        float t = 0f;
        float startA = transitionGroup.alpha;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(startA, 1f, t / fadeDuration);
            transitionGroup.alpha = a;
            yield return null;
        }

        transitionGroup.alpha = 1f;
    }
}
