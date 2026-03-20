// File: FlowerQuestionGenerator.cs                                                                 // Имя файла в проекте
using System.Collections.Generic;                                                                // List<T>
using UnityEngine;                                                                               // UnityEngine (Random, Debug)

public static class FlowerQuestionGenerator                                                       // Генератор вопросов (чистая логика без MonoBehaviour)
{                                                                                                 // Начало класса
    public static List<FlowerQuestion> Generate(                                                   // Главный метод генерации
        FlowerDatabase database,                                                                   // База цветков
        QuizDifficultyPreset preset,                                                               // Пресет сложности
        bool useEnglish,
        int optionsCount = 4                                                                       // Количество вариантов (по умолчанию 4)
    )                                                                                              // Конец сигнатуры
    {                                                                                              // Начало метода
        var questions = new List<FlowerQuestion>();                                                // Итоговый список вопросов

        if (database == null)                                                                      // Проверяем базу
        {                                                                                          // Начало if
            Debug.LogError("[Flowers] Database is null");                                          // Логируем ошибку
            return questions;                                                                      // Возвращаем пустой список
        }                                                                                          // Конец if

        if (preset == null)                                                                        // Проверяем пресет
        {                                                                                          // Начало if
            Debug.LogError("[Flowers] Preset is null");                                            // Логируем ошибку
            return questions;                                                                      // Возвращаем пустой список
        }                                                                                          // Конец if

        if (optionsCount < 2) optionsCount = 2;                                                    // Минимум 2 варианта (иначе викторина бессмысленна)

        List<FlowerData> pool;                                                                     // Переменная для пула доступных цветков

        if (preset.PoolMode == QuestionPoolMode.ByIdRange)                                                 // Если пул формируем по ID диапазону
        {                                                                                          // Начало ветки
            pool = database.GetPoolByIdRange(preset.MinId, preset.MaxId);                          // Берём пул по диапазону
        }                                                                                          // Конец ветки
        else                                                                                       // Иначе формируем по maxDifficulty
        {                                                                                          // Начало else
            pool = database.GetPoolByDifficultyMax(preset.MaxDifficulty);                          // Берём пул по сложности
        }                                                                                          // Конец else

        if (pool.Count < optionsCount)                                                             // Если в пуле меньше, чем нужно вариантов
        {                                                                                          // Начало if
            Debug.LogError($"[Flowers] Pool too small. Need >= {optionsCount}, have {pool.Count}");// Логируем
            return questions;                                                                      // Возвращаем пустой список
        }                                                                                          // Конец if

        var unusedCorrect = new List<FlowerData>(pool);                                            // Список ещё не использованных "правильных" (для уникальности)

        int totalQuestions = preset.QuestionsCount;                                                // Сколько вопросов нужно

        for (int q = 0; q < totalQuestions; q++)                                                   // Генерируем каждый вопрос
        {                                                                                          // Начало цикла
            FlowerData correct;                                                                    // Правильный цветок

            if (preset.UniqueCorrectAnswers && unusedCorrect.Count > 0)                            // Если хотим уникальные и ещё есть неиспользованные
            {                                                                                      // Начало if
                int idx = Random.Range(0, unusedCorrect.Count);                                    // Берём случайный индекс
                correct = unusedCorrect[idx];                                                      // Берём элемент
                unusedCorrect.RemoveAt(idx);                                                       // Удаляем, чтобы не повторялся
            }                                                                                      // Конец if
            else                                                                                   // Иначе допускаем повторы
            {                                                                                      // Начало else
                correct = pool[Random.Range(0, pool.Count)];                                       // Берём случайный из пула
            }                                                                                      // Конец else

            if (correct == null)                                                                   // Страховка от null
            {                                                                                      // Начало if
                q--;                                                                               // Повторяем итерацию (не считаем вопрос)
                continue;                                                                          // Переходим дальше
            }                                                                                      // Конец if

            var picked = new List<FlowerData>(optionsCount);                                       // Список выбранных вариантов (FlowerData)
            picked.Add(correct);                                                                   // Добавляем правильный

            int guard = 0;                                                                         // Защита от бесконечного цикла
            while (picked.Count < optionsCount)                                                    // Пока не набрали варианты
            {                                                                                      // Начало while
                guard++;                                                                           // Увеличиваем счётчик
                if (guard > 5000)                                                                  // Если что-то пошло не так (редкий кейс)
                {                                                                                  // Начало if
                    Debug.LogError("[Flowers] Guard break while picking options");                 // Логируем
                    break;                                                                         // Выходим
                }                                                                                  // Конец if

                var cand = pool[Random.Range(0, pool.Count)];                                      // Берём кандидата
                if (cand == null) continue;                                                        // Пропускаем null
                if (picked.Contains(cand)) continue;                                               // Пропускаем повторы
                picked.Add(cand);                                                                  // Добавляем вариант
            }                                                                                      // Конец while

            for (int i = picked.Count - 1; i > 0; i--)                                             // Fisher–Yates shuffle для перемешивания вариантов
            {                                                                                      // Начало for
                int j = Random.Range(0, i + 1);                                                    // Случайный индекс
                (picked[i], picked[j]) = (picked[j], picked[i]);                                   // Меняем местами
            }                                                                                      // Конец for

            int correctIndex = picked.IndexOf(correct);                                            // Находим индекс правильного после перемешивания

            var options = new string[optionsCount];                                                // Массив строк-ответов
            for (int i = 0; i < optionsCount; i++)                                                 // Заполняем варианты
            {                                                                                      // Начало for
                //options[i] = preset.UseEnglish ? picked[i].NameEn : picked[i].NameRu;              // Берём имя по языку
                options[i] = useEnglish ? picked[i].NameEn : picked[i].NameRu;
            }                                                                                      // Конец for

            questions.Add(new FlowerQuestion                                                       // Добавляем сформированный вопрос
            {                                                                                      // Начало инициализации
                flowerId = correct.Id,                                                             // Сохраняем ID правильного
                image = correct.Image,                                                             // Сохраняем картинку
                options = options,                                                                 // Сохраняем варианты
                correctIndex = correctIndex                                                        // Сохраняем индекс правильного
            });                                                                                    // Конец добавления
        }                                                                                          // Конец цикла

        return questions;                                                                          // Возвращаем вопросы
    }                                                                                              // Конец метода
}                                                                                                 // Конец класса