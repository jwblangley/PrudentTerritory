using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class DroneBehaviour : MonoBehaviour {
    public GameObject target;
    public float droneHealth;
    public GameObject explosionPrefab;
    public bool canSeePlayer = false, isGaurd;
    public static Vector3 lastKnownPlayerLoc=new Vector3(9999,9999,9999);
    
    // Update is called once per frame
    public virtual void Update () {
        if (droneHealth <= 0)
        {
            destruct(1);
            return;
        }
        GetComponentInChildren<Light>().color = canSeePlayer ? Color.red : new Color(0.3f,0,0);
	}

    void OnParticleCollision(GameObject other)
    {
        if (other.name != "RedLaser")
        {
            transform.LookAt(target.transform.position);
            droneHealth--;
        }
    }

    public void destruct(int damageMultiplier)
    {
        GameObject explosion = (GameObject)Instantiate(explosionPrefab, gameObject.transform.position, Quaternion.identity);
        explosion.transform.localScale = new Vector3(damageMultiplier, damageMultiplier, damageMultiplier);
        target.GetComponent<PlayerManager>().damage(5 * damageMultiplier * (int)Mathf.Clamp(8 - Vector3.Distance(target.transform.position, transform.position), 0, 8));
        Destroy(gameObject);
        return;
    }

    
    
}
