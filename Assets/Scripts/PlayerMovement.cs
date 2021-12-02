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
    public HealthBar healtbar;
    public SpriteRenderer charachterSprite;

    public Animator animator;
    public AudioSource augmentAudio;
    public AudioClip augmentAudioClip;

    public float oxygenConsumption = 1;
    public float speed = 12f;
    public float gravity = -50f;
    public float jumpHeight = 2f;
    public float hookshotThrowSpeed = 100;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [SerializeField] private Transform debugHitPointTransform;
    [SerializeField] private Transform hookshotTransform;
    private State state;
    private Vector3 hookshotPosition;
    private Vector3 characterMomentumVelocity;
    private float hookshotSize;
    private float characterVelocityY;


    Vector3 velocity;
    Vector3 currentRotation;
    bool isGrounded;
    bool isHookshotting;
    bool isThrowingHookshot;
    Vector2 mousePos;

    private enum State
    {
        Normal,
        HookshotFlying,
        HookshotThrown,
    }
    private void Awake()
    {
        state = State.Normal;
        hookshotTransform.gameObject.SetActive(false);
        currentRotation = charachterSprite.transform.eulerAngles;
        
    }

    void Update()
    {
        Debug.Log(controller.velocity.magnitude);
        IsRunning();
        IsJumping();
        switch (state)
        {
            default:
            case State.Normal:
                HandleHookshotStart();
                HandleCharacterMovement();
                isHookshotting = false;
                isThrowingHookshot = false;
                break;
            case State.HookshotThrown:
                HandleCharacterMovement();
                HandleHookshotThrow();
                isThrowingHookshot = true;
                break;
            case State.HookshotFlying:
                HandleHookshotMovement();
                isHookshotting = true;
                isThrowingHookshot = false;
                break;

        }

        //Debug.Log(characterMomentumVelocity.magnitude);

        //Debug.Log(isGrounded);
        //Debug.Log(characterMomentumVelocity);
        isFlyingWithGrapple();
        IsThrowingGrapple();
    }

    private void HandleCharacterMovement()
    {

        float x;
        float z;
        bool jumpPressed = false;


        x = Input.GetAxisRaw("Horizontal");
        z = Input.GetAxisRaw("Vertical");
        jumpPressed = Input.GetButtonDown("Jump");

        Vector3 characterVelocity = transform.right * x * speed + transform.forward * z * speed;

        //isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
       

        if (controller.isGrounded)
        {
            characterVelocityY = 0f;
            if (jumpPressed)
            {
                float jumpSpeed = 20;
                characterVelocityY = jumpSpeed;
            }
        }
        //apply gravity
        float gravityDownForce = -60f;
        characterVelocityY += gravityDownForce * Time.deltaTime;

        //apply y vector to move vector
        characterVelocity.y = characterVelocityY;
        //apply momentum

        characterVelocity += characterMomentumVelocity;


        controller.Move(characterVelocity * Time.deltaTime);
        //damp momentum 

        if (characterMomentumVelocity.magnitude >= 0f)
        {
            float momentumDrag = 6;
            characterMomentumVelocity -= characterMomentumVelocity * momentumDrag * Time.deltaTime;
            if (characterMomentumVelocity.magnitude < .0f)
            {
                characterMomentumVelocity = Vector3.zero;
            }
        }

        if (Input.GetKey(KeyCode.A))
        {
            charachterSprite.transform.eulerAngles = new Vector3(currentRotation.x, -180, currentRotation.z);
        }
        else
        {
            charachterSprite.transform.eulerAngles = new Vector3(currentRotation.x, 0, currentRotation.z);
        }

       


    }

    private void ResetGravity()
    {
        characterVelocityY = 0f;
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
            healtbar.TakeDamage(oxygenConsumption);
        }
    }

    private void HandleHookshotThrow()
    {
        //augmentAudio.PlayOneShot(augmentAudioClip);
        hookshotTransform.LookAt(hookshotPosition);
 
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
    }

    private void HandleHookshotMovement()
    {
        hookshotTransform.LookAt(hookshotPosition);
        Vector3 hookshotDir = (hookshotPosition - transform.position).normalized;

        float hookshotSpeedMin = 10f;
        float hookshotSpeedMax = 20f;
        float hookshotSpeed = Mathf.Clamp(Vector3.Distance(transform.position, hookshotPosition), hookshotSpeedMin, hookshotSpeedMax);
        float hookshotSpeedMultiplier = 3f;

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
            float momentumExtraSpeed = 7f;
            characterMomentumVelocity = hookshotDir * hookshotSpeed * momentumExtraSpeed;
            float jumpSpeed = 20;
            characterMomentumVelocity += Vector3.up * jumpSpeed;
            StopHookshot();

        }
    }

    private bool InputDownHookshot()
    {
        return Input.GetKeyDown(KeyCode.E);
    }

    public bool IsRunning()
    {
        if (controller.velocity.magnitude > 0.01)
        {
            animator.SetBool("Is running", true);
            return true;
        }
        else
        {
            animator.SetBool("Is running", false);
        }
        return false;
    }

    public bool IsJumping()
    {
        if (!controller.isGrounded)
        {
            animator.SetBool("isJumping", true);
            return true;
        }
        else
        {
            animator.SetBool("isJumping", false);
        }
        return false;
    }

    public void IsThrowingGrapple()
    {
        if (isThrowingHookshot)
        {
            animator.SetBool("isThrowingHookshot", true);
        }
        else
        {
            animator.SetBool("isThrowingHookshot", false);
        }
    }
    public void isFlyingWithGrapple()
    {
        if (isHookshotting)
        {
            animator.SetBool("isHookshotting", true);
        }
        else
        {
            animator.SetBool("isHookshotting", false);
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward);
    }
}
