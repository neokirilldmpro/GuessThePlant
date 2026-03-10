using TMPro;
using UnityEngine;

public class TooltipUI : MonoBehaviour
{
    [SerializeField] private GameObject root;   // TooltipPanel
    [SerializeField] private TMP_Text text;     // TooltipText
    [SerializeField] private RectTransform rootRect;

    private void Awake()
    {
        Hide();
    }

    public void Show(string message, Vector2 screenPosition)
    {
        if (root != null) root.SetActive(true);

        if (text != null)
            text.text = message;

        // Позиционирование около курсора (в Screen Space Overlay это работает напрямую)
        if (rootRect != null)
            rootRect.position = screenPosition;
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
    }

    public void ShowNear(RectTransform anchor, string message, Vector2 offset)
    {
        if (root != null) root.SetActive(true);

        if (text != null)
            text.text = message;

        if (rootRect == null || anchor == null)
            return;

        // Берём мировую позицию якоря (слота) и добавляем смещение
        Vector3 worldPos = anchor.position;

        // Ставим tooltip рядом со слотом
        rootRect.position = worldPos + (Vector3)offset;
    }
}