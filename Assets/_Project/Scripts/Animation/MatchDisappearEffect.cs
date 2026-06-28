using System.Collections;
using UnityEngine;

public class MatchDisappearEffect : MonoBehaviour
{
    [SerializeField] private float duration = 0.24f;
    [SerializeField] private float popScale = 1.25f;
    [SerializeField] private float liftHeight = 0.16f;
    [SerializeField] private float rotationAmount = 18f;

    private Vector3 startScale;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Awake()
    {
        startScale = transform.localScale;
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    public void Play()
    {
        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            float pop = t < 0.35f
                ? Mathf.Lerp(1f, popScale, t / 0.35f)
                : Mathf.Lerp(popScale, 0f, (t - 0.35f) / 0.65f);

            float lift = Mathf.Sin(t * Mathf.PI) * liftHeight;
            float wobble = Mathf.Sin(t * Mathf.PI * 6f) * rotationAmount * (1f - t);

            transform.localScale = startScale * pop;
            transform.position = startPosition + Vector3.up * lift;
            transform.rotation = startRotation * Quaternion.Euler(0f, wobble, wobble);

            yield return null;
        }

        Destroy(gameObject);
    }
}