using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(TMP_Text))]
[RequireComponent(typeof(TextReceiver))]
public class TextAnimator : MonoBehaviour
{
    [SerializeField]
    private float duration;

    private void Awake()
    {
        var container = GetComponent<TMP_Text>();
        GetComponent<TextReceiver>().OnTextUpdate += (newText, animated) => UpdateTextAnimated(container, newText, animated);
    }

    private void UpdateTextAnimated(TMP_Text container, string newText, bool animated)
    {
        if (!animated)
        {
            container.text = newText;
            return;
        }

        var oldScale = container.rectTransform.localScale;
        DOTween.Sequence()
            .Append(container.rectTransform.DOScale(Vector3.zero, duration)
                .SetEase(Ease.InSine))
            .AppendCallback(() => { container.text = newText; })
            .Append(container.rectTransform.DOScale(oldScale, duration)
                .SetEase(Ease.InSine))
            .Play();
    }
}
