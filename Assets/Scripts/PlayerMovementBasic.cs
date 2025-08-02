using UnityEngine;

public class PlayerMovementBasic : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Vector2 move = new Vector2( Input.GetAxis( "Horizontal" ), Input.GetAxis( "Vertical" ) );
        rb.linearVelocity = move * speed;
    }
}