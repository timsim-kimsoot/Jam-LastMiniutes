using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance { get; private set; }

    [Header("Refs")]
    [SerializeField] MinigameBase[] minigames;
    [SerializeField] Transform miniGameContainer;

    [Header("Difficulty")]
    [SerializeField] float difficulty = 0f;
    [SerializeField] float difficultyIncrease = 0.25f;

    [Header("Phase Order")]
    [SerializeField]
    MinigameBase.DayPhase[] phaseOrder =
    {
        MinigameBase.DayPhase.Morning,
        MinigameBase.DayPhase.Commute,
        MinigameBase.DayPhase.Work,
        MinigameBase.DayPhase.Evening
    };

    MinigameBase activeGame;
    int phaseIndex;
    bool lastResultWasWin;

    public MinigameBase ActiveGame => activeGame;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        if (minigames == null || minigames.Length == 0)
            yield break;

        phaseIndex = 0;

        while (true)
        {
            var phase = phaseOrder[phaseIndex];
            MinigameBase root = PickRandomRootForPhase(phase);

            if (root == null)
            {
                yield return null;
                continue;
            }

            yield return PlayChain(root);

            if (root.CountsForDayCycle)
            {
                if (lastResultWasWin)
                    difficulty += difficultyIncrease;

                phaseIndex++;
                if (phaseIndex >= phaseOrder.Length)
                    phaseIndex = 0;
            }
        }
    }

    MinigameBase PickRandomRootForPhase(MinigameBase.DayPhase phase)
    {
        List<MinigameBase> list = new List<MinigameBase>();

        for (int i = 0; i < minigames.Length; i++)
        {
            var m = minigames[i];
            if (m == null) continue;
            if (m.Phase != phase) continue;
            list.Add(m);
        }

        if (list.Count == 0)
            return null;

        int index = Random.Range(0, list.Count);
        return list[index];
    }

    IEnumerator PlayChain(MinigameBase rootPrefab)
    {
        MinigameBase nextPrefab = rootPrefab;

        while (nextPrefab != null)
        {
            yield return PlaySingleGame(nextPrefab);

            if (!lastResultWasWin)
                break;

            nextPrefab = nextPrefab.NextOnWin;
        }
    }

    IEnumerator PlaySingleGame(MinigameBase prefab)
    {
        bool finished = false;
        lastResultWasWin = false;

        activeGame = Instantiate(prefab, miniGameContainer);
        activeGame.enabled = false;

        activeGame.OnWin += () =>
        {
            finished = true;
            lastResultWasWin = true;
        };

        activeGame.OnFail += () =>
        {
            finished = true;
            lastResultWasWin = false;
        };

        activeGame.Init(difficulty);

        if (MinigameTransitionUI.Instance != null)
            yield return MinigameTransitionUI.Instance.PlayReadyIntro();

        activeGame.enabled = true;

        while (!finished)
            yield return null;

        MinigameTransitionUI.Instance.ZoomOutToBlack();

        if (activeGame != null)
        {
            Destroy(activeGame.gameObject);
            activeGame = null;
        }
    }
}
