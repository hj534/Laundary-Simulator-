using TMPro;
using UnityEngine;
using DG.Tweening;

public class CustomerPopupUI : MonoBehaviour
{
    public CanvasGroup canvasGroup; // Add CanvasGroup on root for fading
    public TextMeshProUGUI popupText;

    private Sequence popupSequence;

    void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        transform.localScale = Vector3.zero;
    }

    public void ShowPopup(string message, float duration = 2f)
    {
        if (popupSequence != null) popupSequence.Kill();

        popupText.text = message;
        canvasGroup.alpha = 0f;
        transform.localScale = Vector3.zero;

        popupSequence = DOTween.Sequence();
        popupSequence.Append(transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack)); // scale pop
        popupSequence.Join(canvasGroup.DOFade(1f, 0.25f)); // fade in
        popupSequence.AppendInterval(duration);
        popupSequence.Append(canvasGroup.DOFade(0f, 0.2f)); // fade out
        popupSequence.Join(transform.DOScale(0f, 0.2f).SetEase(Ease.InBack)); // scale down
    }
}
