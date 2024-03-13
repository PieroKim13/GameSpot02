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
    /// 위치변수
    /// </summary>
    Vector3 moveDir = Vector3.zero;

    /// <summary>
    /// 현재 이동속도
    /// </summary>
    float currentSpeed = 0.0f;

    /// <summary>
    /// 걷기 속도
    /// </summary>
    float walkingSpeed = 3.0f;

    /// <summary>
    /// 달리기 속도
    /// </summary>
    float sprintingSpeed = 4.7f;

    /// <summary>
    /// 웅크리기 감소
    /// </summary>
    float crouchDecrease = 1.0f;

    /// <summary>
    /// 웅크리기 체크
    /// </summary>
    bool crouchChecking = false;

    /// <summary>
    /// 점프 높이
    /// </summary>
    float jumpHeight = 4.0f;

    /// <summary>
    /// 점프 체크
    /// </summary>
    bool jumpChecking = false;

    /// <summary>
    /// 점프 높이 확인
    /// </summary>
    float jumpCheckHeight = 0.0f;

    /// <summary>
    /// 중력 크기
    /// </summary>
    float gravity = 9.8f;

    /// <summary>
    /// 점프 카운트(1번으로 제한)
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

        //입력받은 W, A, S, D(Vector2)좌표를 x, z좌표로 지정
        moveDir.x = dir.x; moveDir.z = dir.y;
    }
    
    private void OnSprint(InputAction.CallbackContext context)
    {
        //웅크리기 상태가 아닐 때
        if(!crouchChecking)
        {
            if (context.performed)
            {
                //현재 이동속도 = 달리기 속도
                currentSpeed = sprintingSpeed * crouchDecrease;
            }
            else
            {
                //현재 이동속도 = 걷기 속도
                currentSpeed = walkingSpeed * crouchDecrease;
            }
        }
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if(jumpCount < 1)
        {
            //y이동 값을 점프 높이로 할당
            moveDir.y = jumpHeight;

            if(jumpCount == 0)
            {
                //목표 지점의 점포 높이
                jumpCheckHeight = transform.position.y + controller.radius * 0.3f;
            }
            //점프 상태 = true
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
        //점프 상태가 아니고, 현재 y높이가 목표 y보다 높을 때
        if(jumpChecking && transform.position.y > jumpCheckHeight)
        {
            jumpChecking = false;
        }

        //캐릭터 밑으로 바닥을 체크하는 직사각형을 생성
        groundCheckPosition = new Vector3(transform.position.x, transform.position.y + controller.radius * -3.0f, transform.position.z);

        //직사각형이 레이어 "Ground"에 닿을 경우
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
