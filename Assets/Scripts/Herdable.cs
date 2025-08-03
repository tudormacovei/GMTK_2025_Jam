using UnityEngine;

public class Herdable : MonoBehaviour
{
    bool isBeingDestroyed = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.RegisterHerdable();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DestroyHerdable()
    {
        if (!isBeingDestroyed)
        {
            isBeingDestroyed = true;
            GameManager.Instance.UnregisterHerdable();
        }
        Destroy(gameObject);
    }
}
