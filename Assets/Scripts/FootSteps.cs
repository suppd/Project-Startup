using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootSteps : MonoBehaviour
{
    public CharacterController controller;
    public AudioSource footstep1;
    public AudioSource footstep2;
    public AudioSource footstep3;

    void Start()
    {
        
    }

    void Update()
    {
        if (controller.isGrounded && controller.velocity.magnitude > 2f && !footstep1.isPlaying)
        {
            footstep1.Play();
        }
        else if (controller.velocity.magnitude < 0.5f)
        {
            footstep1.Stop();
        }
    }

    public float GetRandom()
    {
        return Random.Range(1, 3);
    }
    
}
