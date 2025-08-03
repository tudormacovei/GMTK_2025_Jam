using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// Handles rope generation and intersection 
// Rope is drawn on left mouse click down. If a loop is not completed, rope dissappears on left mouse click up
// When a loop is completed, rope dissappears and all herdable objects inside the loop area dissapear as well
public class RopeManager : MonoBehaviour
{
    #region Rope Generation Params
    [SerializeField] private GameObject ropeSegmentPrefab;
    [SerializeField] private float segmentSpacing = 0.3f;
    [SerializeField] private int anchorToDynamicRatio = 4;
    [SerializeField] private float onRopeCompleteExplosionForce = 300f;
    [SerializeField] private float segmentLimit = 30;

    private Vector2 lastSegmentPosition;
    private GameObject lastSegment;

    private LineRenderer lineRenderer;
    #endregion

    #region Rope Intersection Params
    [SerializeField] private float intersectionThreshold = 0.2f; // How far away can the intersection points be TODO: Consider making this dynamic based on segment size or something 
    [SerializeField] private LayerMask herdablesLayer;
    private float checkInterval = 0.2f; // How often to check for intersections

    private float checkTimer = 0f;

    // Debug visualizer
    private Vector2? lastIntersectionPoint = null;
    #endregion

    private List<GameObject> ropeSegments = new List<GameObject>();
    private bool isGenerating = false;
    private bool isCleaningRope = false;

    public AudioClip segmentPlacement;
    public AudioClip finishRope;


    void Start()
    {
        InitRopeGenParams();
    }

    void Update()
    {
        HandleRopeGenInput();
        HandleRopeIntersection();
    }

    #region Generation Logic
    void InitRopeGenParams()
    {
        lineRenderer = GetComponent<LineRenderer>();    
        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = 0.2f;
        lineRenderer.endWidth = 0.2f;
    }

    void HandleRopeGenInput()
    {
        // Start new rope on mouse down
        if ( Input.GetMouseButton( 0 ) && !isCleaningRope ) 
        {
            isGenerating = true;
        }

        // Stop rope generation on mouse up
        if ( Input.GetMouseButtonUp( 0 ) && !isCleaningRope )
        {
            ClearRope();
        }

        // Generate rope while holding mouse
        if ( isGenerating )
        {
            if (ropeSegments.Count >= segmentLimit)
            {
                ClearRope();
                return;
            }

            float distance = Vector2.Distance( transform.position, lastSegmentPosition );

            if ( distance >= segmentSpacing )
            {
                SpawnSegment();
                lastSegmentPosition = transform.position;
            }
        }

        if (ropeSegments.Count > 0)
            UpdateLineRenderer();

    }
    
    void ClearRope()
    {
        isGenerating = false;
 
        lastIntersectionPoint = null;
        lastSegmentPosition = transform.position;
        lastSegment = null;

        for ( int i = 0; i < ropeSegments.Count; i++ )
        {
            Destroy( ropeSegments[ i ].gameObject );
        }
        ropeSegments.Clear();

        SetLineRendererAlpha( 1.0f );
        lineRenderer.positionCount = 0;
        isCleaningRope = false;
    }

    void SpawnSegment()
    {
        GameObject newSegment = Instantiate( ropeSegmentPrefab, transform.position, Quaternion.identity );
        AudioManager.Instance.SetSFXVolume(0.5f);

        // Not first segment 
        if (lastSegment != null)
        {
            HingeJoint2D joint = newSegment.GetComponent<HingeJoint2D>();
            joint.connectedBody = lastSegment.GetComponent<Rigidbody2D>();
            
            // Always make the last segment static so that it's attached to the Player 
            newSegment.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

            // Make some RBs static so that they anchor the whole rope and some dynamic so the rope has some physics
            if ( ropeSegments.Count % anchorToDynamicRatio != 0 )
                lastSegment.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            else
                AudioManager.Instance.PlaySfx(segmentPlacement);
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
    #endregion

    #region Intersection Check
    void HandleRopeIntersection()
    {
        if ( isGenerating )
        {
            // Check for intersection periodically
            checkTimer += Time.deltaTime;
            if ( checkTimer >= checkInterval )
            {
                checkTimer = 0f;
                CheckSelfIntersection();
            }
        }
    }

    void CheckSelfIntersection()
    {
        for ( int i = 0; i < ropeSegments.Count; i++ )
        {
            for ( int j = i + 2; j < ropeSegments.Count; j++ ) // Skip adjacent segments 
            {
                float dist = Vector2.Distance( ropeSegments[ i ].transform.position, ropeSegments[ j ].transform.position );
                if ( dist < intersectionThreshold )
                {
                    lastIntersectionPoint = ( ropeSegments[ i ].transform.position + ropeSegments[ j ].transform.position ) / 2f;
                    OnRopeLoopComplete();
                    
                    return; 
                }
            }
        }

        lastIntersectionPoint = null;
    }
    #endregion

    // Behaviour that happens when loop is complete
    #region Loop Complete Logic 
    List<GameObject> GetObjectsInsideLoop()
    {
        // Create polygon from rope segments 
        List<Vector2> polygon = new List<Vector2>();
        foreach ( GameObject seg in ropeSegments )
        {
            polygon.Add( seg.transform.position );
        }

        Vector2 center = GetLoopCenter( polygon );
        float radius = GetLoopRadius( center, polygon );
        
        // Variable colliders will hold all objects that INTERSECT the circle (not only objects fully inside)
        Collider2D[] colliders = Physics2D.OverlapCircleAll( center, radius, herdablesLayer );
        // Debug.Log( "Found " + colliders.Length + " viable objects inside rope loop approx. circle.");

        List<GameObject> objectsInside = new List<GameObject>();
        // For extra precision, make sure the objects are fully IN the polygon made by the rope
        foreach ( var col in colliders )
        {
            Vector2 objPos = col.transform.position;
            if ( IsPointInsidePolygon( objPos, polygon ) )
            {
                objectsInside.Add( col.gameObject );
            }
        }

        return objectsInside;
    }

    // Apply force to the rope to simulate it stretching and give visual feedback to the player
    void OnRopeLoopComplete()
    {
        // Play finish audio, stop any playing audio clip
        AudioManager.Instance.SetSFXVolume(1.0f);
        AudioManager.Instance.PlaySfxInterrupt(finishRope);

        isGenerating = false;
        isCleaningRope = true;

        // Create polygon from rope segments
        List<Vector2> polygon = new List<Vector2>();
        foreach ( GameObject seg in ropeSegments )
        {
            polygon.Add( seg.transform.position );
        }
        Vector3 center = GetLoopCenter( polygon );

        // Apply a force to each segment RB
        foreach ( GameObject seg in ropeSegments )
        {
            Rigidbody2D rb = seg.GetComponent<Rigidbody2D>();
            if ( rb != null )
            {
                Vector2 explosionDir = ( rb.transform.position - center ).normalized;
                rb.AddForce( explosionDir * onRopeCompleteExplosionForce );
            }
        }

        List<GameObject> objToDelete = GetObjectsInsideLoop();
        StartCoroutine( FadeAndDestroyRopeAndObjects( objToDelete ) );
    }

    IEnumerator FadeAndDestroyRopeAndObjects( List<GameObject> objToDelete, float fadeDuration = 0.6f )
    {
        // Wait a bit before fading so the force on the rope is visible
        yield return new WaitForSeconds( 0.6f );

        // Fade rope and objects 
        float elapsed = 0f;
        while ( elapsed < fadeDuration )
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp( 1f, 0f, elapsed / fadeDuration );
            SetLineRendererAlpha( alpha );
            foreach ( var obj in objToDelete )
            {
                if ( obj == null )
                    continue;

                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                if ( sr != null )
                {
                    SetSpriteRendererAlpha( sr, alpha );
                }
            }

            yield return null;
        }

        // Destroy the objects after fade is complete 
        foreach ( var obj in objToDelete )
        {
            if (obj.GetComponent<Herdable>() != null)
            {
                obj.GetComponent<Herdable>().DestroyHerdable();
            }
            else
            {
                Destroy(obj); // keep it in just to be safe
            }
        }
        
        ClearRope();
    }

    #region Utils
    Vector2 GetLoopCenter( List<Vector2> points )
    {
        Vector2 sum = Vector2.zero;
        foreach ( var p in points ) 
            sum += p;

        return sum / points.Count;
    }

    float GetLoopRadius( Vector2 center, List<Vector2> points )
    {
        float radius = 0f;
        foreach ( var p in points )
        {
            float dist = Vector2.Distance( center, p );
            if ( dist > radius ) 
                radius = dist;
        }

        return radius;
    }

    // Check using the even-odd rule
    bool IsPointInsidePolygon( Vector2 point, List<Vector2> polygon )
    {
        int crossings = 0;
        for ( int i = 0; i < polygon.Count; i++ )
        {
            // Form an edge of the polygon
            Vector2 a = polygon[ i ];
            Vector2 b = polygon[ ( i + 1 ) % polygon.Count ];

            // Check if the horizontal line through point.y intersects our edge
            // Only count intersections that are to the right of our point 
            if ( ( ( a.y > point.y ) != ( b.y > point.y ) ) &&
                ( point.x < ( b.x - a.x ) * ( point.y - a.y ) / ( b.y - a.y ) + a.x ) )
            {
                crossings++;
            }
        }
        return ( crossings % 2 ) == 1;
    }
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
    #endregion

    void OnDrawGizmos()
    {
        if ( lastIntersectionPoint.HasValue )
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere( lastIntersectionPoint.Value, segmentSpacing );
        }
    }
}