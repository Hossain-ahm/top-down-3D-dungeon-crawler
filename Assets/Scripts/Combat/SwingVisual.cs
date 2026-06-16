using UnityEngine;

public class SwingVisual : MonoBehaviour
{
    public float arcAngle = 90f;
    public float duration = 0.15f;

    private float _elapsed;
    private float _startAngle;

    private void Start()
    {
        _startAngle = -arcAngle / 2f;
        transform.localRotation = Quaternion.Euler(0f, _startAngle, 0f);
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsed / duration);
        float currentAngle = Mathf.Lerp(_startAngle, _startAngle + arcAngle, t);
        transform.localRotation = Quaternion.Euler(0f, currentAngle, 0f);

        if (t >= 1f)
            Destroy(gameObject);
    }
}