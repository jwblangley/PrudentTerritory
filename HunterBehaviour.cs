using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HunterBehaviour : DroneBehaviour {

    UnityEngine.AI.NavMeshAgent navAgent;
    public float gaurdRotateSpeed, keepDistanceValue, shootCooldown;
    public Vector3 targetLoc;
    public bool canShoot=true;
    public AudioClip updatedLocTone, laser;
    private ParticleSystem laserSystem;

    private float spawnChaseRadius = 60;

    // Use this for initialization
    void Start () {
        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        laserSystem = GetComponentInChildren<ParticleSystem>();

        targetLoc = new Vector3(Random.Range(-200f, 200f), 1000, Random.Range(-200f, 200f));
        RaycastHit hit;
        if (Physics.Raycast(targetLoc, -Vector3.up, out hit))
        {
            targetLoc = new Vector3(targetLoc.x, 1000f - hit.distance, targetLoc.z);
        }
    }
	
	// Update is called once per frame
	public override void Update () {
        base.Update();

        if (isGaurd)
        {
            if (canSeePlayer)
            {
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
        else
        {
            navAgent.SetDestination(targetLoc);
            if (lastKnownPlayerLoc.magnitude < 1000f && Vector3.Distance(targetLoc, lastKnownPlayerLoc) > 2 * spawnChaseRadius)
            {//update targetLoc with newLoc
                float tempNum = Random.Range(0f, spawnChaseRadius);
                targetLoc = lastKnownPlayerLoc + new Vector3(tempNum, 0, Mathf.Sqrt(spawnChaseRadius * spawnChaseRadius - tempNum * tempNum));
                RaycastHit hit;
                targetLoc.y = 1000f;
                if (Physics.Raycast(targetLoc, -Vector3.up, out hit))
                {
                    targetLoc = new Vector3(targetLoc.x, 1000 - hit.distance, targetLoc.z);
                    GetComponent<AudioSource>().PlayOneShot(updatedLocTone);
                }
            }

            if (canSeePlayer)
            {
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
            else if (target.GetComponent<PlayerManager>().weaponLight.gameObject.activeInHierarchy && Vector3.Distance(transform.position, target.transform.position) < 66)
            {
                transform.LookAt(target.transform.position);
                navAgent.SetDestination(target.transform.position);
            }
        }
    }

    IEnumerator droneShootDelay()
    {
        yield return new WaitForSeconds(shootCooldown);
        canShoot = true;
    }

}
