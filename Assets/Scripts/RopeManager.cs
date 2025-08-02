using System.Collections.Generic;
using UnityEngine;

// Handles rope generation
public class RopeManager : MonoBehaviour
{
    [SerializeField] private Material ropeMaterial;
    [SerializeField] private GameObject ropeSegmentPrefab;
    [SerializeField] private float segmentSpacing = 0.3f;
    [SerializeField] private int anchorToDymicRatio = 4;

    private Vector2 lastSegmentPosition;
    private GameObject lastSegment;
    private bool isGenerating = false;

    private List<GameObject> ropeSegments = new List<GameObject>();
    private LineRenderer lineRenderer;

    void Start()
    {
        InitRopeGenParams();
    }

    void Update()
    {
        HandleRopeGenInput();
    }

    #region Rope Generation
    
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
}