using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Управляет UI слайдером громкости музыки в SettingsPanel
public class SettingsMusicVolumeUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider volumeSlider;      // Слайдер громкости (0..1)
    [SerializeField] private TMP_Text valueText;       // Текст рядом со слайдером (например "65%")

    private bool _initialized; // Чтобы не ловить лишние вызовы при установке значения

    private void OnEnable()
    {
        InitFromMusicManager();
    }

    private void Start()
    {
        InitFromMusicManager();
    }

    // Инициализация слайдера из текущей громкости MusicManager
    private void InitFromMusicManager()
    {
        if (_initialized) return;

        if (volumeSlider == null)
        {
            Debug.LogError("[SettingsMusicVolumeUI] volumeSlider is not assigned!");
            return;
        }

        if (MusicManager.Instance == null)
        {
            Debug.LogWarning("[SettingsMusicVolumeUI] MusicManager.Instance is null (maybe not in scene yet).");
            return;
        }

        _initialized = true;

        // Настраиваем диапазон (если вдруг не выставлен)
        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;

        // Ставим текущее значение громкости
        float v = MusicManager.Instance.GetMasterVolume();
        volumeSlider.SetValueWithoutNotify(v);
        UpdateValueText(v);

        // Подписываемся на изменение слайдера
        volumeSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnDisable()
    {
        // Отписка — хорошая практика
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnSliderChanged);

        _initialized = false;
    }

    // Вызывается каждый раз, когда игрок двигает слайдер
    private void OnSliderChanged(float v)
    {
        // Если менеджера нет — ничего не делаем
        if (MusicManager.Instance == null) return;

        // Применяем громкость и сохраняем (MusicManager это сделает)
        MusicManager.Instance.SetMasterVolume(v);

        // Обновляем текст процента
        UpdateValueText(v);
    }

    private void UpdateValueText(float v)
    {
        if (valueText == null) return;

        int percent = Mathf.RoundToInt(v * 100f);
        valueText.text = percent + "%";
    }
}