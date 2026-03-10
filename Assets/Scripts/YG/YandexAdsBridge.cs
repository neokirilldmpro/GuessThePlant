using System;          // Нужен для Action (это "хранимая функция / коллбэк")
using UnityEngine;     // Unity API
using YG;              // Пространство имён PluginYG2

// Этот класс - центральный мост между твоей игрой и рекламой Yandex / PluginYG2.
// Зачем он нужен:
// 1) чтобы не писать рекламную логику в 10 разных местах;
// 2) чтобы можно было сказать: "покажи interstitial, а ПОСЛЕ него выполни действие";
// 3) чтобы rewarded реклама выдавала награду в одном месте.
public class YandexAdsBridge : MonoBehaviour
{
    // Singleton-ссылка, чтобы из любого другого скрипта можно было обратиться:
    // YandexAdsBridge.Instance.ShowInterstitialThen(...)
    public static YandexAdsBridge Instance { get; private set; }

    [Header("Reward IDs")]
    [SerializeField] private string correctAnswerRewardId = "correct_answer";
    // Это строковый ID награды для rewarded-рекламы.
    // Плагину нужен id, чтобы он понимал, какую награду мы запросили.
    // В нашем случае это "правильный ответ".

    [Header("Editor / No SDK fallback")]
    [SerializeField] private bool executeCallbacksWithoutSdk = true;
    // Если true:
    // когда SDK недоступен (например, в редакторе или локальном тесте),
    // коллбэки всё равно будут выполняться.
    // Это удобно, чтобы ты мог тестировать игровую логику без реальной рекламы.

    // Здесь временно храним действие, которое надо выполнить ПОСЛЕ interstitial.
    private Action _pendingInterstitialAction;

    // Флаг: ждём ли мы сейчас закрытия interstitial.
    private bool _waitingInterstitialClose;

    // Флаг: идёт ли сейчас rewarded реклама.
    // Нужен, чтобы игрок не спамил кнопку 10 раз подряд.
    private bool _rewardedInProgress;

    private void Awake()
    {
        // Обычный singleton-паттерн.
        // Если объект уже существует, второй экземпляр уничтожаем.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Делаем объект "сквозным" между сценами.
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        // Подписываемся на события interstitial рекламы.
        // onCloseInterAdvWasShow(bool) вызывается при закрытии interstitial
        // и сообщает, была ли реклама реально показана.
        YG2.onCloseInterAdvWasShow += OnInterstitialClosed;

        // Если interstitial не открылся с ошибкой,
        // мы всё равно должны продолжить действие,
        // чтобы кнопка не "ломала" переход.
        YG2.onErrorInterAdv += OnInterstitialError;

        // Для rewarded достаточно знать:
        // если реклама закрылась или произошла ошибка,
        // значит флаг _rewardedInProgress надо снять.
        YG2.onCloseRewardedAdv += OnRewardedClosed;
        YG2.onErrorRewardedAdv += OnRewardedError;
    }

    private void OnDisable()
    {
        // Всегда отписываемся от событий,
        // чтобы не было утечек подписок.
        YG2.onCloseInterAdvWasShow -= OnInterstitialClosed;
        YG2.onErrorInterAdv -= OnInterstitialError;
        YG2.onCloseRewardedAdv -= OnRewardedClosed;
        YG2.onErrorRewardedAdv -= OnRewardedError;
    }

    // Публичный getter, если где-то понадобится узнать ID награды.
    public string CorrectAnswerRewardId => correctAnswerRewardId;

    // ----------------------------------------------------
    // INTERSTITIAL
    // ----------------------------------------------------

    // Показать interstitial и ПОСЛЕ него выполнить afterAd.
    public void ShowInterstitialThen(Action afterAd)
    {
        // Если действия нет - нечего выполнять.
        if (afterAd == null)
            return;

        // Если SDK сейчас недоступен,
        // но мы разрешили fallback - просто выполняем действие без рекламы.
        if (executeCallbacksWithoutSdk && !YG2.isSDKEnabled)
        {
            afterAd.Invoke();
            return;
        }

        // Если уже открыта какая-то реклама - не ждём вторую,
        // просто выполняем действие, чтобы не ломать UX.
        if (YG2.nowAdsShow)
        {
            afterAd.Invoke();
            return;
        }

        // Если interstitial сейчас не готов по таймеру,
        // тоже не ждём, а просто продолжаем действие.
        // Это важно, потому что у плагина есть интервал между показами.
        if (!YG2.isTimerAdvCompleted)
        {
            afterAd.Invoke();
            return;
        }

        // Сохраняем отложенное действие.
        _pendingInterstitialAction = afterAd;

        // Ставим флаг ожидания закрытия рекламы.
        _waitingInterstitialClose = true;

        // Вызываем сам interstitial.
        YG2.InterstitialAdvShow();
    }

    // Этот метод вызывается, когда interstitial закрыт.
    private void OnInterstitialClosed(bool wasShown)
    {
        // Если мы сейчас ничего не ждали - выходим.
        if (!_waitingInterstitialClose)
            return;

        // Снимаем флаг ожидания.
        _waitingInterstitialClose = false;

        // Выполняем действие после рекламы.
        ExecutePendingInterstitialAction();
    }

    // Этот метод вызывается, если interstitial дал ошибку.
    private void OnInterstitialError()
    {
        // Если мы сейчас ничего не ждали - выходим.
        if (!_waitingInterstitialClose)
            return;

        // Снимаем флаг ожидания.
        _waitingInterstitialClose = false;

        // Даже при ошибке не блокируем игрока.
        ExecutePendingInterstitialAction();
    }

    // Внутренний метод: безопасно выполнить сохранённое действие.
    private void ExecutePendingInterstitialAction()
    {
        // Забираем ссылку во временную переменную.
        Action action = _pendingInterstitialAction;

        // Сразу очищаем поле, чтобы оно не вызвалось повторно.
        _pendingInterstitialAction = null;

        // Если действие было - запускаем.
        action?.Invoke();
    }

    // ----------------------------------------------------
    // REWARDED
    // ----------------------------------------------------

    // Показать rewarded рекламу и, если награда реально выдана,
    // выполнить rewardAction.
    public void ShowRewardedForCorrectAnswer(Action rewardAction)
    {
        // Если награда не задана - смысла нет.
        if (rewardAction == null)
            return;

        // Если rewarded уже идёт - повторно не запускаем.
        if (_rewardedInProgress)
            return;

        // Если SDK недоступен и включён fallback -
        // сразу выдаём награду без реальной рекламы.
        if (executeCallbacksWithoutSdk && !YG2.isSDKEnabled)
        {
            rewardAction.Invoke();
            return;
        }

        // Если уже открыта другая реклама - не запускаем ещё одну.
        if (YG2.nowAdsShow)
            return;

        // Ставим флаг "rewarded в процессе".
        _rewardedInProgress = true;

        // Показываем rewarded.
        // ВАЖНО:
        // rewardAction вызовется только если платформа реально выдала награду.
        YG2.RewardedAdvShow(correctAnswerRewardId, () =>
        {
            // Награда получена -> снимаем флаг.
            _rewardedInProgress = false;

            // Выдаём игровое вознаграждение.
            rewardAction.Invoke();
        });
    }

    // Rewarded закрылась.
    private void OnRewardedClosed()
    {
        _rewardedInProgress = false;
    }

    // Rewarded дала ошибку.
    private void OnRewardedError()
    {
        _rewardedInProgress = false;
    }
}