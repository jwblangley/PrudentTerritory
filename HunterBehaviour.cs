using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HunterBehaviour : DroneBehaviour {
    //Inherits from DroneBehaviour

    UnityEngine.AI.NavMeshAgent navAgent;
    public float gaurdRotateSpeed, keepDistanceValue, shootCooldown;
    public Vector3 targetLoc;
    bool canShoot = true;
    public AudioClip updatedLocTone, laser;
    private ParticleSystem laserSystem;

    private float spawnChaseRadius = 60;

    // Use this for initialization
    void Start () {
        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        laserSystem = GetComponentInChildren<ParticleSystem>();

        //Choose a random point to patrol to
        targetLoc = new Vector3(Random.Range(-200f, 200f), 1000, Random.Range(-200f, 200f));
        RaycastHit hit;
        if (Physics.Raycast(targetLoc, -Vector3.up, out hit))
        {
            targetLoc = new Vector3(targetLoc.x, 1000f - hit.distance, targetLoc.z);
        }
    }

	// Update is called once per frame
	public override void Update () {
        base.Update(); //parent class Update()

        if (isGaurd)
        {
            if (canSeePlayer)
            {
                //Follow player at a distance and shoot if possible
                navAgent.Resume();
                transform.LookAt(target.transform.position);
                Vector3 directionVector = target.transform.position - transform.position;
                navAgent.SetDestination(transform.position + directionVector - keepDistanceValue * directionVector / directionVector.magnitude); //Keep away from player

                if (canShoot)
                {
                    canShoot = false;
                    laserSystem.Play();
                    GetComponent<AudioSource>().PlayOneShot(laser);
                    StartCoroutine(droneShootDelay());
                }
            }
            else
            {
                transform.Rotate(new Vector3(0f, gaurdRotateSpeed, 0f));
            }
        }
        else //not a gaurd
        {
            navAgent.SetDestination(targetLoc); //move towards chosen target location
            if (lastKnownPlayerLoc.magnitude < 1000f && Vector3.Distance(targetLoc, lastKnownPlayerLoc) > 2 * spawnChaseRadius) //scoutDrone has seen the player far away from the current targetLoc
            {
                //update targetLoc with newLoc
                float tempNum = Random.Range(0f, spawnChaseRadius);
                targetLoc = lastKnownPlayerLoc + new Vector3(tempNum, 0, Mathf.Sqrt(spawnChaseRadius * spawnChaseRadius - tempNum * tempNum));
                RaycastHit hit;
                targetLoc.y = 1000f;
                if (Physics.Raycast(targetLoc, -Vector3.up, out hit)) //ensure targetLoc isn't beneath the surface of the terrain
                {
                    targetLoc = new Vector3(targetLoc.x, 1000 - hit.distance, targetLoc.z);
                    GetComponent<AudioSource>().PlayOneShot(updatedLocTone); //Play audio to signal being updated
                }
            }

            if (canSeePlayer)
            {
                //Follow player at a distance and shoot if possible
                transform.LookAt(target.transform.position);
                Vector3 directionVector = target.transform.position - transform.position;
                navAgent.SetDestination(transform.position + directionVector - keepDistanceValue * directionVector / directionVector.magnitude); //Keep away from player

                if (canShoot)
                {
                    canShoot = false;
                    laserSystem.Play();
                    GetComponent<AudioSource>().PlayOneShot(laser);
                    StartCoroutine(droneShootDelay());
                }
            }
            else if (target.GetComponent<PlayerManager>().weaponLight.gameObject.activeInHierarchy && Vector3.Distance(transform.position, target.transform.position) < 66) //Player turns on flashlight and can be 'seen'
            {
                transform.LookAt(target.transform.position);
                navAgent.SetDestination(target.transform.position);
            }
        }
    }

    IEnumerator droneShootDelay()
    {
        //Pause some time before next shot without freezing game
        yield return new WaitForSeconds(shootCooldown);
        canShoot = true;
    }

}
