using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimationScript : MonoBehaviour
{
    [SerializeField] private Sprite[] frameArray;
    private int currentFrame;
    private float timer;

    public SpriteRenderer forwardSprite;
    public SpriteRenderer backwardSprite;
    public SpriteRenderer rightSprite;
    public SpriteRenderer leftSprite;
    public CharacterController characterController;

    public SpriteRenderer spriteRenderer;

    public float frameRate = 0.1f;

    void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        currentFrame = 0;
    }


    void Update()
    {
        
        
        if (currentFrame >= frameArray.Length)
        {
            currentFrame = 0;
        }
        //Debug.Log(CheckWhichKeysArePressed());
        if (CheckWhichKeysArePressed() == 1)
        {
            PlaySpriteAnimation(currentFrame);
        }
        else if (CheckWhichKeysArePressed() == 2) 
        {
            PlaySpriteAnimation(currentFrame);
        }
        else if (CheckWhichKeysArePressed() == 3)
        {
            PlaySpriteAnimation(currentFrame);
        }
        else if (CheckWhichKeysArePressed() == 4)
        {
            PlaySpriteAnimation(currentFrame);
        }
    }

    void PlaySpriteAnimation(int frame)
    {
        timer += Time.deltaTime;

        if (timer > frameRate)
        {
            timer -= frameRate;
            currentFrame = frame;
            spriteRenderer.sprite = frameArray[frame];

        }
    }

    float CheckWhichDirection()
    {
        float dot = Vector3.Dot(characterController.transform.forward, Vector3.forward);
        if (dot > 0.9) // going forward direction
        {
            return 1;
        }
        else if (dot < -0.9)// going opposite to forward direction
        {
            return 2;
        }
        else
        {
            Vector3 cross = Vector3.Cross(characterController.transform.forward, Vector3.forward);
            // This could be the other way around...never remember which order
            if (cross.y < 0) // going right 
            {
                return 3;
            }
            else // going left 
            {
                return 4;
            }
        }
        
    }

    float CheckWhichKeysArePressed()
    {
        var x = Input.GetAxisRaw("Horizontal");
        var z = Input.GetAxisRaw("Vertical");

        if (x == 1)
        {
            return 1; // right
        }
        else if (x == -1)
        {
            return 2; // left
        }
        else if (z == -1)
        {
            return 3; // down
        }
        else if (z == 1)
        {
            return 4; // up
        }
        else
        {
            return 0; // idle
        }
    }
}
    
