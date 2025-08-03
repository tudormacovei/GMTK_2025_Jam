using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class ModifyPostProcessing : MonoBehaviour
{
    [Header("Fade Settings")]
    [Tooltip("UI Image covering the screen, starts black.")]
    public Image fadeImage;
    [Tooltip("Time it takes to fade in from black at start.")]
    public float fadeDuration = 2.0f;

    [Header("Glitch Settings")]
    public float duration = 3.0f; // total duration of the distortion effect

    private LensDistortion lensDistortion;

    public Camera camera;
    public float startingCameraSize = 3f;
    private float initialCameraSize;

    private void Start()
    {
        // grab the lens distortion override
        var volume = GetComponent<Volume>();
        volume.profile.TryGet(out lensDistortion);

        // kick off the startup sequence
        StartCoroutine(StartupSequence());
        initialCameraSize = camera.orthographicSize;
        camera.orthographicSize = startingCameraSize;
    }

    private IEnumerator StartupSequence()
    {
        // 1) Fade in from black
        StartCoroutine(FadeFromBlack());

        // 2) Wait until 0.5s from the very start
        yield return new WaitForSeconds(0.2f);

        // 3) Begin glitch/distortion
        StartCoroutine(GlitchEffect());
    }

    private IEnumerator FadeFromBlack()
    {
        if (fadeImage == null)
        {
            yield break;
        }

        float elapsed = 0f;
        // ensure we start fully black
        Color c = fadeImage.color;
        c.a = 1f;
        fadeImage.color = c;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration) * Mathf.Clamp01(elapsed / fadeDuration);
            fadeImage.color = new Color(c.r, c.g, c.b, alpha);
            camera.orthographicSize = Mathf.Lerp(startingCameraSize, initialCameraSize, alpha);

            yield return null;
        }

        // ensure fully transparent
        fadeImage.color = new Color(c.r, c.g, c.b, 0f);
    }

    private IEnumerator GlitchEffect()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // x goes 0→1 over the course of 'duration'
            float x = elapsed / duration;

            // optional speed ramp
            float speed = 2.0f - x;

            // your sine-based color shift
            float frequency = 10f;
            float amplitude = 0.1f * (1f - x);
            float value = Mathf.Sin(elapsed * frequency) * amplitude;
            Shader.SetGlobalFloat("_colorshiftFactor", value);

            // ramp lens distortion from -1→0
            lensDistortion.intensity.value = -1f * (1f - x);

            elapsed += Time.deltaTime * speed;
            yield return null;
        }

        // make sure we clear it
        Shader.SetGlobalFloat("_colorshiftFactor", 0f);
        lensDistortion.intensity.value = 0f;
    }
}
