using System;
using UnityEngine;

// Этот сервис хранит состояние достижений в PlayerPrefs.
//
// Что мы здесь улучшаем:
// 1) Unlock теперь возвращает bool:
//    true  -> достижение только что открылось
//    false -> достижение уже было открыто раньше
//
// 2) Добавляем событие AchievementUnlocked,
//    чтобы UI (портфель, мини-превью и т.д.) мог сразу обновляться,
//    как только нашивка реально открылась.
public static class AchievementService
{
    private const string Prefix = "ACH_";

    // Событие вызывается в момент, когда достижение
    // реально открылось впервые.
    public static event Action<AchievementId> AchievementUnlocked;

    // Проверить, получено ли достижение.
    public static bool IsUnlocked(AchievementId id)
    {
        string key = Prefix + id;
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    // Выдать достижение.
    //
    // Возвращает:
    // true  -> если достижение открылось ПРЯМО СЕЙЧАС
    // false -> если оно уже было открыто раньше
    public static bool Unlock(AchievementId id)
    {
        string key = Prefix + id;

        // Если уже было открыто — ничего не делаем.
        if (PlayerPrefs.GetInt(key, 0) == 1)
            return false;

        // Сохраняем флаг открытия.
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();

        // Сообщаем всем подписчикам, что новая нашивка открыта.
        AchievementUnlocked?.Invoke(id);

        return true;
    }

    // Сбросить все достижения.
    // Полезно для тестов.
    public static void ResetAll()
    {
        foreach (AchievementId id in Enum.GetValues(typeof(AchievementId)))
        {
            PlayerPrefs.DeleteKey(Prefix + id);
        }

        PlayerPrefs.Save();
    }
}