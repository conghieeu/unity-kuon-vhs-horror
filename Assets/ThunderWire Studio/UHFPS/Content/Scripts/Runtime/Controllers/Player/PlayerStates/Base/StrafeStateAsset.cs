using UnityEngine;
using UHFPS.Scriptable;

namespace UHFPS.Runtime.States
{
    public abstract class StrafeStateAsset : PlayerStateAsset
    {
        public override FSMPlayerState InitState(PlayerStateMachine machine, PlayerStatesGroup group)
        {
            return new StrafePlayerState(machine, group);
        }

        public class StrafePlayerState : FSMPlayerState
        {
            protected StrafeMovementGroup strafeGroup;
            protected float movementSpeed;

            public StrafePlayerState(PlayerStateMachine machine, PlayerStatesGroup group) : base(machine)
            {
                strafeGroup = (StrafeMovementGroup)group;
            }

            public override void OnStateUpdate()
            {
                Vector3 wishDir = new(machine.Input.x, 0, machine.Input.y);
                wishDir = cameraLook.RotationX * wishDir;

                if (machine.IsGrounded)
                {
                    Accelerate(ref machine.Motion, wishDir, movementSpeed);
                    Friction(ref machine.Motion);
                    machine.Motion.y = -machine.PlayerControllerSettings.AntiBumpFactor;
                }
                else
                {
                    AirAccelerate(ref machine.Motion, wishDir, movementSpeed);
                }

                ApplyGravity(ref machine.Motion);
                PlayerHeightUpdate();
            }

            protected void Accelerate(ref Vector3 velocity, Vector3 wishDir, float wishSpeed)
            {
                // see if we are changing direction.
                float currentSpeed = Vector3.Dot(velocity, wishDir);

                // see how much to add.
                float addSpeed = wishSpeed - currentSpeed;

                // if not going to add any speed, done.
                if (addSpeed <= 0) return;

                // determine amount of accleration.
                float accelSpeed = strafeGroup.acceleration * wishSpeed * Time.deltaTime;

                // cap at addspeed.
                accelSpeed = Mathf.Min(accelSpeed, addSpeed);

                // adjust velocity.
                velocity += wishDir * accelSpeed;
            }

            protected void AirAccelerate(ref Vector3 velocity, Vector3 wishDir, float wishSpeed)
            {
                float wishspd = wishSpeed;

                // cap speed.
                wishspd = Mathf.Min(wishspd, strafeGroup.airAccelerationCap);

                // see if we are changing direction.
                float currentSpeed = Vector3.Dot(velocity, wishDir);

                // see how much to add.
                float addSpeed = wishspd - currentSpeed;

                // if not going to add any speed, done.
                if (addSpeed <= 0) return;

                // determine amount of accleration.
                float accelSpeed = strafeGroup.airAcceleration * wishSpeed * Time.deltaTime;

                // cap at addspeed.
                accelSpeed = Mathf.Min(accelSpeed, addSpeed);

                // adjust velocity.
                velocity += wishDir * accelSpeed;
            }

            protected void Friction(ref Vector3 velocity)
            {
                float speed = velocity.magnitude;
                float surfaceFriction = footstepsSystem && footstepsSystem.CurrentSurface != null
                    ? footstepsSystem.CurrentSurface.SurfaceFriction : 1f;

                if (speed != 0)
                {
                    float control = (speed < strafeGroup.deceleration) ? strafeGroup.deceleration : speed;
                    float drop = control * strafeGroup.friction * surfaceFriction * Time.deltaTime;
                    float newSpeed = Mathf.Max(speed - drop, 0) / speed;
                    velocity.x *= newSpeed;
                    velocity.z *= newSpeed;
                }
            }
        }
    }
}