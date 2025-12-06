using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StartMenuUI : MonoBehaviour
{
    public static StartMenuUI Instance { get; private set; }

    [Header("Roots")]
    [SerializeField] GameObject startMenuRoot;
    [SerializeField] GameObject comicRoot;

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

        OpenComic();
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
            yield return MinigameTransitionUI.Instance.PlayReadyIntro();

        Debug.Log("End Transition");

        MinigameManager.Instance?.BeginGameLoop();
    }
}
