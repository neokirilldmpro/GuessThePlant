// File: FlowerDifficulty.cs  // Имя файла в проекте
using System;                 // Подключаем базовые типы .NET (например, Enum) 

[Serializable]                // Разрешаем сериализацию (на будущее, если enum будет частью сериализуемых структур)
public enum FlowerDifficulty  // Объявляем перечисление сложности цветка
{                             // Начало enum
    Easy = 0,                 // Лёгкий
    Medium = 1,               // Средний
    Hard = 2,                 // Тяжёлый
    MaxHard = 3               // Максимально тяжёлый
}                             // Конец enum