using UnityEngine;
using System.Collections;
using UnityEngine.VFX;

public class AirRippleAnimator : MonoBehaviour
{
    [SerializeField]
    private Pool airRipplePool;

    private void Start()
    {
        StartCoroutine(Test());
    }

    private IEnumerator Test()
    {
        while (true)
        {
            var effect = SpawnEffect(Quaternion.Euler(0f, 0f, 90f));
            yield return new WaitUntil(() => effect.aliveParticleCount == 0);
            airRipplePool.ReturnToPool(effect);

            yield return new WaitForSeconds(5f);
        }
    }

    private VisualEffect SpawnEffect(Quaternion rotation)
    {
        var effect = airRipplePool.GetFromPool<VisualEffect>();
        effect.transform.rotation = rotation;
        effect.transform.position = transform.position;

        effect.Reinit();
        effect.Play();

        return effect;
    }
}
