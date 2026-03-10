using UnityEngine;

// Один раз при старте меню регистрируем ссылки на пресеты в GameSessionSettings
public class PresetRegistry : MonoBehaviour
{
    [SerializeField] private QuizDifficultyPreset easy;
    [SerializeField] private QuizDifficultyPreset medium;
    [SerializeField] private QuizDifficultyPreset hard;
    [SerializeField] private QuizDifficultyPreset maxHard;

    private void Awake()
    {
        GameSessionSettings.EasyPreset = easy;
        GameSessionSettings.MediumPreset = medium;
        GameSessionSettings.HardPreset = hard;
        GameSessionSettings.MaxHardPreset = maxHard;
    }
}