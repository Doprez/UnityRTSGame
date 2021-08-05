using Cinemachine;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;


public class PlayerInputStateHandler : MonoBehaviour
{
    public PlayerInputState playerInputState;
    public GameObject UnitCameraRoot;
    public GameObject UnitCharacter;
    public GameObject RTSCharacter;
    public GameObject ThirdPersonFollower;
    public GameObject RTSFollower;

    // Start is called before the first frame update
    void Start()
    {
        TogglePlayerInputStates();
    }

    private void FixedUpdate()
    {
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            TogglePlayerInputStates();
        }
    }

    public void TogglePlayerInputStates()
    {

        switch (playerInputState)
        {
            case PlayerInputState.ThirdPerson:
                EnableRTS(false);
                EnableThirdPerson(true);
                playerInputState = PlayerInputState.Strategy;
                break;
            case PlayerInputState.Strategy:
                EnableThirdPerson(false);
                EnableRTS(true);
                playerInputState = PlayerInputState.ThirdPerson;
                break;
            default:
                playerInputState = 0;
                Debug.LogError($"This should not have been hit TogglePlayerInputState() playerInputState is now set to {playerInputState}");
                break;
        }
            

    }

    public void EnableThirdPerson(bool active)
    {
        UnitCharacter.GetComponent<PlayerInput>().enabled = active;
        UnitCharacter.GetComponent<NavMeshAgent>().enabled = !active;
        UnitCharacter.GetComponent<CharacterController>().enabled = active;
        UnitCharacter.GetComponent<ThirdPersonController>().enabled = active;
        ThirdPersonFollower.GetComponent<CinemachineVirtualCamera>().enabled = active;
        ThirdPersonFollower.GetComponent<CinemachineVirtualCamera>().Follow = UnitCameraRoot.transform;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void EnableRTS(bool active)
    {
        RTSCharacter.GetComponent<PlayerInput>().enabled = active;
        RTSFollower.GetComponent<CinemachineVirtualCamera>().enabled = active;
        UnitCharacter.GetComponent<CharacterController>().enabled = !active;
        UnitCharacter.GetComponent<NavMeshAgent>().enabled = active;
        UnitCharacter.GetComponent<ThirdPersonController>().enabled = !active;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
}

public enum PlayerInputState
{
    ThirdPerson,
    Strategy,
}
