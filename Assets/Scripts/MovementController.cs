using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class MovementController : MonoBehaviourPunCallbacks
{
    [SerializeField] float xSensivity = 400f, ySensivity = 200f;
    [SerializeField] float gravity = -9.81f, distanceToGround = .5f;
    [SerializeField] float speed = 12f, jumpHeight = 3f, runningSpeed = 10f;

    [SerializeField] Transform cam = null;
    [SerializeField] LayerMask groundMask;


    FixedTouchField fixedTouchField;
    Vector2 lookInputAxis;
    Joystick joystick;


    float defaultSpeed;
    float xRotation = 0;

    Vector3 velocity;
    CharacterController controller;
    Animator anim;

    private void Awake()
    {
        defaultSpeed = speed;
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
    }
    void Update()
    {
        MouseLook();
        if (IsGrounded() && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        Movement();
        if (CrossPlatformInputManager.GetButtonDown("Jump") && IsGrounded())
        {
            velocity.y = Mathf.Sqrt(jumpHeight * gravity * -2);
        }
        Gravity();

    }
    public void SetJoystickAndTouchField(Joystick joystick, FixedTouchField fixedTouchField)
    {
        this.joystick = joystick;
        this.fixedTouchField = fixedTouchField;

    }
    private void Gravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void Movement()
    {
        float x = joystick.Horizontal;
        float z = joystick.Vertical;
        anim.SetFloat("Horizontal", x);
        anim.SetFloat("Vertical", z);
        if(Mathf.Abs(x) > .9f || Mathf.Abs(z) > .9f)
        {
            speed = runningSpeed;
            anim.SetBool("IsRunning", true);
        }
        else
        {
            speed = defaultSpeed;
            anim.SetBool("IsRunning", false);
        }
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);
    }

    private void MouseLook()
    {
        lookInputAxis = fixedTouchField.TouchDist;
        float mouseX = lookInputAxis.x* Time.deltaTime * xSensivity;
        float mouseY = lookInputAxis.y * Time.deltaTime * ySensivity;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cam.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(mouseX * Vector2.up);
    }

    private bool IsGrounded()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, Vector3.down,out hit, distanceToGround, groundMask);
        return hit.collider;
    }
    

}
