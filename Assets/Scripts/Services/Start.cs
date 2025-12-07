using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartMenuUI : MonoBehaviour
{
    public static StartMenuUI Instance { get; private set; }

    [Header("Roots")]
    [SerializeField] GameObject startMenuRoot;
    [SerializeField] GameObject comicRoot;

    [Header("Comic Panels")]
    [SerializeField] List<ComicPanel> comicPanels = new();

    [Header("Buttons")]
    [SerializeField] Button startButton;
    [SerializeField] Button skipButton;

    bool hasStarted = false;

    void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        startButton.onClick.AddListener(OnStartPressed);
        skipButton.onClick.AddListener(OnSkipPressed);

        startMenuRoot.SetActive(true);
        comicRoot.SetActive(false);
    }
    void Start()
    {
        MusicManager.Instance?.PlayTitleMusic();
    }

    void OnStartPressed()
    {
        if (hasStarted) return;
        hasStarted = true;

        startMenuRoot.SetActive(false);
        comicRoot.SetActive(true);
        PlayComicSequence();

        MusicManager.Instance?.PlayComicMusic();
    }

    void OnSkipPressed()
    {
        StartCoroutine(BeginGameplay());
    }

    public void OpenStartMenu()
    {
        comicRoot.SetActive(true);
        startMenuRoot.SetActive(false);

        MusicManager.Instance?.PlayComicMusic();

        hasStarted = false;
    }

    public void OpenComic()
    {
        startMenuRoot.SetActive(false);
        comicRoot.SetActive(true);
    }
    public void PlayComicSequence()
    {
        foreach (var panel in comicPanels)
        {
            panel.panel.gameObject.SetActive(false);
        }

        StartCoroutine(PlayComicSequenceCoroutine());
    }

    IEnumerator PlayComicSequenceCoroutine()
    {
        foreach (var panel in comicPanels)
        {
            yield return new WaitForSeconds(panel.delay);

            panel.panel.localScale = Vector3.zero;
            panel.panel.gameObject.SetActive(true);

            panel.panel.DOScale(Vector3.one, panel.duration)
                       .SetEase(panel.ease);
        }

        skipButton.gameObject.SetActive(true);
    }

    public void CloseAllMenus()
    {
        startMenuRoot.SetActive(false);
        comicRoot.SetActive(false);
    }
    IEnumerator BeginGameplay()
    {
        CloseAllMenus();

        Debug.Log("Begain");

        if (MinigameTransitionUI.Instance != null)
            yield return MinigameTransitionUI.Instance.PlayReadyIntro("COLLECTS ALL THE COINS!");

        Debug.Log("End Transition");

        MinigameManager.Instance?.BeginGameLoop();
    }
}
