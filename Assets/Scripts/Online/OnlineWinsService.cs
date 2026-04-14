using UnityEngine;
//Тут мы считаем только победы в «онлайне» для лидерборда.
public static class OnlineWinsService
{
    private const string KeyWins = "ONLINE_WINS"; // Ключ для PlayerPrefs [cite: 1235]

    // Получаем текущее кол-во побед
    public static int GetWins() => PlayerPrefs.GetInt(KeyWins, 0);

    // Метод добавления победы
    public static void AddWin()
    {
        int wins = GetWins() + 1;           // Увеличиваем на 1
        PlayerPrefs.SetInt(KeyWins, wins);  // Сохраняем [cite: 1238]
        PlayerPrefs.Save();                 // Записываем [cite: 1240]

        // Отправляем данные в твой LeaderboardService, чтобы они ушли в Яндекс 
        LeaderboardService.Instance.SubmitOnlineWins(wins);
    }
}