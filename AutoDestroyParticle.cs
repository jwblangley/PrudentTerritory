using UnityEngine;
using System.Collections;

public class AutoDestroyParticle : MonoBehaviour {

    public AudioClip[] explosionClips;
    ParticleSystem ps;
    AudioSource AS;

    void Start () {
        //Play a random explosion audio clip
        ps = GetComponent<ParticleSystem>();
        AS = GetComponent<AudioSource>();
        AS.clip = explosionClips[Random.Range(0, explosionClips.Length)];
        AS.Play();
	}

	// Update is called once per frame
	void Update () {
        //Destroys the current gameobject after the explosion particles have all faded
        if (!ps.IsAlive() && !AS.isPlaying)
        {
            Destroy(gameObject);
        }
	}
}
