using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipManager : MonoBehaviour
{
    // Public movement variables.
    public float TimeToTarget;          // Timesteps for acceleration.
    public float SlowRadius;            // Radius at which to start slowing the ship down.
    public float AngularTolerance;      // Angle at which to start slowing the ship's rotation.
    public float MaxMoveSpeed;          // Maximum movement speed of the ship.
    public float MaxVelocity;           // Maximum velocity of the ship.
    public float AngularAcceleration;   // Angular acceleration to apply as torque to the ship.
    public float MaxAngularVelocity;    // Maximum angular velocity of the ship.

    public SpriteRenderer[] SpriteComponents;

    // Public reference to the animator component on the engine child object.
    public Animator EngineAnimator;

    // Private reference to the global game manager.
    private GameManager GameManager;

    // Private reference to the RigidBody2D component.
    private Rigidbody2D PhysicsComponent;

    // Target position for this ship to move towards.
    private Vector3 TargetPosition;
    
    // Radius at which this ship should stop moving entirely. Calculated from SlowRadius.
    private float StopRadius;

    // Reference to the current waypoint this ship is moving to.
    private GameObject CurrentWaypoint;

    // Bool for whether this ship should or should not move this tick.
    private bool bShouldMove = false;

    // On awake, get objects and components and calculate the stop radius.
    void Awake()
    {
        GameManager = Object.FindObjectOfType<GameManager>();
        PhysicsComponent = GetComponent<Rigidbody2D>();
        
        StopRadius = SlowRadius / 4.0f;

        GameManager.RegisterShipSprites(SpriteComponents);
    }
    
    // On fixed update, perform movement.
    void FixedUpdate()
    {
        if (bShouldMove)
        {
            // Calculate the current direction vector.
            Vector3 Direction = CurrentWaypoint.transform.position - PhysicsComponent.transform.position;

            // Calculate the distance between the two points.
            float Distance = Direction.magnitude;
            Direction.Normalize();
            
            // Calculate the angle between the ship's forward vector and the direction we need to head in.
            float Angle = Vector3.SignedAngle(Direction, PhysicsComponent.transform.right, Vector3.forward);

            // Map value to range before using in lerp.
            float MapValue = 180.0f - Mathf.Abs(Angle);
            float LerpValue = map(MapValue, 0.0f, 180.0f, 0.0f, MaxVelocity);

            float TargetSpeed = Mathf.Lerp(0, MaxVelocity, LerpValue);

            // Stop the ship if it gets within the stop radius, and end the movement.
            if (Distance < StopRadius)
            {
                TargetSpeed = 0.0f;
                PhysicsComponent.velocity *= 0.0f;

                EndMove();
            }
            // Else if we're in the slow radius, slow movement down.
            else if (Distance < SlowRadius)
            {
                TargetSpeed *= Distance / (SlowRadius * 10.0f);
            }

            // Otherwise, perform movement as normal.
            if (Distance > StopRadius)
            {
                if ((Mathf.Abs(Angle) > 1.0f))
                {
                    // Rotate the ship if we're not already facing the destination.
                    if (Mathf.Abs(PhysicsComponent.angularVelocity) < MaxAngularVelocity)
                    {
                        // Speed up the ship's rotation, up to the max angular velocity.
                        float AngularVelocityDiff = MaxAngularVelocity - PhysicsComponent.angularVelocity;
                        float AngularVelocity = (AngularAcceleration < AngularVelocityDiff) ? AngularAcceleration : AngularVelocityDiff;

                        if (Angle > 0.0f)
                        {
                            PhysicsComponent.AddTorque(-AngularVelocity);
                        }
                        else
                        {
                            PhysicsComponent.AddTorque(AngularVelocity);
                        }
                    }
                    else
                    {
                        // Slow the ship's rotation down, using the correct sign.
                        if (PhysicsComponent.angularVelocity < 0.0f)
                        {
                            PhysicsComponent.angularVelocity = -MaxAngularVelocity;
                        }
                        else
                        {
                            PhysicsComponent.angularVelocity = MaxAngularVelocity;
                        }
                    }
                }
            }

            // If we're approaching the desired rotation, slow down the rotation speed.
            if (Mathf.Abs(Angle) < AngularTolerance)
            {
                PhysicsComponent.angularVelocity *= Mathf.Abs(Angle) / AngularTolerance;
            }

            // Move the ship along its forward vector by the target speed we calculated from the angle before.
            Vector3 TargetVelocity = PhysicsComponent.transform.right;
            TargetVelocity.Normalize();
            TargetVelocity *= TargetSpeed;

            // Apply acceleration using TimeToTarget (if this step doesn't make sense, blame Ian Millington).
            Vector3 Steer = TargetVelocity;
            Steer /= TimeToTarget;

            // Clamp acceleration to within acceptable limits.
            if (Steer.magnitude > MaxMoveSpeed)
            {
                Steer.Normalize();
                Steer += Steer * (MaxMoveSpeed - Steer.magnitude);
            }

            // Apply new acceleration as a force.
            PhysicsComponent.AddForce(new Vector2(Steer.x, Steer.y));
        }
    }

    // Begin movement and destroy the previous waypoint, if there is one.
    public void BeginMove(Vector3 Position, GameObject Waypoint)
    {
        bShouldMove = true;
        
        if (EngineAnimator)
        {
            EngineAnimator.SetTrigger("StartMove");
        }

        TargetPosition = Position;

        if (CurrentWaypoint)
        {
            GameManager.RemoveWaypoint(CurrentWaypoint);
        }

        CurrentWaypoint = Waypoint;
    }

    // End movement and destroy the waypoint.
    private void EndMove()
    {
        bShouldMove = false;

        if (EngineAnimator)
        {
            EngineAnimator.SetTrigger("EndMove");
        }

        if (CurrentWaypoint)
        {
            GameManager.RemoveWaypoint(CurrentWaypoint);
        }
    }

    // Debug label.
    public void OnGUI()
    {
        //GUI.Label(new Rect(10, 10, 300, 20), "Angular Velocity: " + PhysicsComponent.angularVelocity);
    }

    // #TODO: Make this a global static function.
    private float map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }
}
