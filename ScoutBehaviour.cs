using UnityEngine;
using System.Collections;

public class ScoutBehaviour : DroneBehaviour
{
    UnityEngine.AI.NavMeshAgent navAgent;
    public float keepDistanceValue;
    public Vector3 targetLoc, secondTargetLoc;

    void Start()
    {
        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }
    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if (droneHealth <= 0) { return; } //Drone is dead - handled by superclass
        if (canSeePlayer)
        {
            navAgent.Resume();
            Vector3 directionVector = target.transform.position - transform.position;
            navAgent.SetDestination(transform.position+directionVector-keepDistanceValue*directionVector/directionVector.magnitude); //Keep set distance away from player
            transform.LookAt(target.transform.position);
            targetLoc = target.transform.position;

            //Update public drone knowledge of the Player's location
            lastKnownPlayerLoc = target.transform.position;
        }
        else
        {
            //Patrol Between two points
            if (Vector3.Distance(transform.position, targetLoc) < 10f) //close to targetLoc ('close to' instead of 'at' since some locatios may be unreachable due to other objects)
            {
                //Switch targetLoc and secondTargetLoc
                Vector3 tempLoc = targetLoc;
                targetLoc = secondTargetLoc;
                secondTargetLoc = tempLoc;
            }
            navAgent.SetDestination(targetLoc);

            if (target.GetComponent<PlayerManager>().weaponLight.gameObject.activeInHierarchy && Vector3.Distance(transform.position, target.transform.position)<66) //Player turns on flashlight and can be 'seen'
            {
                transform.LookAt(target.transform.position);
                navAgent.SetDestination(target.transform.position);
            }

        }
    }
    void FixedUpdate()
    {
        //Does not run when paused
        if (canSeePlayer)
        {
            target.GetComponent<PlayerManager>().damage(0.015f);
        }
    }
}
