using System.Collections.Generic;
using UnityEngine;

// Handles rope generation and intersection 
public class RopeManager : MonoBehaviour
{
    #region Rope Generation Params
    [SerializeField] private Material ropeMaterial;
    [SerializeField] private GameObject ropeSegmentPrefab;
    [SerializeField] private float segmentSpacing = 0.3f;
    [SerializeField] private int anchorToDymicRatio = 4;

    private Vector2 lastSegmentPosition;
    private GameObject lastSegment;

    private LineRenderer lineRenderer;
    #endregion

    #region Rope Intersection Params
    [SerializeField] private float intersectionThreshold = 0.2f; // How far away can the intersection points be TODO: Consider making this dynamic based on segment size or something 
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

    #region Rope Generation Logic
    
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

    #region Rope Intersection Logic
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
                    DoSomething();
                    
                    return; 
                }
            }
        }
    }

    void DoSomething()
    {
        // TODO[ziana]
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