using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    void Start()
    {
        
    }

    public Camera cam;
    public CharacterController controller;

    public int DistanceAway = 10;
    void Update()
    { 
        Vector3 PlayerPOS = controller.transform.transform.position;
        cam.transform.position = new Vector3(PlayerPOS.x + cam.transform.position.x, PlayerPOS.y + cam.transform.position.y, PlayerPOS.z + cam.transform.position.z);
    }
}
