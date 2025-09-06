using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionCheck : MonoBehaviour
{
    public bool isCollision=false;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Placeable"))
        {
            if (other.gameObject != transform.parent.gameObject)
            {
                isCollision = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Placeable"))
        {
            if (other.gameObject != transform.parent.gameObject)
            {
                isCollision = false;
            }
        }
    }
}
