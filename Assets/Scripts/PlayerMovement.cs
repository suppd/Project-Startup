#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public CharacterController controller;
    public Transform cursorPos;

    public float speed = 12f;
    public float gravity = -50f;
    public float jumpHeight = 2f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [SerializeField] private Transform debugHitPointTransform;
    [SerializeField] private Transform hookshotTransform;
    private State state;
    private Vector3 hookshotPosition;
    private Vector3 characterMomentumVelocity;
    private float hookshotSize;

   
    Vector3 velocity;
    bool isGrounded;
    Vector2 mousePos;

    private enum State
    {
        Normal,
        HookshotFlying,
        HookshotThrown,
    }
    private void Awake()
    {
        hookshotTransform.gameObject.SetActive(false);
    }

#if ENABLE_INPUT_SYSTEM
    InputAction movement;
    InputAction jump;

    void Start()
    {
        movement = new InputAction("PlayerMovement", binding: "<Gamepad>/leftStick");
        movement.AddCompositeBinding("Dpad")
            .With("Up", "<Keyboard>/w")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/s")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/a")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/d")
            .With("Right", "<Keyboard>/rightArrow");
        
        jump = new InputAction("PlayerJump", binding: "<Gamepad>/a");
        jump.AddBinding("<Keyboard>/space");

        movement.Enable();
        jump.Enable();
    }
#endif

    // Update is called once per frame
    void Update()
    {
       
       
        switch (state)
        {
            default:
            case State.Normal:
                HandleHookshotStart();
                HandleCharacterMovement();
                break;
            case State.HookshotThrown:
                HandleCharacterMovement();
                HandleHookshotThrow();
                break;
            case State.HookshotFlying:
                HandleHookshotMovement();
                break;

        }

        //Debug.Log(characterMomentumVelocity.magnitude);

        //Debug.Log(isGrounded);
        //Debug.Log(velocity.y);
    }

    private void HandleCharacterMovement()
    {
        float x;
        float z;
        bool jumpPressed = false;

#if ENABLE_INPUT_SYSTEM
        var delta = movement.ReadValue<Vector2>();
        x = delta.x;
        z = delta.y;
        jumpPressed = Mathf.Approximately(jump.ReadValue<float>(), 1);
#else
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");
        jumpPressed = Input.GetButtonDown("Jump");
#endif

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        Vector3 move = transform.right * x + transform.forward * z;


        controller.Move(move * speed * Time.deltaTime);

        velocity += characterMomentumVelocity;

        if (jumpPressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        
        velocity.y += gravity * Time.deltaTime;

        

        controller.Move(velocity * Time.deltaTime);



        if (characterMomentumVelocity.magnitude >= 0f)
        {
            float momentumDrag = 100f;
            characterMomentumVelocity -= characterMomentumVelocity * momentumDrag * Time.deltaTime;
            if (characterMomentumVelocity.magnitude < .0f)
            {
                characterMomentumVelocity = Vector3.zero;
            }
        }
    }

    private void ResetGravity()
    {
        velocity.y = -2f;
    }

    private void HandleHookshotStart()
    {

        if (InputDownHookshot())
        {  
            debugHitPointTransform.position = cursorPos.transform.position;
            hookshotPosition = cursorPos.transform.position;
            hookshotSize = 0f;
            hookshotTransform.gameObject.SetActive(true);
            hookshotTransform.localScale = Vector3.zero;
            state = State.HookshotThrown;
        }
    }

    private void HandleHookshotThrow()
    {
        hookshotTransform.LookAt(hookshotPosition);

        float hookshotThrowSpeed = 40f;
        hookshotSize += hookshotThrowSpeed * Time.deltaTime;
        hookshotTransform.localScale = new Vector3(1, 1, hookshotSize);

        if (hookshotSize >= Vector3.Distance(transform.position, hookshotPosition))
        {
            state = State.HookshotFlying;

        }
    }

    private void StopHookshot()
    {
        state = State.Normal;
        ResetGravity();
        hookshotTransform.gameObject.SetActive(false);
        //characterMomentumVelocity = Vector3.zero;
    }

    private void HandleHookshotMovement()
    {
        hookshotTransform.LookAt(hookshotPosition);
        Vector3 hookshotDir = (hookshotPosition - transform.position).normalized;

        float hookshotSpeedMin = 10f;
        float hookshotSpeedMax = 20f;
        float hookshotSpeed = Mathf.Clamp(Vector3.Distance(transform.position, hookshotPosition), hookshotSpeedMin, hookshotSpeedMax);
        float hookshotSpeedMultiplier = 2f;

        controller.Move(hookshotDir * hookshotSpeed * hookshotSpeedMultiplier * Time.deltaTime);

        hookshotSize -= hookshotSpeed * hookshotSpeedMultiplier * Time.deltaTime;
        hookshotTransform.localScale = new Vector3(1, 1, hookshotSize);

        float reachedHookshotPos = 1.5f;
        if (Vector3.Distance(transform.position, hookshotPosition) < reachedHookshotPos)
        {
            StopHookshot();
        }

        if (InputDownHookshot())
        {
            StopHookshot();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            
            float momentumExtraSpeed = 1f;
            characterMomentumVelocity = hookshotDir * hookshotSpeed * momentumExtraSpeed;
            StopHookshot();
            
        }
    }
    
    private bool InputDownHookshot()
    {
        return Input.GetKeyDown(KeyCode.E);
    }



    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward);
    }
}
