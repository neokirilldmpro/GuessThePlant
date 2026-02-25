// File: FlowerQuestion.cs                                                                  // Имя файла в проекте
using System;                                                                               // Подключаем System для Serializable
using UnityEngine;                                                                          // Подключаем Unity API

[Serializable]                                                                              // Делаем структуру сериализуемой (удобно для отладки)
public class FlowerQuestion                                                                 // Runtime-модель вопроса
{                                                                                           // Начало класса
    public int flowerId;                                                                    // ID правильного цветка (для статистики/логов)
    public Sprite image;                                                                    // Картинка вопроса (спрайт правильного цветка)
    public string[] options;                                                                // 4 варианта ответа (строки RU/EN)
    public int correctIndex;                                                                // Индекс правильного ответа (0..3)
}                                                                                           // Конец класса