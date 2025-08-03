using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NPC moves in a pre-defined path and lays down rope 
public class NPCRopeManager : MonoBehaviour
{
    [SerializeField] private GameObject Player;

    [Header("Rope Generation")]
    [SerializeField] private GameObject ropeSegmentPrefab;
    [SerializeField] private float segmentSpacing = 0.3f;
    [SerializeField] private int anchorToDynamicRatio = 4;
    private List<GameObject> ropeSegments = new List<GameObject>();

    [Header("NPC / Rope Behaviour")]
    [SerializeField] private Transform[] pathPoints;
    private int currentPoint = 0;

    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float startDelay = 6f;
    [SerializeField] private float onRopeCompleteExplosionForce = 500f;

    public LayerMask detectionLayer;

    private Vector2 lastSegmentPosition;
    private GameObject lastSegment;

    private LineRenderer lineRenderer;

    private bool isLoopComplete = false;

    private const float EPS = 0.05f; 

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = 0.2f;
        lineRenderer.endWidth = 0.2f;
        
        lastSegmentPosition = transform.position;
        lastSegment = null;

        transform.position = pathPoints[ 0 ].position;
    }

    void Update()
    {
        startDelay -= Time.deltaTime;
        if ( startDelay > 0.0f )
            return;

        if ( currentPoint < pathPoints.Length )
        {
            // Move toward the path point
            Vector2 target = pathPoints[ currentPoint ].position;
            transform.position = Vector2.MoveTowards( transform.position, target, moveSpeed * Time.deltaTime );

            // Lay rope as NPC moves
            float distance = Vector2.Distance( transform.position, lastSegmentPosition );
            if ( distance >= segmentSpacing )
            {
                SpawnSegment();
                lastSegmentPosition = transform.position;
            }

            // Advance to next point if reached
            if ( Vector2.Distance( transform.position, target ) < EPS )
            {
                currentPoint++;
            }
        }
        else if ( !isLoopComplete )
        {
            isLoopComplete = true;
            OnLoopComplete();
        }

        UpdateLineRenderer();
    }

    // TODO: Add Audio
    void SpawnSegment()
    {
        GameObject newSegment = Instantiate( ropeSegmentPrefab, transform.position, Quaternion.identity );

        // Not first segment 
        if ( lastSegment != null )
        {
            HingeJoint2D joint = newSegment.GetComponent<HingeJoint2D>();
            joint.connectedBody = lastSegment.GetComponent<Rigidbody2D>();

            // Always make the last segment static so that it's attached to the NPC 
            newSegment.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

            // Make some RBs static so that they anchor the whole rope and some dynamic so the rope has some physics
            if ( ropeSegments.Count % anchorToDynamicRatio != 0 )
                lastSegment.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        }

        lastSegment = newSegment;
        ropeSegments.Add( newSegment );
        lineRenderer.positionCount = ropeSegments.Count;
    }

    void UpdateLineRenderer()
    {
        for ( int i = 0; i < ropeSegments.Count; i++ )
        {
            Vector3 position = ropeSegments[ i ].transform.position;
            position.z = 3; // Render the rope behind all other game objects   

            lineRenderer.SetPosition( i, position );
        }
    }

    void OnLoopComplete()
    {
        StartCoroutine( FadeRopeAndPlayer() );
    }

    IEnumerator FadeRopeAndPlayer( float fadeDuration = 2f )
    {
        // Apply a force to each segment RB
        foreach ( GameObject seg in ropeSegments )
        {
            Rigidbody2D rb = seg.GetComponent<Rigidbody2D>();
            if ( rb != null && Player != null)
            {
                Vector2 explosionDir = ( rb.transform.position - Player.transform.position ).normalized;
                rb.AddForce( explosionDir * onRopeCompleteExplosionForce );
            }
        }

        // Wait a bit before fading so the force on the rope is visible
        yield return new WaitForSeconds( 1f );

        // Fade rope and objects 
        float elapsed = 0f;
        while ( elapsed < fadeDuration )
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp( 1f, 0f, elapsed / fadeDuration );
            SetLineRendererAlpha( alpha );
            SetSpriteRendererAlpha( Player.GetComponentInChildren<SpriteRenderer>(), alpha );

            yield return null;
        }

        GameManager.Instance.LoadNextLevel();
    }

    #region Utils
    void SetLineRendererAlpha( float alpha )
    {
        Color c = lineRenderer.material.color;
        c.a = alpha;
        lineRenderer.material.color = c;
    }
    void SetSpriteRendererAlpha( SpriteRenderer sr, float alpha )
    {
        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }
    #endregion
}
