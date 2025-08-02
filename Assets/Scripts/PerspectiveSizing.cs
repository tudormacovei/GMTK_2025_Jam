using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class PerspectiveSizing : MonoBehaviour
{
    [SerializeField] private float minimumScaleFactor;
    [SerializeField] private float maximumScaleFactor;

    [SerializeField] private float minimumY;
    [SerializeField] private float maximumY;

    private Vector3 initialScale;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initialScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        // scale based on position on y-axis
        float yPos = GetComponent<Transform>().position.y;
        float diff = maximumY - minimumY;
        float alpha = Mathf.Max(maximumY - yPos, 0) / diff;

        float scale = Mathf.Lerp(minimumScaleFactor, maximumScaleFactor, alpha);
        GetComponent<Transform>().localScale = initialScale * scale;
        GetComponent<Transform>().localPosition = new(GetComponent<Transform>().localPosition.x, GetComponent<Transform>().localPosition.y, -scale);
    }
}
