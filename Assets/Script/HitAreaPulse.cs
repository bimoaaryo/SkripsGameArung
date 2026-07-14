using UnityEngine;

public class HitAreaPulse : MonoBehaviour
{
    public float pulseSpeed    = 2f;
    public float scaleMin      = 0.9f;
    public float scaleMax      = 1.15f;
    [Range(0f, 1f)] public float alphaMin = 0.15f;
    [Range(0f, 1f)] public float alphaMax = 0.45f;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = new Color(1f, 0f, 0f, alphaMin);
    }

    void Update()
    {
        float t     = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f; // 0..1
        float scale = Mathf.Lerp(scaleMin, scaleMax, t);
        float alpha = Mathf.Lerp(alphaMin, alphaMax, t);

        transform.localScale = Vector3.one * scale;

        if (sr != null)
        {
            Color c = sr.color;
            c.r = 1f; c.g = 0f; c.b = 0f;
            c.a = alpha;
            sr.color = c;
        }
    }
}