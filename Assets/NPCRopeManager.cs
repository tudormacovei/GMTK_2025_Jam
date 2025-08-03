using System.Collections.Generic;
using UnityEngine;

// NPC moves in a pre-defined path and lays down rope 
public class NPCRopeManager : MonoBehaviour
{
    [Header("Rope Generation")]
    [SerializeField] private GameObject ropeSegmentPrefab;
    [SerializeField] private float segmentSpacing = 0.3f;
    [SerializeField] private int anchorToDynamicRatio = 4;
    private List<GameObject> ropeSegments = new List<GameObject>();

    [Header("NPC / Rope Behaviour")]
    [SerializeField] private Transform[] pathPoints;
    private int currentPoint = 0;

    [SerializeField] private float moveSpeed = 2f;
    // [SerializeField] private float onRopeCompleteExplosionForce = 300f;

    public LayerMask detectionLayer;

    private Vector2 lastSegmentPosition;
    private GameObject lastSegment;

    private LineRenderer lineRenderer;

    private const float EPS = 0.05f; 

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        
        lastSegmentPosition = transform.position;
        lastSegment = null;

        transform.position = pathPoints[ 0 ].position;
    }

    void Update()
    {
        if ( currentPoint >= pathPoints.Length )
        {
            // TODO: Handle OnLoopComplete - add force to rope and make player fade out
            return;
        }

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

        UpdateLineRenderer();

        // Advance to next point if reached
        if ( Vector2.Distance( transform.position, target ) < EPS )
        {
            currentPoint++;
        }
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
}
