using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Clips")]
    [SerializeField] private AudioClip menuClip;
    [SerializeField] private AudioClip easyClip;
    [SerializeField] private AudioClip mediumClip;
    [SerializeField] private AudioClip hardClip;
    [SerializeField] private AudioClip maxHardClip;

    [Header("Playback")]
    [SerializeField] private float masterVolume = 0.6f;
    private const string MusicVolumeKey = "FLOWER_MUSIC_VOLUME";
    [SerializeField] private float crossfadeSeconds = 1.0f;

    private AudioSource _a;               // активный
    private AudioSource _b;               // резервный для кроссфейда
    private AudioSource _activeSource;    // какая сейчас играет
    private Coroutine _fadeRoutine;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Делаем 2 AudioSource для кроссфейда
        _a = gameObject.AddComponent<AudioSource>();
        _b = gameObject.AddComponent<AudioSource>();

        SetupSource(_a);
        SetupSource(_b);

        _activeSource = _a;

        // Подписываемся на смену сцен
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Загружаем сохранённую громкость (если нет — используем masterVolume из инспектора)
        masterVolume = PlayerPrefs.GetFloat(MusicVolumeKey, masterVolume);

        // Применяем громкость к текущему активному источнику (на случай если он уже играет)
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
        s.spatialBlend = 0f; // 2D
    }

    private void Start()
    {
        // На случай если MusicManager стоит сразу в первой сцене
        ApplyMusicForCurrentContext();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyMusicForCurrentContext();
    }

    private void ApplyMusicForCurrentContext()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        // 1) Меню
        if (sceneName == GameSessionSettings.MenuSceneName)
        {
            PlayWithCrossfade(menuClip);
            return;
        }

        // 2) Игра
        if (sceneName == GameSessionSettings.GameSceneName)
        {
            AudioClip clip = PickGameClip();
            PlayWithCrossfade(clip);
            return;
        }

        // 3) Прочие сцены — можно оставить как есть (ничего не менять)
    }

    private AudioClip PickGameClip()
    {
        // Берём выбранный preset из меню
        QuizDifficultyPreset preset = GameSessionSettings.SelectedPreset;

        // Если preset не выбран — считаем, что Easy
        string presetName = preset != null ? preset.PresetName : "Easy";
        string key = presetName.Trim().ToLowerInvariant();

        if (key == "easy") return easyClip != null ? easyClip : menuClip;
        if (key == "medium") return mediumClip != null ? mediumClip : menuClip;
        if (key == "hard") return hardClip != null ? hardClip : menuClip;
        if (key == "maxhard" || key == "max hard" || key == "max_hard") return maxHardClip != null ? maxHardClip : menuClip;

        // Если пресет назван нестандартно — fallback
        return menuClip;
    }

    public void PlayWithCrossfade(AudioClip newClip)
    {
        if (newClip == null) return;

        // Если уже играет этот клип — ничего не делаем
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

    // Опционально: чтобы потом сделать слайдер громкости
    // Установить громкость музыки (0..1) и сохранить в PlayerPrefs
    public void SetMasterVolume(float v)
    {
        masterVolume = Mathf.Clamp01(v); // Ограничиваем диапазон

        // Применяем к текущему активному источнику
        if (_activeSource != null)
            _activeSource.volume = masterVolume;

        // Сохраняем
        PlayerPrefs.SetFloat(MusicVolumeKey, masterVolume);
        PlayerPrefs.Save();
    }

    // Получить текущую громкость (удобно для выставления слайдера при открытии настроек)
    public float GetMasterVolume()
    {
        return masterVolume;
    }
}