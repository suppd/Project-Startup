using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mouse3DPos : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask layermask;

    private void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 10000f, layermask))
        {
            transform.position = raycastHit.point;
        }
    }
}
