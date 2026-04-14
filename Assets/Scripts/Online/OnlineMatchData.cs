using UnityEngine;

// Перечисление для статуса матча (Победа, Поражение, Ничья)
[System.Serializable]
public enum MatchResult { None, Win, Lose, Draw }

[System.Serializable]
public class OnlineMatchData
{
    // Флаг: активен ли сейчас онлайн-режим (чтобы QuizGameController знал, как себя вести)
    public bool IsOnlineMatch = false;

    // Имя противника (берется у бота или "Призрака")
    public string OpponentName = "";

    // Сколько очков набрал соперник (генерируется заранее)
    public int OpponentScore = 0;

    // За какое время соперник прошел тест (в секундах)
    public float OpponentTime = 0f;

    // Результаты игрока (заполняются в конце игры)
    public int PlayerScore = 0;
    public float PlayerTime = 0f;

    // Итоговый результат (кто победил)
    public MatchResult Result = MatchResult.None;

    // Был ли соперником "Призрак" (твой прошлый лучший результат)
    public bool IsGhost = false;
}