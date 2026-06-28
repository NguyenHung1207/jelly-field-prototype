using System.Collections;
using UnityEngine;

public class JellyAnimator : MonoBehaviour
{
    private Coroutine currentScaleRoutine;
    private Coroutine currentMoveRoutine;

    private Vector3 baseScale = Vector3.one;

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    public void PlaySpawnAnimation()
    {
        StopScaleRoutine();

        transform.localScale = baseScale * 0.2f;

        currentScaleRoutine = StartCoroutine(ScaleSequence(new[]
        {
            new ScaleKey(baseScale * 1.18f, 0.12f),
            new ScaleKey(new Vector3(baseScale.x * 0.92f, baseScale.y * 1.08f, baseScale.z * 0.92f), 0.08f),
            new ScaleKey(baseScale, 0.10f)
        }));
    }

    public void SetDraggingVisual(bool isDragging)
    {
        StopScaleRoutine();

        Vector3 targetScale = isDragging
            ? baseScale * 1.08f
            : baseScale;

        currentScaleRoutine = StartCoroutine(ScaleTo(targetScale, 0.08f));
    }

    public void PlayPlaceAnimation()
    {
        StopScaleRoutine();

        currentScaleRoutine = StartCoroutine(ScaleSequence(new[]
        {
            new ScaleKey(new Vector3(baseScale.x * 1.18f, baseScale.y * 0.72f, baseScale.z * 1.18f), 0.07f),
            new ScaleKey(new Vector3(baseScale.x * 0.88f, baseScale.y * 1.18f, baseScale.z * 0.88f), 0.09f),
            new ScaleKey(new Vector3(baseScale.x * 1.05f, baseScale.y * 0.95f, baseScale.z * 1.05f), 0.07f),
            new ScaleKey(baseScale, 0.08f)
        }));
    }

    public void MoveTo(Vector3 targetPosition, float duration)
    {
        StopMoveRoutine();
        currentMoveRoutine = StartCoroutine(MoveToRoutine(targetPosition, duration));
    }

    private IEnumerator MoveToRoutine(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = EaseOutBack(t);

            transform.position = Vector3.LerpUnclamped(startPosition, targetPosition, easedT);

            yield return null;
        }

        transform.position = targetPosition;
        currentMoveRoutine = null;
    }

    private IEnumerator ScaleTo(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = EaseOutCubic(t);

            transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, easedT);

            yield return null;
        }

        transform.localScale = targetScale;
        currentScaleRoutine = null;
    }

    private IEnumerator ScaleSequence(ScaleKey[] keys)
    {
        foreach (ScaleKey key in keys)
        {
            yield return ScaleTo(key.TargetScale, key.Duration);
        }

        currentScaleRoutine = null;
    }

    private void StopScaleRoutine()
    {
        if (currentScaleRoutine != null)
        {
            StopCoroutine(currentScaleRoutine);
            currentScaleRoutine = null;
        }
    }

    private void StopMoveRoutine()
    {
        if (currentMoveRoutine != null)
        {
            StopCoroutine(currentMoveRoutine);
            currentMoveRoutine = null;
        }
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;

        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private readonly struct ScaleKey
    {
        public readonly Vector3 TargetScale;
        public readonly float Duration;

        public ScaleKey(Vector3 targetScale, float duration)
        {
            TargetScale = targetScale;
            Duration = duration;
        }
    }
}