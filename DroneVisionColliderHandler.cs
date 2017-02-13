using UnityEngine;
using System.Collections;

public class DroneVisionColliderHandler : MonoBehaviour {


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Player")
        {
            gameObject.GetComponentInParent<DroneBehaviour>().canSeePlayer = true;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "Player")
        {
            gameObject.GetComponentInParent<DroneBehaviour>().canSeePlayer = false;
        }
    }
}
