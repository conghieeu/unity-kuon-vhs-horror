using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;

		[Header("Gameplay Input Values")]
		public bool interact;
		public bool flashlight;
		public bool dropItem;
		public bool pause;
		public bool zoom;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		public void OnInteract(InputValue value)
		{
			InteractInput(value.isPressed);
		}

		public void OnFlashlight(InputValue value)
		{
			FlashlightInput(value.isPressed);
		}

		public void OnDropItem(InputValue value)
		{
			DropItemInput(value.isPressed);
		}

		public void OnPause(InputValue value)
		{
			PauseInput(value.isPressed);
		}

		public void OnZoom(InputValue value)
		{
			ZoomInput(value.isPressed);
		}
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		public void InteractInput(bool newInteractState)
		{
			interact = newInteractState;
		}

		public void FlashlightInput(bool newFlashlightState)
		{
			flashlight = newFlashlightState;
		}

		public void DropItemInput(bool newDropItemState)
		{
			dropItem = newDropItemState;
		}

		public void PauseInput(bool newPauseState)
		{
			pause = newPauseState;
		}

		public void ZoomInput(bool newZoomState)
		{
			zoom = newZoomState;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}