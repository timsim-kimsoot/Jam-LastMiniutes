using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinigameManager : MonoBehaviour
{
    [SerializeField] List<MinigameBase> minigamePrefabs;
    [SerializeField] Transform minigameParent;

    [SerializeField] CanvasGroup transitionGroup;
    [SerializeField] float fadeDuration = 0.5f;
    [SerializeField] float betweenGamesDelay = 0.5f;
    [SerializeField] Text titleText;

    MinigameBase currentMinigame;

    void Start()
    {
        StartCoroutine(MainLoop());
    }

    IEnumerator MainLoop()
    {
        yield return FadeFromBlack();

        while (true)
        {
            var prefab = minigamePrefabs[Random.Range(0, minigamePrefabs.Count)];

            if (titleText != null)
                titleText.text = prefab.name;

            yield return FadeToBlack();

            if (currentMinigame != null)
                Destroy(currentMinigame.gameObject);

            currentMinigame = Instantiate(prefab, minigameParent);

            bool done = false;
            currentMinigame.OnFinished += () => done = true;
            currentMinigame.StartGame();

            yield return FadeFromBlack();

            while (!done)
                yield return null;

            yield return new WaitForSeconds(betweenGamesDelay);
        }
    }

    IEnumerator FadeToBlack()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            transitionGroup.alpha = a;
            yield return null;
        }
        transitionGroup.alpha = 1f;
    }

    IEnumerator FadeFromBlack()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            transitionGroup.alpha = a;
            yield return null;
        }
        transitionGroup.alpha = 0f;
    }
}
