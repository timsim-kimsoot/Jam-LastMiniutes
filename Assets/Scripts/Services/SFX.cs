using UnityEngine;
using System.Collections.Generic;

public class SFXService : MonoBehaviour
{
    public static SFXService Instance;

    [Header("Default Parent (Optional)")]
    [SerializeField] private Transform defaultParent;

    [Header("Pitch Randomization")]
    [SerializeField] private bool enablePitchRandomization = false;
    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 1.05f;

    [Header("SFX Library")]
    [SerializeField] private List<SFXEntry> sounds = new List<SFXEntry>();

    private Dictionary<string, AudioClip> soundLookup;

    [System.Serializable]
    public class SFXEntry
    {
        public string name;
        public AudioClip clip;
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        soundLookup = new Dictionary<string, AudioClip>();

        foreach (var s in sounds)
        {
            if (!soundLookup.ContainsKey(s.name) && s.clip != null)
                soundLookup.Add(s.name, s.clip);
        }
    }

    public void Play(string name, Transform parent = null, float volume = 1f, float pitch = 1f)
    {
        if (!soundLookup.TryGetValue(name, out AudioClip clip)) return;

        Transform p = parent != null ? parent : defaultParent;

        GameObject go = new GameObject("SFX_" + name);
        if (p != null) go.transform.SetParent(p, false);

        AudioSource src = go.AddComponent<AudioSource>();
        ConfigureSource(src, clip, volume, pitch);
        src.Play();

        Destroy(go, clip.length / Mathf.Max(0.1f, src.pitch));
    }

    public void PlayAt(string name, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (!soundLookup.TryGetValue(name, out AudioClip clip)) return;

        GameObject go = new GameObject("SFX_" + name);
        go.transform.position = position;

        AudioSource src = go.AddComponent<AudioSource>();
        ConfigureSource(src, clip, volume, pitch);
        src.Play();

        Destroy(go, clip.length / Mathf.Max(0.1f, src.pitch));
    }

    private void ConfigureSource(AudioSource src, AudioClip clip, float volume, float pitch)
    {
        src.clip = clip;
        src.playOnAwake = false;
        src.loop = false;
        src.volume = volume;

        float finalPitch = pitch;

        if (enablePitchRandomization)
            finalPitch *= Random.Range(minPitch, maxPitch);

        src.pitch = finalPitch;

        src.spatialBlend = 0f;
    }
}
