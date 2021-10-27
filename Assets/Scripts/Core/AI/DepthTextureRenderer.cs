using UnityEngine;
using System.Collections;
using System;

public class DepthTextureRenderer : MonoBehaviour
{

    [SerializeField]
    private RenderTexture inputTexture;

    [SerializeField]
    private RenderTexture outputTexture;

    private void Start()
    {
        StartCoroutine(CopyTexture());
    }

    private IEnumerator CopyTexture()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            Graphics.Blit(inputTexture, outputTexture);
        }
    }
}
