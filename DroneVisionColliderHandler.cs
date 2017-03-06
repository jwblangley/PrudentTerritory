using UnityEngine;
using System.Collections;

public class DroneVisionColliderHandler : MonoBehaviour {

    //Pass information to parent class when player enters/exits the drones 'vision'
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
