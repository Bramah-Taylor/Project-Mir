using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipManager : MonoBehaviour
{
    public float Acceleration;
    public float MaxVelocity;
    public float AngularAcceleration;
    public float MaxAngularVelocity;

    private GameManager GameManager;

    private Rigidbody2D PhysicsComponent;

    private Vector3 TargetPosition;
    private Vector3 TargetForwardVector;
    private float TargetRotation;

    private float StartingDistance;

    private GameObject CurrentWaypoint;

    private bool bShouldMove = false;

    void Awake()
    {
        GameManager = Object.FindObjectOfType<GameManager>();
        PhysicsComponent = GetComponent<Rigidbody2D>();
    }
    
    void FixedUpdate()
    {
        if (bShouldMove)
        {
            // Gonna need some custom steering behaviours. Fuck.
        }
    }

    public void BeginMove(Vector3 Position, GameObject Waypoint)
    {
        bShouldMove = true;

        TargetPosition = Position;
        TargetForwardVector = Position - transform.position;
        TargetRotation = Vector2.Angle(Vector2.right, TargetForwardVector);

        if (CurrentWaypoint)
        {
            GameManager.RemoveWaypoint(CurrentWaypoint);
        }

        CurrentWaypoint = Waypoint;
    }

    private void EndMove()
    {
        bShouldMove = false;

        if (CurrentWaypoint)
        {
            GameManager.RemoveWaypoint(CurrentWaypoint);
        }
    }
}
