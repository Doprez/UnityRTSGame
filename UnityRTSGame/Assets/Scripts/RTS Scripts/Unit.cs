using System;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class Unit : MonoBehaviour
{
    public bool IsSelected;
    public GameObject SelectionSprite;
    public GameObject UnitCameraRoot;
    private Animator _animator;
    public GameObject TargetPickupItem;
    public UnitInventory Inventory;
    public Unit CommandingUnit;

    [Header("NavMesh Options")]
    public NavMeshAgent agent;
    public float FollowCommanderOffsetx;
    public float FollowCommanderOffsetz;

    private bool _hasAnimator;
    private float _animationBlend;
    float inputMagnitude;

    // animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;
    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;
    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;
    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    private void Start()
    {
        AssignAnimationIDs();
        _animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        Inventory = GetComponent<UnitInventory>();
    }

    private void Update()
    {
        AssignAnimationIDs();
        AnimateUnit();
        FollowCommandingUnit();
    }

    private void FixedUpdate()
    {
        CheckForPickUpItem();
    }

    public void FollowCommandingUnit()
    {
        if (CommandingUnit != null)
        {
            if(FollowCommanderOffsetx == 0 || FollowCommanderOffsetz == 0)
            {
                FollowCommanderOffsetx = Random.Range(-10, 10);
                FollowCommanderOffsetz = Random.Range(-10, 10);
            }
            var destination = new Vector3(CommandingUnit.transform.position.x + FollowCommanderOffsetx, CommandingUnit.transform.position.y, CommandingUnit.transform.position.z + FollowCommanderOffsetz);
            GetComponent<NavMeshAgent>().SetDestination(destination);
            
        }
    }

    public void CheckForPickUpItem()
    {
        if(TargetPickupItem != null)
        {
            var distance = TargetPickupItem.transform.position.magnitude - gameObject.transform.position.magnitude;
            Debug.Log("Item TargetPickupItem.transform.position.magnitude: " + distance);
            if(distance < 1f && distance > -1f && TargetPickupItem.TryGetComponent(out GroundItem groundItem))
            {
                Inventory.Inventory.Add(groundItem.item);
                Destroy(TargetPickupItem);
                ClearTargetPickUpObject();
            }
        }
    }

    public void SetTargetPickUpObject(GameObject item)
    {
        TargetPickupItem = item;
    }

    public void ClearTargetPickUpObject()
    {
        TargetPickupItem = null;
    }

    public void AnimateUnit()
    {
        if (!GetComponent<PlayerInput>().enabled)
        {
            inputMagnitude = agent.velocity.magnitude / agent.speed;
            _animationBlend = Mathf.Lerp(_animationBlend, agent.speed, Time.deltaTime * SpeedChangeRate);
            
            _animator.SetFloat(_animIDSpeed, agent.velocity.magnitude);
            _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);

            if (Grounded)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }
            else
            {
                _animator.SetBool(_animIDFreeFall, true);
            }

            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GetComponent<ThirdPersonController>().GroundedRadius, GetComponent<ThirdPersonController>().GroundLayers, QueryTriggerInteraction.Ignore);

            _animator.SetBool(_animIDGrounded, Grounded);
        }
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    public void Deselect()
    {
        IsSelected = false;
        SelectionSprite.SetActive(false);
    }

    public void Select()
    {
        IsSelected = true;
        SelectionSprite.SetActive(true);
    }

    public GameObject GetAssetsCameraRoot()
    {
        return UnitCameraRoot;
    }

}
