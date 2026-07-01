using System.Collections;
using UnityEngine;

public class JellyAnimator : MonoBehaviour
{
    private Coroutine currentScaleRoutine;
    private Coroutine currentMoveRoutine;
    private Coroutine currentPlaceRoutine;

    private Vector3 baseScale = Vector3.one;
    private Quaternion baseRotation = Quaternion.identity;

    private void Awake()
    {
        baseScale = transform.localScale;
        baseRotation = transform.localRotation;
    }

    public void PlaySpawnAnimation()
    {
        StopPlaceRoutine();
        StopScaleRoutine();

        transform.localScale = baseScale * 0.2f;
        transform.localRotation = baseRotation;

        currentScaleRoutine = StartCoroutine(ScaleSequence(new[]
        {
            new ScaleKey(baseScale * 1.18f, 0.12f),
            new ScaleKey(new Vector3(baseScale.x * 0.92f, baseScale.y * 1.08f, baseScale.z * 0.92f), 0.08f),
            new ScaleKey(baseScale, 0.10f)
        }));
    }

    public void SetDraggingVisual(bool isDragging)
    {
        StopPlaceRoutine();
        StopScaleRoutine();

        Vector3 targetScale = isDragging
            ? baseScale * 1.08f
            : baseScale;

        currentScaleRoutine = StartCoroutine(ScaleTo(targetScale, 0.08f));
    }

    public void PlayPlaceAnimation()
    {
        StopScaleRoutine();
        StopMoveRoutine();
        StopPlaceRoutine();

        currentPlaceRoutine = StartCoroutine(PlaceJellyRoutine());
    }

    public void MoveTo(Vector3 targetPosition, float duration)
    {
        StopMoveRoutine();
        currentMoveRoutine = StartCoroutine(MoveToRoutine(targetPosition, duration));
    }

    private IEnumerator PlaceJellyRoutine()
    {
        Vector3 startLocalPosition = transform.localPosition;
        Quaternion startLocalRotation = transform.localRotation;

        float duration = 0.46f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float squash = Mathf.Exp(-t * 5.5f) * Mathf.Sin(t * Mathf.PI * 7.5f);
            float wobble = Mathf.Exp(-t * 6.2f) * Mathf.Sin(t * Mathf.PI * 9.0f);
            float bounce = Mathf.Exp(-t * 6.5f) * Mathf.Abs(Mathf.Sin(t * Mathf.PI * 4.0f));

            float scaleX = 1f + squash * 0.18f;
            float scaleY = 1f - squash * 0.22f;
            float scaleZ = 1f + squash * 0.18f;

            transform.localScale = new Vector3(
                baseScale.x * scaleX,
                baseScale.y * scaleY,
                baseScale.z * scaleZ
            );

            float rotationZ = wobble * 5.5f;
            float rotationX = -wobble * 3.5f;

            transform.localRotation = startLocalRotation * Quaternion.Euler(rotationX, 0f, rotationZ);

            Vector3 offset = new Vector3(
                wobble * 0.025f,
                bounce * 0.045f,
                -wobble * 0.018f
            );

            transform.localPosition = startLocalPosition + offset;

            yield return null;
        }

        transform.localScale = baseScale;
        transform.localRotation = baseRotation;
        transform.localPosition = startLocalPosition;

        currentPlaceRoutine = null;
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

    private void StopPlaceRoutine()
    {
        if (currentPlaceRoutine != null)
        {
            StopCoroutine(currentPlaceRoutine);
            currentPlaceRoutine = null;
        }

        transform.localScale = baseScale;
        transform.localRotation = baseRotation;
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