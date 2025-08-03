using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimateCassete : MonoBehaviour
{
    public float animationTime = 3f;
    float elapsedTime = 0.0f;

    bool didCallLoad = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (elapsedTime < animationTime)
        {
            Vector3 newPosition = gameObject.transform.localPosition;

            float x = elapsedTime / animationTime;
            float yPos = Mathf.Tan(x * 2 - 1);
            yPos = yPos * yPos * yPos;

            newPosition.y = yPos + 0.5f;
            gameObject.transform.localPosition = newPosition;
            elapsedTime += Time.deltaTime;
        }
        else if (!didCallLoad)
        {
            didCallLoad = true;
            GameManager.Instance.LoadNextLevel();
        }
    }
}
