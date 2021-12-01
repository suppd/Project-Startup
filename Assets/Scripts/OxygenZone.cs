using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OxygenZone : MonoBehaviour
{
    public float drain = 0.2f;
    GameObject oxygen;
    HealthBar oxygenbar;
     public void Start()
    {
         oxygen = GameObject.Find("Oxygen");
         oxygenbar = oxygen.GetComponent<HealthBar>();
        
    }
    
   
    private void OnTriggerEnter(Collider other)
    {
        oxygenbar.damage = drain;
        
    }

    private void OnTriggerExit(Collider other)
    {
        oxygenbar.damage = 0.1f;
       
    }
}
