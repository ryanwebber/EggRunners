using UnityEngine;
using System.Collections;

public class TextReceiver : MonoBehaviour
{
    public Event<string, bool> OnTextUpdate;

    public void UpdateText(string text, bool animated = true)
    {
        OnTextUpdate?.Invoke(text, animated);
    }
}
