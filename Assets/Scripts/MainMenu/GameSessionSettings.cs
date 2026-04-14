using System.Collections.Generic;
using UnityEngine;

// Этот статический класс хранит данные текущей игровой сессии между сценами.
//
// Что именно он хранит:
// 1) какой этап сейчас выбран;
// 2) список всех этапов кампании;
// 3) имя игровых сцен.
//
// Раньше тут была логика только под 4 уровня:
// Easy / Medium / Hard / MaxHard.
//
// Теперь мы переводим проект на нормальную кампанию из 8 этапов.
// Поэтому вместо 4 отдельных ссылок храним МАССИВ всех этапов.
public static class GameSessionSettings
{
    // Выбранный этап, который должен запуститься в GameScene.
    // Этот preset выставляется в меню, а в игровой сцене читается.
    public static QuizDifficultyPreset SelectedPreset;

    // Имя сцены игры.
    // Так лучше, чем писать "GameScene" строкой в разных местах.
    public const string GameSceneName = "GameScene";

    // Имя сцены меню.
    public const string MenuSceneName = "MenuScene";

    // Внутренний массив всех этапов кампании.
    // Делаем private, чтобы его нельзя было случайно испортить снаружи.
    private static QuizDifficultyPreset[] _campaignStages = new QuizDifficultyPreset[0];

    // Публичный метод регистрации этапов кампании.
    // Его будет вызывать PresetRegistry в меню.
    // Добавляем ссылку на пресет для онлайна
    public static QuizDifficultyPreset OnlinePreset;
    public static void RegisterCampaignPresets(QuizDifficultyPreset[] presets)
    {
        // Если в метод пришёл null — создаём пустой массив,
        // чтобы дальше не ловить NullReferenceException.
        if (presets == null)
        {
            _campaignStages = new QuizDifficultyPreset[0];
            SelectedPreset = null;

            Debug.LogWarning("[GameSessionSettings] RegisterCampaignPresets received null array.");
            return;
        }

        // Создаём временный список, чтобы:
        // 1) выкинуть null-элементы,
        // 2) не хранить мусор,
        // 3) потом отсортировать этапы по StageOrder.
        List<QuizDifficultyPreset> cleaned = new List<QuizDifficultyPreset>();

        for (int i = 0; i < presets.Length; i++)
        {
            // Берём очередной preset из входного массива
            QuizDifficultyPreset p = presets[i];

            // Если элемент пустой — пропускаем
            if (p == null)
                continue;

            // Добавляем в список
            cleaned.Add(p);
        }

        // Сортируем этапы по порядку кампании:
        // 1, 2, 3, ... 8
        cleaned.Sort((a, b) => a.StageOrder.CompareTo(b.StageOrder));

        // Сохраняем отсортированный массив
        _campaignStages = cleaned.ToArray();

        // Если после регистрации этапов выбранный preset пустой
        // или больше не входит в кампанию — выбираем стартовый.
        if (SelectedPreset == null || !ContainsPreset(SelectedPreset))
        {
            // Пытаемся выбрать первый открытый этап.
            // Если ничего не найдено — берём просто первый этап кампании.
            SelectedPreset = GetFirstUnlockedStage();

            if (SelectedPreset == null)
                SelectedPreset = GetFirstStage();
        }

        Debug.Log($"[GameSessionSettings] Registered campaign stages: {_campaignStages.Length}");
    }

    // Вернуть копию массива этапов кампании.
    //
    // Почему копию:
    // чтобы какой-нибудь внешний код случайно не переписал
    // внутренний массив _campaignStages.
    public static QuizDifficultyPreset[] GetAllCampaignStages()
    {
        QuizDifficultyPreset[] copy = new QuizDifficultyPreset[_campaignStages.Length];

        for (int i = 0; i < _campaignStages.Length; i++)
            copy[i] = _campaignStages[i];

        return copy;
    }

    // Вернуть первый этап кампании.
    public static QuizDifficultyPreset GetFirstStage()
    {
        if (_campaignStages == null || _campaignStages.Length == 0)
            return null;

        return _campaignStages[0];
    }

    // Вернуть первый ОТКРЫТЫЙ этап кампании.
    //
    // Обычно это либо stage 1,
    // либо первый ещё не пройденный, но уже открытый этап.
    public static QuizDifficultyPreset GetFirstUnlockedStage()
    {
        if (_campaignStages == null || _campaignStages.Length == 0)
            return null;

        for (int i = 0; i < _campaignStages.Length; i++)
        {
            QuizDifficultyPreset p = _campaignStages[i];

            if (p == null)
                continue;

            if (StageProgressService.IsUnlocked(p))
                return p;
        }

        return null;
    }

    // Найти этап по его порядковому номеру.
    // Например:
    // stageOrder = 1 -> Easy 1
    // stageOrder = 2 -> Easy 2
    public static QuizDifficultyPreset GetPresetByStageOrder(int stageOrder)
    {
        if (_campaignStages == null || _campaignStages.Length == 0)
            return null;

        for (int i = 0; i < _campaignStages.Length; i++)
        {
            QuizDifficultyPreset p = _campaignStages[i];

            if (p == null)
                continue;

            if (p.StageOrder == stageOrder)
                return p;
        }

        return null;
    }

    // Найти следующий этап после текущего.
    public static QuizDifficultyPreset GetNextStage(QuizDifficultyPreset current)
    {
        if (current == null)
            return null;

        return GetPresetByStageOrder(current.StageOrder + 1);
    }

    // Проверить, входит ли preset в зарегистрированную кампанию.
    public static bool ContainsPreset(QuizDifficultyPreset preset)
    {
        if (preset == null)
            return false;

        if (_campaignStages == null || _campaignStages.Length == 0)
            return false;

        for (int i = 0; i < _campaignStages.Length; i++)
        {
            if (_campaignStages[i] == preset)
                return true;
        }

        return false;
    }

    // Метод для активации онлайн-режима
 /*   public static void SetOnlineMode(int count)
    {
        // Если есть хоть один пресет в базе 
        if (_campaignStages != null && _campaignStages.Length > 0)
        {
            // Берем пресет "Medium" (индекс 2) как базу для баланса
            SelectedPreset = _campaignStages[Mathf.Min(2, _campaignStages.Length - 1)];
            // ВАЖНО: Твой QuizGameController сам возьмет 10 вопросов, 
            // так как мы проверим флаг онлайн-матча.
        }
    }*/
}