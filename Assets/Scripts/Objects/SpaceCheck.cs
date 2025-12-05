using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceCheck : MonoBehaviour
{
    void OnTriggerEnter(Collider col)
    {
        Debug.Log("Hit");
    }
}
