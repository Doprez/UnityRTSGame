using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.AI;
using StarterAssets;

public class RTSInputController : MonoBehaviour
{
    [Header("Character Settings")]
    public float MoveSpeed = 1;
    public float SprintSpeed = 2;
    public float HorizontalSensitivity = 1f;
    public float VerticalSensitivity = 1f;
    public float ScrollSpeed;
    public float MaxCameraHeight;
    public float MinCameraHeight;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;
    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;
    public float BottomClamp;
    public float TopClamp;

    [Header("Selection Options")]
    public RectTransform unitSelectionArea = null;
    public LayerMask layerMask = new LayerMask();
    public RTSHandler rtsHandler;
    public PlayerInputStateHandler inputStateHandler;

    private RaycastHit hit;

    private Vector2 startPosition;
    public List<Unit> SelectedUnits { get; } = new List<Unit>();


    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;
    public GameObject _cameraRoot;

    private RTSInputs _input;
    private CharacterController _controller;
    private GameObject _mainCamera;

    private float _speed;

    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private const float _threshold = 0.01f;
    private float _verticalVelocity;


    // Start is called before the first frame update
    void Start()
    {
        _input = GetComponent<RTSInputs>();
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
        _controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        CameraHeight();
        Selection();
        MoveUnit();
    }

    private void CameraHeight()
    {
        transform.position = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y + (-_input.scroll.y * Time.deltaTime * ScrollSpeed), MinCameraHeight, MaxCameraHeight), transform.position.z);
    }

    private void MoveUnit()
    {
        
        if (Mouse.current.rightButton.IsPressed())
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            Ray ray = _mainCamera.GetComponent<Camera>().ScreenPointToRay(mousePosition);

            if(Physics.Raycast(ray, out hit))
            {
                if (hit.collider.TryGetComponent(out GroundItem groundItem))
                {
                    if (SelectedUnits.Count > 0)
                    {
                        SelectedUnits[0].agent.SetDestination(hit.point);
                        SelectedUnits[0].SetTargetPickUpObject(groundItem.gameObject);
                    }
                }
                else
                {
                    foreach (var unit in SelectedUnits)
                    {
                        if (unit.IsSelected)
                        {
                            unit.agent.SetDestination(hit.point);
                            unit.ClearTargetPickUpObject();
                        }
                    }
                }
            }
        }
    }

    private void LateUpdate()
    {
        RotateCamera();
    }

    private void RotateCamera()
    {
        if (_input.look.sqrMagnitude >= _threshold && _input.EnableCameraRotate)
        {
            _cinemachineTargetYaw += _input.look.x * Time.deltaTime * HorizontalSensitivity;
            _cinemachineTargetPitch += _input.look.y * Time.deltaTime * VerticalSensitivity;
        }

        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);

    }

    private void Move()
	{
        float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

        if (_input.move == Vector2.zero) targetSpeed = 0.0f;

        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

        if (_input.move != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
    }

    #region selection

    private void Selection()
    {
        
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartSelectionArea();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ClearSelectionArea();
        }
        else if (_input.select)
        {
            UpdateSelectionArea();
        }

    }
    
    private void StartSelectionArea()
    {
        if (!_input.sprint)
        {
            foreach (Unit selectedUnit in SelectedUnits)
            {
                selectedUnit?.Deselect();
            }

            SelectedUnits.Clear();
        }
        unitSelectionArea.gameObject.SetActive(true);

        startPosition = Mouse.current.position.ReadValue();

        UpdateSelectionArea();
    }

    private void UpdateSelectionArea()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        float areaWidth = mousePosition.x - startPosition.x;
        float areaHeight = mousePosition.y - startPosition.y;

        unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
        unitSelectionArea.anchoredPosition = startPosition + new Vector2(areaWidth / 2, areaHeight / 2);
    }

    private void ClearSelectionArea()
    {
        unitSelectionArea.gameObject.SetActive(false);

        if (unitSelectionArea.sizeDelta.magnitude == 0)
        {
            Ray ray = _mainCamera.GetComponent<Camera>().ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)) { return; }
            SelectSingleUnit();
            return;
        }

        Vector2 min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta / 2);
        Vector2 max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta / 2);

        foreach (var unit in rtsHandler.GetMyUnits())
        {
            if (SelectedUnits.Contains(unit)) { continue; }
            Vector3 screenPosition = _mainCamera.GetComponent<Camera>().WorldToScreenPoint(unit.transform.position);

            if (screenPosition.x > min.x && screenPosition.x < max.x && screenPosition.y > min.y && screenPosition.y < max.y)
            {
                SelectedUnits.Add(unit);
                unit.Select();
            }
        }

    }

    private void SelectSingleUnit()
    {
        if (hit.collider.TryGetComponent(out Unit unit))
        {

            SelectedUnits.Add(unit);

            foreach (var selectedUnit in SelectedUnits)
            {
                selectedUnit.Select();
                inputStateHandler.UnitCameraRoot = selectedUnit.GetAssetsCameraRoot();
                inputStateHandler.UnitCharacter = selectedUnit.gameObject;
            }
        }

    }
    #endregion

}

