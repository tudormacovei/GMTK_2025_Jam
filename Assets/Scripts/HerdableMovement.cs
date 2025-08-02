using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class HerdableMovement : MonoBehaviour
{
    [SerializeField] float movementSpeed = 1.0f;

    [SerializeField] float movementPointSelectDistance = 1.0f; // how far away will the point that we move towards be, when we select a point to move towards?

    // two states: either startled or not
    // not startled means the herdable is roaming   
    private Vector3 pointToMoveTowards;

    CircleCollider2D triggeringCollider; 

    enum HerdableState
    {
        None,
        Roaming,
        Startled
    }

    private HerdableState state;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        state = HerdableState.None;
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
            state = HerdableState.Startled;

            foreach (var position in fencesToAvoid)
            {
                directionToMoveTowards += position;
            }
            directionToMoveTowards /= fencesToAvoid.Count;
            directionToMoveTowards = gameObject.transform.position - directionToMoveTowards;

            // directionToMoveTowards might be zero here, possible issues if that's the case
            directionToMoveTowards.Normalize();
            pointToMoveTowards = directionToMoveTowards * movementPointSelectDistance + gameObject.transform.position;
        }
        else if (herdersToAvoid.Count > 0) // code duplication :(
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
            pointToMoveTowards = directionToMoveTowards * movementPointSelectDistance + gameObject.transform.position;
        }
        if (state == HerdableState.None)
        {
            directionToMoveTowards.Set(UnityEngine.Random.Range(-1.0f, 1.0f), UnityEngine.Random.Range(-1.0f, 1.0f), 0.0f);
            state = HerdableState.Roaming;

            // directionToMoveTowards might be zero here, possible issues if that's the case
            directionToMoveTowards.Normalize();
            pointToMoveTowards = directionToMoveTowards * movementPointSelectDistance + gameObject.transform.position;
        }

        if (state != HerdableState.None) // techincally this will always evaluate to true, but check just in case
        {

            // check if you have reached the pointToMoveTowards (the goal)
            Vector2 delta = new(gameObject.transform.position.x - pointToMoveTowards.x, gameObject.transform.position.y - pointToMoveTowards.y);

            if (delta.magnitude < 0.5f)
            {
                state = HerdableState.None; // reached current movement goal, reset state
            }
        }
    }

    void HandleMovement()
    {
        if (state != HerdableState.None)
        {
            // move toward point
            Vector3 direction = pointToMoveTowards - gameObject.transform.position;
            direction.Normalize();
            Vector3 newPosition = Time.fixedDeltaTime * movementSpeed * direction + gameObject.transform.position; 
            gameObject.GetComponent<Transform>().position = newPosition;
            // if reached point within epsilon, set state to None 
        }
    }

    private void FixedUpdate()
    {
        UpdateState();
        HandleMovement();
        // set state
        // process state

    }
}
