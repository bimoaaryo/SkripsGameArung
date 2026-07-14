using UnityEngine;
using System.Collections;


public class PlayerBlink : MonoBehaviour
{

    private SpriteRenderer sr;
    private Coroutine      blinkRoutine;

    public bool SedangBlink { get; private set; } = false;


    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }


    public void MulaiBlink(float durasi, float interval)
    {
        if (SedangBlink) return;

        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        blinkRoutine = StartCoroutine(BlinkRoutine(durasi, interval));
    }


    IEnumerator BlinkRoutine(float durasi, float interval)
    {
        SedangBlink = true;

        if (sr == null) sr = GetComponent<SpriteRenderer>();

        float elapsed = 0f;
        while (elapsed < durasi)
        {
            if (sr != null) sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        if (sr != null) sr.enabled = true;
        SedangBlink = false;
        blinkRoutine = null;
    }
}