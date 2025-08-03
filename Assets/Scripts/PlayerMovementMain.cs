using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 10f;

    [Header("Jump/Bounce Settings")]
    [SerializeField] private float bounceHeight = 0.3f;
    [SerializeField] private float bounceDistance = 1f;

    [Header("Components")]
    private Rigidbody2D rb;
    private Animator animator;
    private Transform visualChild; // The child object with sprite renderer

    [Header("Input")]
    private Vector2 movementInput;
    private bool isRunning;

    [Header("Current State")]
    [SerializeField] private Vector2 currentVelocity;
    [SerializeField] private bool isMoving;

    [Header("Bounce State")]
    private float distanceTraveled = 0f;
    private Vector3 originalChildPosition;
    private Vector3 lastPosition;

    [SerializeField]private Animator playerAnimator;

    private void Awake()
    {
        // Get required components
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        // Find the visual child (the one with the sprite renderer)
        SpriteRenderer childSprite = GetComponentInChildren<SpriteRenderer>();
        if (childSprite != null)
        {
            visualChild = childSprite.transform;
            originalChildPosition = visualChild.localPosition;
        }

        // Configure Rigidbody2D settings
        rb.freezeRotation = true;
        rb.gravityScale = 0f; // No gravity for top-down movement

        lastPosition = transform.position;
    }

    private void Update()
    {
        HandleInput();
        HandleBounceEffect();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleInput()
    {
        // Get input from both WASD and arrow keys
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        movementInput = new Vector2(horizontal, vertical).normalized;

        // Check if running (shift key)
        isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // Update movement state
        isMoving = movementInput.magnitude > 0.1f;
    }

    private void HandleMovement()
    {
        // Calculate target velocity
        float targetSpeed = isRunning ? runSpeed : walkSpeed;
        Vector2 targetVelocity = movementInput * targetSpeed;

        // Smooth acceleration/deceleration
        if (isMoving)
        {
            currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentVelocity = Vector2.MoveTowards(currentVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
        }

        // Apply movement to rigidbody (moves the parent)
        rb.linearVelocity = currentVelocity;

        // Handle sprite flipping based on movement direction
        SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            if (movementInput.x > 0.1f)
            {
                spriteRenderer.flipX = false;
            }
            else if (movementInput.x < -0.1f)
            {
                spriteRenderer.flipX = true;
            }
        }
    }


    private void HandleBounceEffect()
    {
        if (visualChild == null) return;

        if (isMoving)
        {
            // Calculate distance traveled since last frame
            Vector3 currentPosition = transform.position;
            float frameDistance = Vector3.Distance(currentPosition, lastPosition);
            distanceTraveled += frameDistance;
            lastPosition = currentPosition;

            // Calculate bounce based on distance traveled
            float bounceProgress = (distanceTraveled / bounceDistance) * Mathf.PI * 2f; // Full cycle = 2π

            // Create a snappy bounce that's tied to movement distance
            float sineWave = Mathf.Sin(bounceProgress);
            float bounceOffset = Mathf.Max(0, sineWave) * bounceHeight;

            // Apply vertical bounce to the visual child only
            Vector3 bouncePosition = originalChildPosition;
            bouncePosition.y += bounceOffset;
            visualChild.localPosition = bouncePosition;
        }
        else
        {
            // Smoothly return to original position when not moving
            visualChild.localPosition = Vector3.Lerp(visualChild.localPosition, originalChildPosition, Time.deltaTime * 8f);

            // Reset distance when not moving
            distanceTraveled = 0f;
            lastPosition = transform.position;
        }
    }

    private void UpdateAnimations()
    {
        /*if (animator != null)
        {
            // Set animation parameters
            animator.SetFloat("Speed", currentVelocity.magnitude);
            animator.SetBool("IsMoving", isMoving);
            animator.SetBool("IsRunning", isRunning && isMoving);

            // Set movement direction for directional animations
            if (isMoving)
            {
                animator.SetFloat("MoveX", movementInput.x);
                animator.SetFloat("MoveY", movementInput.y);
            }
        }*/

        if (animator != null)
        {
            animator.SetBool( "isWalking", isMoving );
            animator.SetFloat( "Speed", currentVelocity.magnitude );
        }
    }

    // Public methods for external scripts
    public Vector2 GetMovementDirection()
    {
        return movementInput;
    }

    public float GetCurrentSpeed()
    {
        return currentVelocity.magnitude;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public bool IsRunning()
    {
        return isRunning && isMoving;
    }

    // Method to temporarily disable movement (useful for cutscenes, menus, etc.)
    public void SetMovementEnabled(bool enabled)
    {
        this.enabled = enabled;
        if (!enabled)
        {
            rb.linearVelocity = Vector2.zero;
            currentVelocity = Vector2.zero;
        }
    }
}