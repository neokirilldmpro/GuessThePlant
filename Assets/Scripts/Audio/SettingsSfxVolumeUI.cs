using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Управляет UI слайдером громкости SFX в SettingsPanel
public class SettingsSfxVolumeUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider sfxSlider;    // Slider (0..1)
    [SerializeField] private TMP_Text valueText;  // Проценты "80%"

    private bool _initialized;

    private void OnEnable()
    {
        Init();
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        if (_initialized) return;

        if (sfxSlider == null)
        {
            Debug.LogError("[SettingsSfxVolumeUI] sfxSlider is not assigned!");
            return;
        }

        if (SfxManager.Instance == null)
        {
            Debug.LogWarning("[SettingsSfxVolumeUI] SfxManager.Instance is null. Ensure SfxManager exists in MenuScene.");
            return;
        }

        _initialized = true;

        // Гарантируем диапазон
        sfxSlider.minValue = 0f;
        sfxSlider.maxValue = 1f;

        // Ставим текущее значение
        float v = SfxManager.Instance.GetSfxVolume();
        sfxSlider.SetValueWithoutNotify(v);
        UpdateValueText(v);

        // Подписка на изменение
        sfxSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnDisable()
    {
        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnSliderChanged);

        _initialized = false;
    }

    private void OnSliderChanged(float v)
    {
        if (SfxManager.Instance == null) return;

        // Применяем и сохраняем
        SfxManager.Instance.SetSfxVolume(v);

        // Обновляем проценты
        UpdateValueText(v);
    }

    private void UpdateValueText(float v)
    {
        if (valueText == null) return;

        int percent = Mathf.RoundToInt(v * 100f);
        valueText.text = percent + "%";
    }
}