using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ModifyPostProcessing : MonoBehaviour
{
    LensDistortion lensDistortion;
    public float duration = 3.0f; // Duration of the effect in seconds

    private void Start()
    {
        var v = GetComponent<Volume>();
        v.profile.TryGet<LensDistortion>(out lensDistortion);

        // Start the glitchy effect at the beginning of the game
        StartCoroutine(GlitchEffect());
    }

    // Coroutine that handles the glitchy oscillation
    private System.Collections.IEnumerator GlitchEffect()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {

            float x = elapsed / duration; // Normalize elapsed time to [0, 1]
            float speed = 2.0f - x;

            float frequency = 10f; // Speed of oscillation
            float amplitude = 0.1f * (1.0f - x);
            float value = Mathf.Sin(elapsed * frequency) * amplitude;

            Shader.SetGlobalFloat("_colorshiftFactor", value);

            lensDistortion.intensity.value = (-1.0f) * (1.0f - x);

            elapsed += Time.deltaTime * speed;
            yield return null;
        }

        // Reset value to 0 after the effect ends
        Shader.SetGlobalFloat("_colorshiftFactor", 0f);
    }

    private void Update()
    {
        // No need to update continuously after the effect
    }
}