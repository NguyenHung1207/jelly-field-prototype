using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Volume")]
    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float sfxVolume = 1f;
    [SerializeField] private float musicVolume = 0.25f;

    [Header("Optional Clips")]
    [SerializeField] private AudioClip buttonClip;
    [SerializeField] private AudioClip dragClip;
    [SerializeField] private AudioClip placeClip;
    [SerializeField] private AudioClip invalidClip;
    [SerializeField] private AudioClip matchClip;
    [SerializeField] private AudioClip fillClip;
    [SerializeField] private AudioClip collectClip;
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip loseClip;
    [SerializeField] private AudioClip backgroundMusicClip;

    private AudioSource sfxSource;
    private AudioSource musicSource;

    private AudioClip generatedButtonClip;
    private AudioClip generatedDragClip;
    private AudioClip generatedPlaceClip;
    private AudioClip generatedInvalidClip;
    private AudioClip generatedMatchClip;
    private AudioClip generatedFillClip;
    private AudioClip generatedCollectClip;
    private AudioClip generatedWinClip;
    private AudioClip generatedLoseClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;

        GenerateFallbackClips();
        PlayMusic();
    }

    private void GenerateFallbackClips()
    {
        generatedButtonClip = CreateTone("Button", 720f, 0.055f, 0.35f);
        generatedDragClip = CreateTone("Drag", 520f, 0.045f, 0.25f);
        generatedPlaceClip = CreateTone("Place", 260f, 0.10f, 0.45f);
        generatedInvalidClip = CreateTone("Invalid", 130f, 0.16f, 0.45f);
        generatedMatchClip = CreateTone("Match", 820f, 0.13f, 0.55f);
        generatedFillClip = CreateTone("Fill", 460f, 0.18f, 0.25f);
        generatedCollectClip = CreateTone("Collect", 1040f, 0.10f, 0.45f);
        generatedWinClip = CreateSuccessClip();
        generatedLoseClip = CreateLoseClip();
    }

    public void PlayButton()
    {
        PlaySfx(buttonClip != null ? buttonClip : generatedButtonClip, 0.75f);
    }

    public void PlayDrag()
    {
        PlaySfx(dragClip != null ? dragClip : generatedDragClip, 0.65f);
    }

    public void PlayPlace()
    {
        PlaySfx(placeClip != null ? placeClip : generatedPlaceClip, 0.85f);
    }

    public void PlayInvalid()
    {
        PlaySfx(invalidClip != null ? invalidClip : generatedInvalidClip, 0.85f);
    }

    public void PlayMatch()
    {
        PlaySfx(matchClip != null ? matchClip : generatedMatchClip, 0.9f);
    }

    public void PlayFill()
    {
        PlaySfx(fillClip != null ? fillClip : generatedFillClip, 0.6f);
    }

    public void PlayCollect()
    {
        PlaySfx(collectClip != null ? collectClip : generatedCollectClip, 0.8f);
    }

    public void PlayWin()
    {
        PlaySfx(winClip != null ? winClip : generatedWinClip, 0.95f);
    }

    public void PlayLose()
    {
        PlaySfx(loseClip != null ? loseClip : generatedLoseClip, 0.95f);
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        ApplyMusicVolume();
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        ApplyMusicVolume();
    }

    private void PlayMusic()
    {
        if (backgroundMusicClip == null)
            return;

        musicSource.clip = backgroundMusicClip;
        ApplyMusicVolume();
        musicSource.Play();
    }

    private void ApplyMusicVolume()
    {
        if (musicSource != null)
        {
            musicSource.volume = masterVolume * musicVolume;
        }
    }

    private void PlaySfx(AudioClip clip, float volume)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip, masterVolume * sfxVolume * volume);
    }

    private AudioClip CreateTone(string clipName, float frequency, float duration, float volume)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            float envelope = 1f - (i / (float)sampleCount);
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * volume;
        }

        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateSuccessClip()
    {
        int sampleRate = 44100;
        float duration = 0.42f;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            float frequency = t < 0.14f ? 620f : t < 0.28f ? 820f : 1040f;
            float envelope = 1f - (i / (float)sampleCount) * 0.55f;
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.45f;
        }

        AudioClip clip = AudioClip.Create("Win", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateLoseClip()
    {
        int sampleRate = 44100;
        float duration = 0.38f;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            float frequency = Mathf.Lerp(260f, 110f, t / duration);
            float envelope = 1f - (i / (float)sampleCount);
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.45f;
        }

        AudioClip clip = AudioClip.Create("Lose", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}