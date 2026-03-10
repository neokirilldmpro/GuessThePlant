using UnityEngine;

// Менеджер звуковых эффектов (SFX): клики, правильно/неправильно и т.д.
public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance { get; private set; }

    [Header("Clips")]
    [SerializeField] private AudioClip correctClip; // звук правильного ответа
    [SerializeField] private AudioClip wrongClip;   // звук неправильного ответа
    [SerializeField] private AudioClip clickClip;   // звук клика по кнопке

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.9f; // громкость эффектов
    private const string SfxVolumeKey = "FLOWER_SFX_VOLUME";

    private AudioSource _source; // один 2D источник для всех SFX

    private void Awake()
    {
        // Singleton (чтобы не создавалось 2 менеджера)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Создаём AudioSource
        _source = gameObject.AddComponent<AudioSource>();

        // Настройки AudioSource для UI/SFX
        _source.playOnAwake = false;
        _source.loop = false;
        _source.spatialBlend = 0f; // 2D
        _source.volume = sfxVolume;
        // Загружаем сохранённую громкость SFX (если нет — берём значение из инспектора)
        sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, sfxVolume);

        // Применяем к AudioSource
        if (_source != null)
            _source.volume = sfxVolume;
    }
    public void PlayClick()
    {
        if (_source == null) return;
        if (clickClip == null) return;

        _source.PlayOneShot(clickClip, sfxVolume);
    }

    // Играть звук правильного ответа
    public void PlayCorrect()
    {
        if (_source == null) return;
        if (correctClip == null) return;

        _source.PlayOneShot(correctClip, sfxVolume);
    }

    // Играть звук неправильного ответа
    public void PlayWrong()
    {
        if (_source == null) return;
        if (wrongClip == null) return;

        _source.PlayOneShot(wrongClip, sfxVolume);
    }
    public void SetSfxVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);

        if (_source != null)
            _source.volume = sfxVolume;

        PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
        PlayerPrefs.Save();
    }

    public float GetSfxVolume()
    {
        return sfxVolume;
    }
}