using UnityEngine;
using UHFPS.Scriptable;

namespace UHFPS.Runtime.States
{
    public abstract class SmoothStateAsset : PlayerStateAsset
    {
        public override FSMPlayerState InitState(PlayerStateMachine machine, PlayerStatesGroup group)
        {
            return new SmoothPlayerState(machine, group);
        }

        public class SmoothPlayerState : FSMPlayerState
        {
            protected SmoothMovementGroup smoothGroup;
            protected float movementSpeed;

            public SmoothPlayerState(PlayerStateMachine machine, PlayerStatesGroup group) : base(machine)
            {
                smoothGroup = (SmoothMovementGroup)group;
            }

            public override void OnStateUpdate()
            {
                Vector3 inputDir = new(machine.Input.x, 0, machine.Input.y);
                Vector3 wishDir = cameraLook.TransformWishDir(inputDir);

                if (machine.IsGrounded)
                {
                    float baseFriction = smoothGroup.friction;
                    float surfaceFriction = footstepsSystem && footstepsSystem.CurrentSurface != null
                                            ? footstepsSystem.CurrentSurface.SurfaceFriction : 1f;

                    float friction = baseFriction * surfaceFriction;
                    float acceleration = smoothGroup.acceleration * friction;
                    float deceleration = smoothGroup.deceleration * friction;

                    Vector3 targetVelocity = wishDir * movementSpeed;
                    float targetAccel = inputDir.magnitude > 0 ? acceleration : deceleration;

                    machine.Motion = Vector3.Lerp(machine.Motion, targetVelocity, Time.deltaTime * targetAccel);
                    machine.Motion.y = -machine.PlayerControllerSettings.AntiBumpFactor;
                }

                ApplyGravity(ref machine.Motion);
                PlayerHeightUpdate();
            }
        }
    }
}