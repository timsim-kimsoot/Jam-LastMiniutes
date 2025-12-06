using UnityEngine;
using DG.Tweening;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Sources")]
    [SerializeField] private AudioSource sourceA;
    [SerializeField] private AudioSource sourceB;

    [Header("General Settings")]
    [SerializeField] private float musicVolume = 0.25f;
    [SerializeField] private float fadeDuration = 1.8f;

    [Header("UI/Non-Gameplay Music")]
    [SerializeField] private AudioClip titleMusicClip;
    [SerializeField] private AudioClip comicMusicClip;

    [Header("Minigame Music")]
    [SerializeField] private AudioClip phase1Clip;
    [SerializeField] private AudioClip phase2Clip;
    [SerializeField] private AudioClip phase3Clip;

    private bool usingA = true;
    private int currentPhase = -1;
    private enum MusicMode { None, Title, Comic, Minigame }
    private MusicMode currentMode = MusicMode.None;


    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayTitleMusic()
    {
        PlayMode(MusicMode.Title, titleMusicClip);
    }

    public void PlayComicMusic()
    {
        PlayMode(MusicMode.Comic, comicMusicClip);
    }

    public void UpdateMinigameMusic(float difficulty)
    {
        int phase = GetPhase(difficulty);
        if (phase == currentPhase && currentMode == MusicMode.Minigame) return;

        currentPhase = phase;
        currentMode = MusicMode.Minigame;

        PlayClip(GetClipForPhase(phase));
    }

    private void PlayMode(MusicMode mode, AudioClip clip)
    {
        currentMode = mode;
        currentPhase = -1;
        PlayClip(clip);
    }

    private int GetPhase(float diff)
    {
        if (diff < 2f) return 0;
        if (diff < 5d) return 1;
        return 2;
    }

    private AudioClip GetClipForPhase(int phase)
    {
        switch (phase)
        {
            case 0: return phase1Clip;
            case 1: return phase2Clip;
            default: return phase3Clip;
        }
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip == null) return;

        AudioSource current = usingA ? sourceA : sourceB;
        AudioSource next = usingA ? sourceB : sourceA;

        usingA = !usingA;

        next.clip = clip;
        next.volume = 0f;
        next.Play();

        DOTween.Kill(current);
        DOTween.Kill(next);

        current.DOFade(0f, fadeDuration).SetEase(Ease.InOutSine);
        next.DOFade(musicVolume, fadeDuration).SetEase(Ease.InOutSine);
    }
}
