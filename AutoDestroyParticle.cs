using UnityEngine;
using System.Collections;

public class AutoDestroyParticle : MonoBehaviour {

    // Use this for initialization
    public AudioClip[] explosionClips;
    ParticleSystem ps;
    AudioSource AS;

    void Start () {
        ps = GetComponent<ParticleSystem>();
        AS = GetComponent<AudioSource>();
        AS.clip = explosionClips[Random.Range(0, explosionClips.Length)];
        AS.Play();
	}
	
	// Update is called once per frame
	void Update () {
        if (!ps.IsAlive() && !AS.isPlaying)
        {
            Destroy(gameObject);
        }
	}
}
