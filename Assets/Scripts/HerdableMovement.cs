using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class HerdableMovement : MonoBehaviour
{
    [SerializeField] float roamingMovementSpeed = 0.5f;
    [SerializeField] float startledMovementSpeed = 1.0f;
    [SerializeField] float WaitingTime = 0.5f; // how long to wait in between roam iterations

    private float movementSpeed;

    [SerializeField] float movementPointSelectDistanceRoaming = 0.75f;
    [SerializeField] float movementPointSelectDistanceStartled = 1.5f; // how far away will the point that we move towards be, when we select a point to move towards?

    // two states: either startled or not
    // not startled means the herdable is roaming   
    private Vector3 pointToMoveTowards;
    private float elapsedTime;
    
    enum HerdableState
    {
        None,
        Roaming,
        Startled,
        AvoidingFence,
        Waiting
    }

    private HerdableState state;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        state = HerdableState.None;
        movementSpeed = 100.0f; // large value to make it visible if it hasn't been overwritten
        elapsedTime = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateState()
    {
        List<Collider2D> results = new();
        gameObject.GetComponent<CircleCollider2D>().Overlap(results);

        List<Vector3> fencesToAvoid = new();
        List<Vector3> herdersToAvoid = new();
        Vector3 directionToMoveTowards = Vector3.zero;

        foreach (var collider in results)
        {
            if (collider.gameObject.CompareTag("LevelBound"))
            {
                fencesToAvoid.Add(collider.gameObject.GetComponent<Transform>().position);
            }
            if (collider.gameObject.CompareTag("Herder"))
            {
                herdersToAvoid.Add(collider.gameObject.GetComponent<Transform>().position);
            }
        }

        if (fencesToAvoid.Count > 0)
        {
            state = HerdableState.AvoidingFence; 

            foreach (var position in fencesToAvoid)
            {
                directionToMoveTowards += position;
            }
            directionToMoveTowards /= fencesToAvoid.Count;
            directionToMoveTowards = gameObject.transform.position - directionToMoveTowards;

            // directionToMoveTowards might be zero here, possible issues if that's the case
            directionToMoveTowards.Normalize();
            pointToMoveTowards = directionToMoveTowards * movementPointSelectDistanceRoaming + gameObject.transform.position;
            movementSpeed = startledMovementSpeed;
        }
        else if (herdersToAvoid.Count > 0 && state != HerdableState.AvoidingFence) // code duplication :(
        {
            state = HerdableState.Startled;

            foreach (var position in herdersToAvoid)
            {
                directionToMoveTowards += position;
            }
            directionToMoveTowards /= herdersToAvoid.Count;
            directionToMoveTowards = gameObject.transform.position - directionToMoveTowards;

            // directionToMoveTowards might be zero here, possible issues if that's the case
            directionToMoveTowards.Normalize();
            pointToMoveTowards = directionToMoveTowards * movementPointSelectDistanceStartled + gameObject.transform.position;
            movementSpeed = startledMovementSpeed;
        }
        if (state == HerdableState.None)
        {
            directionToMoveTowards.Set(UnityEngine.Random.Range(-1.0f, 1.0f), UnityEngine.Random.Range(-1.0f, 1.0f), 0.0f);
            state = HerdableState.Roaming;

            // directionToMoveTowards might be zero here, possible issues if that's the case
            directionToMoveTowards.Normalize();
            pointToMoveTowards = directionToMoveTowards * movementPointSelectDistanceRoaming + gameObject.transform.position;
            movementSpeed = roamingMovementSpeed;
        }

        if (state != HerdableState.None) // techincally this will always evaluate to true, but check just in case
        {
            if (state == HerdableState.Waiting)
            {
                elapsedTime += Time.fixedDeltaTime; // this function is called in FixedUpdate
                if (elapsedTime < WaitingTime)
                {
                    return;
                }
                else
                {
                    elapsedTime = 0.0f;
                    state = HerdableState.None; // finished waiting
                }
            }

            // check if you have reached the pointToMoveTowards (the goal)
            Vector2 delta = new(gameObject.transform.position.x - pointToMoveTowards.x, gameObject.transform.position.y - pointToMoveTowards.y);

            if (delta.magnitude < 0.1f)
            {
                if (state == HerdableState.Roaming)
                {
                    state = HerdableState.Waiting;
                }
                else
                {
                    state = HerdableState.None; // reached current movement goal, reset state
                }
            }
        }
    }

    void HandleMovement()
    {
        if (state != HerdableState.None && state != HerdableState.Waiting)
        {
            // move toward point
            Vector3 direction = pointToMoveTowards - gameObject.transform.position;
            direction.Normalize();
            Vector3 newPosition = Time.fixedDeltaTime * movementSpeed * direction + gameObject.transform.position; 
            gameObject.GetComponent<Transform>().position = newPosition;
        }
    }

    private void FixedUpdate()
    {
        UpdateState();
        HandleMovement();
    }
}
