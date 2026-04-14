using UnityEngine;

public static class BotOpponent
{
    // 50 реалистичных русских никнеймов
    private static readonly string[] NamesRu = {
        "Alex_99", "Serega_Pro", "Masha_flower", "Vanya_Bro", "Kote_Kotyata",
        "Alisa_Fox", "MaxPower", "Zmey_Gorynych", "Romashka_88", "Oduvanchik",
        "Anna_M", "Kiber_Brat", "Dimon_777", "Pchelka", "Luntik_Zloy",
        "Elena_B", "Ivan_Uragan", "Svetlana1990", "Dasha_Sun", "Kolya_Top",
        "Vova_Tank", "Olya_Lya", "Dima_Pro", "Kisa_Murisa", "Vika_Victoria",
        "Kaktus", "Prizrak_Nochi", "Stalker_Zone", "Nata_Sha", "Anton_K",
        "Sasha_Bely", "Denis_X", "Rina_Rina", "Anya_Cute", "Gamer_Ru",
        "Pro_IgroK", "Biba_2000", "Boba_2001", "Pupsik_Keksik", "Tigr_Rrr",
        "Volk_Odinuchka", "Liska_Aliska", "Yana_Banan", "guest_8842", "Player_001",
        "Kivi_Frukt", "Vinograd", "I_love_cats", "MAMA_YA_V_YUTUBE", "Nagibator_228"
    };

    // 50 реалистичных английских никнеймов
    private static readonly string[] NamesEn = {
        "CoolDude99", "ShadowKnight", "Rose_Bud", "Lily_Flower", "Pro_Player_X",
        "Star_Boy", "Moon_Light", "xX_Sniper_Xx", "GamerGirl_007", "AlexTheGreat",
        "Daisy_Duke", "Dark_Lord", "Night_Wolf", "Fire_Fly", "Water_Lily",
        "Sun_Flower", "Star_Catcher", "Moon_Walker", "Sky_Diver", "Ocean_Breeze",
        "ForestRanger", "Desert_Fox", "Snow_Bird", "Ice_Queen", "Fire_King",
        "StormBringer", "ThunderStrike", "LightningBolt", "Wind_Rider", "CloudStrife",
        "Ghost_Rider", "MysticMage", "DragonSlayer", "KnightTemplar", "RogueAssassin",
        "guest_1029", "Player_847", "Kitty_Cat", "Puppy_Dog", "Happy_Face",
        "SpeedyG", "Slow_Poke", "Smart_Guy", "Big_Boss", "Little_Boss",
        "Mega_Man", "Super_Girl", "Toxic_Gamer", "Max_Pain", "Iron_Man"
    };

    // Добавим публичный метод, чтобы мы могли вытаскивать просто случайное имя
    // для маскировки "Призрака"
    public static string GetRandomName(bool en)
    {
        return en ? NamesEn[Random.Range(0, NamesEn.Length)] : NamesRu[Random.Range(0, NamesRu.Length)];
    }

    public static (int score, float time, string name) Generate(int wins, bool en)
    {
        // Логика сложности бота
        int score = (wins < 5) ? Random.Range(5, 8) : Random.Range(7, 11);
        float time = Random.Range(30f, 60f);

        // Берем случайное имя через наш новый метод
        string name = GetRandomName(en);

        return (score, time, name);
    }
}