using UnityEngine;

public class PerspectiveSizing : MonoBehaviour
{
    [SerializeField] private float minimumScaleFactor;
    [SerializeField] private float maximumScaleFactor;

    [SerializeField] private float minimumY;
    [SerializeField] private float maximumY;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // scale based on position on y-axis
        float yPos = GetComponent<Transform>().position.y;
        float diff = maximumY - minimumY;
        float alpha = Mathf.Max(maximumY - yPos, 0) / diff;

        float scale = Mathf.Lerp(minimumScaleFactor, maximumScaleFactor, alpha);
        GetComponent<Transform>().localScale = new Vector3(scale, scale, 1);
    }
}
