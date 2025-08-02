using Unity.VisualScripting;
using UnityEngine;

public class ModifyPostProcessing : MonoBehaviour
{
    public Material postProcessingMaterial;

    private void Start()
    {
        // Start the glitchy effect at the beginning of the game
        StartCoroutine(GlitchEffect());
    }

    // Coroutine that handles the glitchy oscillation
    private System.Collections.IEnumerator GlitchEffect()
    {
        float duration = 2.0f; // Duration of the effect in seconds
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = elapsed / duration; // Normalize elapsed time to [0, 1]
            float speed = 2.0f - x;

            float frequency = 10f; // Speed of oscillation
            float amplitude = 0.1f * (1.0f - x);
            float value = Mathf.Sin(elapsed * frequency) * amplitude;

            Shader.SetGlobalFloat("_colorshiftFactor", value);

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