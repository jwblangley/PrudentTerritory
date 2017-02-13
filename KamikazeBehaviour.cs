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


        if (Vector3.Distance(transform.position, target.transform.position) < triggerDistance)
        {
            destruct(2);
            return;
        }

        if (isGaurd)
        {
            if (canSeePlayer)
            {
                navAgent.Resume();
                transform.LookAt(target.transform.position);
                navAgent.SetDestination(target.transform.position);
                navAgent.speed = 7f+Mathf.Clamp(8-Vector3.Distance(transform.position,target.transform.position),0f,8f);
            }
            else
            {
                transform.Rotate(new Vector3(0f, gaurdRotateSpeed, 0f));
                navAgent.speed = 3.5f;
            }
        }
        else
        {
            navAgent.SetDestination(targetLoc);
            if (lastKnownPlayerLoc.magnitude<1000f && Vector3.Distance(targetLoc, lastKnownPlayerLoc) > 2*spawnChaseRadius)
            {//update targetLoc with newLoc
                float tempNum = Random.Range(0f, spawnChaseRadius);
                targetLoc=lastKnownPlayerLoc+ new Vector3(tempNum, 0, Mathf.Sqrt(spawnChaseRadius*spawnChaseRadius - tempNum * tempNum));
                RaycastHit hit;
                targetLoc.y = 1000f;
                if (Physics.Raycast(targetLoc, -Vector3.up, out hit))
                {
                    targetLoc = new Vector3(targetLoc.x,1000 - hit.distance,targetLoc.z);
                    GetComponent<AudioSource>().PlayOneShot(updatedLocTone);
                }
            }

            if (canSeePlayer)
            {
                transform.LookAt(target.transform.position);
                navAgent.SetDestination(target.transform.position);
                navAgent.speed = 7f + Mathf.Clamp(8 - Vector3.Distance(transform.position, target.transform.position), 0f, 8f);
                if (Vector3.Distance(transform.position, target.transform.position) < triggerDistance)
                {
                    destruct(2);
                    return;
                }
            }
            else
            {
                navAgent.speed = 3.5f;
            }

            if (target.GetComponent<PlayerManager>().weaponLight.gameObject.activeInHierarchy && Vector3.Distance(transform.position, target.transform.position) < 66)
            {
                transform.LookAt(target.transform.position);
                navAgent.SetDestination(target.transform.position);
            }
        }
    }
}