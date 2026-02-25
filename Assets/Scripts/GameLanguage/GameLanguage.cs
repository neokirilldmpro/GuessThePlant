/*Кнопка/переключатель языка в SettingsPanel
Глобальное хранение языка (GameLanguage)
Сохранение в PlayerPrefs
Обновление UI текста в меню
QuizGameController будет использовать выбранный язык, а не только preset.UseEnglish*/
// Пространство имён Unity не обязательно, но оставим для единообразия
using UnityEngine;

// Перечисление доступных языков в игре
public enum GameLanguage
{
    Russian = 0, // Русский
    English = 1  // Английский
}