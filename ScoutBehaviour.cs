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
        if (droneHealth <= 0) { return; }
        if (canSeePlayer)
        {
            navAgent.Resume();
            Vector3 directionVector = target.transform.position - transform.position;
            navAgent.SetDestination(transform.position+directionVector-keepDistanceValue*directionVector/directionVector.magnitude); //Keep away from player
            transform.LookAt(target.transform.position);
            targetLoc = target.transform.position;

            lastKnownPlayerLoc = target.transform.position;
        }
        else
        {
            if (Vector3.Distance(transform.position, targetLoc) < 10f)
            {
                Vector3 tempLoc = targetLoc;
                targetLoc = secondTargetLoc;
                secondTargetLoc = tempLoc;
            }
            navAgent.SetDestination(targetLoc);

            if (target.GetComponent<PlayerManager>().weaponLight.gameObject.activeInHierarchy && Vector3.Distance(transform.position, target.transform.position)<66)
            {
                transform.LookAt(target.transform.position);
                navAgent.SetDestination(target.transform.position);
            }
            
        }
    }
    void FixedUpdate()
    {
        if (canSeePlayer)
        {
            target.GetComponent<PlayerManager>().damage(0.015f);
        }
    }
}
