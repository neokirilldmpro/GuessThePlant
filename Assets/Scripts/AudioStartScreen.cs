using UnityEngine;

public class AudioStartScreen : MonoBehaviour
{
    // Статическая переменная живет всё время работы игры, не сбрасываясь при смене сцен
    private static bool _alreadyStarted = false;

    private void Start()
    {
        // Если мы уже стартовали игру и просто вернулись в меню, сразу прячем панель
        if (_alreadyStarted)
        {
            gameObject.SetActive(false);
        }
    }

    // Этот метод мы привяжем к клику по панели
    public void OnScreenClicked()
    {
        _alreadyStarted = true;
        gameObject.SetActive(false);
    }
}