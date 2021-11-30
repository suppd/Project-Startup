using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteLookCamScript : MonoBehaviour
{
    public Camera playerCam;
    void Start()
    {
        
    }

    void Update()
    {
        transform.LookAt(playerCam.transform.position);
    }
}
