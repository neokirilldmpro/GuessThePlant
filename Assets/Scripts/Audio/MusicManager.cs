using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Менеджер музыки между сценами.
// Поддерживает menu music + 4 игровые группы музыки:
// easy / medium / hard / expert(maxHardClip).
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Clips")]
    [SerializeField] private AudioClip menuClip;
    [SerializeField] private AudioClip easyClip;
    [SerializeField] private AudioClip mediumClip;
    [SerializeField] private AudioClip hardClip;
    [SerializeField] private AudioClip maxHardClip;
    // maxHardClip используем как музыку для expert-этапов,
    // чтобы не ломать уже настроенный Inspector.

    [Header("Playback")]
    [SerializeField] private float masterVolume = 0.6f;
    private const string MusicVolumeKey = "FLOWER_MUSIC_VOLUME";

    [SerializeField] private float crossfadeSeconds = 1.0f;

    private AudioSource _a;
    private AudioSource _b;
    private AudioSource _activeSource;
    private Coroutine _fadeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _a = gameObject.AddComponent<AudioSource>();
        _b = gameObject.AddComponent<AudioSource>();

        SetupSource(_a);
        SetupSource(_b);

        _activeSource = _a;

        SceneManager.sceneLoaded += OnSceneLoaded;

        masterVolume = PlayerPrefs.GetFloat(MusicVolumeKey, masterVolume);
        _activeSource.volume = masterVolume;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void SetupSource(AudioSource s)
    {
        s.playOnAwake = false;
        s.loop = true;
        s.volume = 0f;
        s.spatialBlend = 0f;
    }

    private void Start()
    {
        ApplyMusicForCurrentContext();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyMusicForCurrentContext();
    }

    private void ApplyMusicForCurrentContext()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        // В меню всегда играет menuClip.
        if (sceneName == GameSessionSettings.MenuSceneName)
        {
            PlayWithCrossfade(menuClip);
            return;
        }

        // В игровой сцене выбираем клип по текущему этапу.
        if (sceneName == GameSessionSettings.GameSceneName)
        {
            AudioClip clip = PickGameClip();
            PlayWithCrossfade(clip);
            return;
        }
    }

    private AudioClip PickGameClip()
    {
        QuizDifficultyPreset preset = GameSessionSettings.SelectedPreset;

        // Если preset почему-то не выбран —
        // лучше играть easyClip, а не menuClip,
        // чтобы не было ощущения, что игра "зависла на меню-музыке".
        if (preset == null)
        {
            Debug.LogWarning("[MusicManager] SelectedPreset is null. Fallback to easyClip.");
            return easyClip != null ? easyClip : menuClip;
        }

        // Главный новый ориентир — stageKey.
        string stageKey = preset.StageKey != null
            ? preset.StageKey.Trim().ToLowerInvariant()
            : string.Empty;

        // ---------- Группы по stageKey ----------
        if (stageKey.StartsWith("easy"))
            return easyClip != null ? easyClip : menuClip;

        if (stageKey.StartsWith("medium"))
            return mediumClip != null ? mediumClip : menuClip;

        if (stageKey.StartsWith("hard"))
            return hardClip != null ? hardClip : menuClip;

        if (stageKey.StartsWith("expert"))
            return maxHardClip != null ? maxHardClip : menuClip;

        // ---------- Запасной вариант по StageOrder ----------
        // Если stageKey по какой-то причине не заполнен,
        // пытаемся определить группу по номеру этапа.
        int order = preset.StageOrder;

        if (order == 1 || order == 2)
            return easyClip != null ? easyClip : menuClip;

        if (order == 3 || order == 4)
            return mediumClip != null ? mediumClip : menuClip;

        if (order == 5 || order == 6)
            return hardClip != null ? hardClip : menuClip;

        if (order == 7 || order == 8)
            return maxHardClip != null ? maxHardClip : menuClip;

        // Самый последний fallback.
        return menuClip;
    }

    public void PlayWithCrossfade(AudioClip newClip)
    {
        if (newClip == null)
            return;

        if (_activeSource.clip == newClip && _activeSource.isPlaying)
            return;

        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        _fadeRoutine = StartCoroutine(CrossfadeTo(newClip));
    }

    private IEnumerator CrossfadeTo(AudioClip newClip)
    {
        AudioSource from = _activeSource;
        AudioSource to = (from == _a) ? _b : _a;

        to.clip = newClip;
        to.volume = 0f;
        to.Play();

        float t = 0f;
        float duration = Mathf.Max(0.01f, crossfadeSeconds);

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);

            to.volume = masterVolume * k;
            from.volume = masterVolume * (1f - k);

            yield return null;
        }

        to.volume = masterVolume;
        from.volume = 0f;
        from.Stop();

        _activeSource = to;
        _fadeRoutine = null;
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MusicVolumeKey, masterVolume);
        PlayerPrefs.Save();

        if (_a != null && _a.isPlaying && _a != _activeSource)
            _a.volume = 0f;

        if (_b != null && _b.isPlaying && _b != _activeSource)
            _b.volume = 0f;

        if (_activeSource != null)
            _activeSource.volume = masterVolume;
    }

    public float GetMasterVolume()
    {
        return masterVolume;
    }
}