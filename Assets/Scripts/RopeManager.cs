using System.Collections.Generic;
using UnityEngine;

// Handles rope generation and intersection 
public class RopeManager : MonoBehaviour
{
    #region Rope Generation Params
    [SerializeField] private Material ropeMaterial;
    [SerializeField] private GameObject ropeSegmentPrefab;
    [SerializeField] private float segmentSpacing = 0.3f;
    [SerializeField] private int anchorToDynamicRatio = 4;

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
        lineRenderer.material = new Material( ropeMaterial );
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
    }

    void HandleRopeGenInput()
    {
        // Start new rope on mouse down
        if ( Input.GetMouseButtonDown( 0 ) )
        {
            isGenerating = true;
        }

        // Stop rope generation on mouse up
        if ( Input.GetMouseButtonUp( 0 ) )
        {
            ClearRope();
            isGenerating = false;
        }

        // Generate rope while holding mouse
        if ( isGenerating )
        {
            float distance = Vector2.Distance( transform.position, lastSegmentPosition );
            if ( distance >= segmentSpacing )
            {
                SpawnSegment();
                lastSegmentPosition = transform.position;
            }

            UpdateLineRenderer();
        }
    }

    void ClearRope()
    {
        lastSegmentPosition = transform.position;
        lastSegment = null;

        for ( int i = 0; i < ropeSegments.Count; i++ )
        {
            Destroy( ropeSegments[ i ].gameObject );
        }
        ropeSegments.Clear();

        lineRenderer.positionCount = 0;
    }

    void SpawnSegment()
    {
        GameObject newSegment = Instantiate( ropeSegmentPrefab, transform.position, Quaternion.identity );
        
        // Make some RBs static so that they anchor the whole rope
        if ( ropeSegments.Count % anchorToDymicRatio == 0 )
            newSegment.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

        // Not first segment 
        if (lastSegment != null)
        {
            HingeJoint2D joint = newSegment.GetComponent<HingeJoint2D>();
            joint.connectedBody = lastSegment.GetComponent<Rigidbody2D>();
        }

        lastSegment = newSegment;
        ropeSegments.Add( newSegment );
        lineRenderer.positionCount = ropeSegments.Count;
    }

    void UpdateLineRenderer()
    {
        for ( int i = 0; i < ropeSegments.Count; i++ )
        {
            lineRenderer.SetPosition( i, ropeSegments[ i ].transform.position );
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
                    CheckObjectsInsideLoop();
                    
                    return; 
                }
            }
        }

        lastIntersectionPoint = null;
    }
    #endregion

    #region Intersection Logic 

    void CheckObjectsInsideLoop()
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

        // For extra precision, make sure the objects are fully IN the polygon made by the rope
        foreach ( var col in colliders )
        {
            Vector2 objPos = col.transform.position;
            if ( IsPointInsidePolygon( objPos, polygon ) )
            {
                Debug.Log( "Object inside rope loop: " + col.name );
            }
        }
    }

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

public class RopeLoopDetector : MonoBehaviour
{
    public List<Vector2> ropePoints; // Points forming the rope
    public LayerMask detectionLayer; // Layer of objects to check

    void CheckObjectsInsideLoop()
    {
        if ( !IsLoopClosed( ropePoints ) ) return;

        Collider2D[] colliders = Physics2D.OverlapCircleAll( GetLoopCenter( ropePoints ), GetLoopRadius( ropePoints ), detectionLayer );

        foreach ( var col in colliders )
        {
            Vector2 objPos = col.transform.position;
            if ( IsPointInsidePolygon( objPos, ropePoints ) )
            {
                Debug.Log( "Object inside loop: " + col.name );
            }
        }
    }

    bool IsLoopClosed( List<Vector2> points )
    {
        // Simple check: first and last points are close enough
        return Vector2.Distance( points[ 0 ], points[ points.Count - 1 ] ) < 0.1f;
    }

    Vector2 GetLoopCenter( List<Vector2> points )
    {
        Vector2 sum = Vector2.zero;
        foreach ( var p in points ) sum += p;
        return sum / points.Count;
    }

    float GetLoopRadius( List<Vector2> points )
    {
        Vector2 center = GetLoopCenter( points );
        float maxDist = 0f;
        foreach ( var p in points )
        {
            float dist = Vector2.Distance( center, p );
            if ( dist > maxDist ) maxDist = dist;
        }
        return maxDist;
    }

    bool IsPointInsidePolygon( Vector2 point, List<Vector2> polygon )
    {
        int crossings = 0;
        for ( int i = 0; i < polygon.Count; i++ )
        {
            Vector2 a = polygon[ i ];
            Vector2 b = polygon[ ( i + 1 ) % polygon.Count ];

            if ( ( ( a.y > point.y ) != ( b.y > point.y ) ) &&
                ( point.x < ( b.x - a.x ) * ( point.y - a.y ) / ( b.y - a.y ) + a.x ) )
            {
                crossings++;
            }
        }
        return ( crossings % 2 ) == 1;
    }
}