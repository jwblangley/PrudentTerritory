using UnityEngine;
using System.Collections;

public class KamikazeBehaviour : DroneBehaviour
{
    UnityEngine.AI.NavMeshAgent navAgent;
    public float gaurdRotateSpeed, triggerDistance;
    public Vector3 targetLoc;
    public AudioClip updatedLocTone;

    private float spawnChaseRadius = 60f;

    void Start()
    {
        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        //Choose a random point to patrol to
        targetLoc = new Vector3(Random.Range(-200f, 200f), 1000, Random.Range(-200f, 200f));
        RaycastHit hit;
        if (Physics.Raycast(targetLoc, -Vector3.up, out hit))
        {
            targetLoc = new Vector3(targetLoc.x, 1000f - hit.distance, targetLoc.z);
        }
    }
    // Update is called once per frame
    public override void Update()
    {
        //NOT CALLING base.Update()
        //Move method to hear to edit;
        if (droneHealth <= 0)
        {
            destruct(2);
            return;
        }
        GetComponentInChildren<Light>().color = canSeePlayer ? Color.red : new Color(0.3f, 0, 0);
        //Above is base.update


        if (Vector3.Distance(transform.position, target.transform.position) < triggerDistance) //Close to Player
        {
            destruct(2); //Self-destruct causing double the damage to the player
            return;
        }

        if (isGaurd)
        {
            if (canSeePlayer)
            {
                //Chase down player whilst accelerating
                navAgent.Resume();
                transform.LookAt(target.transform.position);
                navAgent.SetDestination(target.transform.position);
                navAgent.speed = 7f+Mathf.Clamp(8-Vector3.Distance(transform.position,target.transform.position),0f,8f);
            }
            else
            {
                //Look around
                transform.Rotate(new Vector3(0f, gaurdRotateSpeed, 0f));
                navAgent.speed = 3.5f;
            }
        }
        else //not a gaurd
        {
            navAgent.SetDestination(targetLoc); //move towards chosen target location
            if (lastKnownPlayerLoc.magnitude<1000f && Vector3.Distance(targetLoc, lastKnownPlayerLoc) > 2*spawnChaseRadius) //scoutDrone has seen the player far away from the current targetLoc
            {
                //update targetLoc with newLoc
                float tempNum = Random.Range(0f, spawnChaseRadius);
                targetLoc=lastKnownPlayerLoc+ new Vector3(tempNum, 0, Mathf.Sqrt(spawnChaseRadius*spawnChaseRadius - tempNum * tempNum));
                RaycastHit hit;
                targetLoc.y = 1000f;
                if (Physics.Raycast(targetLoc, -Vector3.up, out hit)) //ensure targetLoc isn't beneath the surface of the terrain
                {
                    targetLoc = new Vector3(targetLoc.x,1000 - hit.distance,targetLoc.z);
                    GetComponent<AudioSource>().PlayOneShot(updatedLocTone); //Play audio to signal being updated
                }
            }

            if (canSeePlayer)
            {
                //Chase down player whilst accelerating
                transform.LookAt(target.transform.position);
                navAgent.SetDestination(target.transform.position);
                navAgent.speed = 7f + Mathf.Clamp(8 - Vector3.Distance(transform.position, target.transform.position), 0f, 8f);
                if (Vector3.Distance(transform.position, target.transform.position) < triggerDistance) //Close to Player
                {
                    destruct(2); //Self-destruct causing double the damage to the player
                    return;
                }
            }
            else
            {
                navAgent.speed = 3.5f; //reset drone speed
            }

            if (target.GetComponent<PlayerManager>().weaponLight.gameObject.activeInHierarchy && Vector3.Distance(transform.position, target.transform.position) < 66) //Player turns on flashlight and can be 'seen'
            {
                transform.LookAt(target.transform.position);
                navAgent.SetDestination(target.transform.position);
            }
        }
    }
}
