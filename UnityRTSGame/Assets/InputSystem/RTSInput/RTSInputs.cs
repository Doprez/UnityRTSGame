using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RTSInputs : MonoBehaviour
{

	[Header("Character Input Values")]
	public Vector2 move;
	public Vector2 look;
	public Vector2 scroll;
	public bool sprint;
	public bool select;
	public bool EnableCameraRotate;

	public bool analogMovement;

	public void OnMove(InputValue value)
	{
		MoveInput(value.Get<Vector2>());
	}

	public void OnLook(InputValue value)
	{
		LookInput(value.Get<Vector2>());
	}

	public void OnSprint(InputValue value)
	{
		SprintInput(value.isPressed);
	}

    public void OnSelect(InputValue value)
    {
		SelectInput(value.isPressed);
    }

	public void OnScroll(InputValue value)
    {
		ScrollInput(value.Get<Vector2>());

	}

	public void OnEnableRotateCamera(InputValue value)
    {
		EnableCameraRotateInput(value.isPressed);
	}

	public void EnableCameraRotateInput(bool newRotateCameraState)
    {
		EnableCameraRotate = newRotateCameraState;
    }

	public void ScrollInput(Vector2 newScroll)
    {
		scroll = newScroll;
    }

    public void LookInput(Vector2 newLookDirection)
	{
		look = newLookDirection;
	}

	public void SprintInput(bool newSprintState)
	{
		sprint = newSprintState;
	}

	public void SelectInput(bool newSelectState)
	{
		select = newSelectState;
	}

	public void MoveInput(Vector2 newMoveDirection)
	{
		move = newMoveDirection;
	}

}
