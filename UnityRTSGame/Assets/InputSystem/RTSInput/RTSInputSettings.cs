using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class RTSInputSettings : MonoBehaviour
{
    [Header("Available RTS controls")]
    [Header("Camera controls")]
    [Tooltip("Button that is pressed to activate camera rotation")]
    public  ButtonControl RTSRotationButton;

    [Header("MovementControls")]
    public  KeyControl Forward;
    public  KeyControl Left;
    public  KeyControl Right;
    public  KeyControl Back;

}