using Cinemachine.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerController : MonoBehaviour
{
    PlayerInputAction InputAction;
    public PlayerInputAction playerInputAction => InputAction;
    CharacterController controller;

    /// <summary>
    /// ��ġ����
    /// </summary>
    Vector3 moveDir = Vector3.zero;

    /// <summary>
    /// ���� �̵��ӵ�
    /// </summary>
    float currentSpeed = 0.0f;

    /// <summary>
    /// �ȱ� �ӵ�
    /// </summary>
    float walkingSpeed = 3.0f;

    /// <summary>
    /// �޸��� �ӵ�
    /// </summary>
    float sprintingSpeed = 4.7f;

    /// <summary>
    /// ��ũ���� ����
    /// </summary>
    float crouchDecrease = 1.0f;

    /// <summary>
    /// ��ũ���� üũ
    /// </summary>
    bool crouchChecking = false;

    /// <summary>
    /// ���� ����
    /// </summary>
    float jumpHeight = 4.0f;

    /// <summary>
    /// ���� üũ
    /// </summary>
    bool jumpChecking = false;

    /// <summary>
    /// ���� ���� Ȯ��
    /// </summary>
    float jumpCheckHeight = 0.0f;

    /// <summary>
    /// �߷� ũ��
    /// </summary>
    float gravity = 9.8f;

    /// <summary>
    /// ���� ī��Ʈ(1������ ����)
    /// </summary>
    int jumpCount = 0;

    float Temp = 0.0f;
    Vector3 boxsize = new Vector3(0.25f, 0.125f, 0.25f);
    Vector3 groundCheckPosition;

    private void Start()
    {
        currentSpeed = walkingSpeed;
        Temp = currentSpeed;
    }

    private void Awake()
    {
        InputAction = new PlayerInputAction();

        controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        InputAction.Player.Enable();
        InputAction.Player.Move.performed += OnMove;
        InputAction.Player.Move.canceled += OnMove;
        InputAction.Player.Sprint.performed += OnSprint;
        InputAction.Player.Sprint.canceled += OnSprint;
        InputAction.Player.Jump.performed += OnJump;
        InputAction.Player.Crouch.performed += OnCrouch;
        InputAction.Player.Crouch.canceled += OnCrouch;
    }

    

    private void OnDisable()
    {
        InputAction.Player.Crouch.canceled -= OnCrouch;
        InputAction.Player.Crouch.performed -= OnCrouch;
        InputAction.Player.Jump.performed -= OnJump;
        InputAction.Player.Sprint.canceled -= OnSprint;
        InputAction.Player.Sprint.performed -= OnSprint;
        InputAction.Player.Move.canceled -= OnMove;
        InputAction.Player.Move.performed -= OnMove;
        InputAction.Player.Disable();
    }

    private void Update()
    {
        if (!IsGrounded())
        {
            moveDir.y -= gravity * Time.deltaTime;
        }

        controller.Move(Time.deltaTime * currentSpeed * transform.TransformDirection(new Vector3(moveDir.x, 0.0f, moveDir.z)));
        controller.Move(Time.deltaTime * new Vector3(0.0f, moveDir.y, 0.0f));
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        Vector2 dir = context.ReadValue<Vector2>();

        //�Է¹��� W, A, S, D(Vector2)��ǥ�� x, z��ǥ�� ����
        moveDir.x = dir.x; moveDir.z = dir.y;
    }
    
    private void OnSprint(InputAction.CallbackContext context)
    {
        //��ũ���� ���°� �ƴ� ��
        if(!crouchChecking)
        {
            if (context.performed)
            {
                //���� �̵��ӵ� = �޸��� �ӵ�
                currentSpeed = sprintingSpeed * crouchDecrease;
            }
            else
            {
                //���� �̵��ӵ� = �ȱ� �ӵ�
                currentSpeed = walkingSpeed * crouchDecrease;
            }
        }
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if(jumpCount < 1)
        {
            //y�̵� ���� ���� ���̷� �Ҵ�
            moveDir.y = jumpHeight;

            if(jumpCount == 0)
            {
                //��ǥ ������ ���� ����
                jumpCheckHeight = transform.position.y + controller.radius * 0.3f;
            }
            //���� ���� = true
            jumpChecking = true;
            jumpCount++;
        }
    }
    
    private void OnCrouch(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            crouchDecrease = 0.5f;
            currentSpeed = Temp * crouchDecrease;
            crouchChecking = true;
        }

        else
        {
            crouchDecrease = 1.0f;
            currentSpeed = Temp * crouchDecrease;
            crouchChecking = false;
        }
    }

    private bool IsGrounded()
    {
        //���� ���°� �ƴϰ�, ���� y���̰� ��ǥ y���� ���� ��
        if(jumpChecking && transform.position.y > jumpCheckHeight)
        {
            jumpChecking = false;
        }

        //ĳ���� ������ �ٴ��� üũ�ϴ� ���簢���� ����
        groundCheckPosition = new Vector3(transform.position.x, transform.position.y + controller.radius * -3.0f, transform.position.z);

        //���簢���� ���̾� "Ground"�� ���� ���
        if(Physics.CheckBox(groundCheckPosition, boxsize, Quaternion.identity, LayerMask.GetMask("Ground")))
        {
            if (!jumpChecking)
            {
                if(moveDir.y < jumpHeight)
                {
                    moveDir.y = -0.01f;
                }
                jumpChecking = false;
                jumpCount = 0;
                return true;
            }
        }
        return false;
    
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(groundCheckPosition, boxsize);
    }
#endif
}
