using UnityEngine;
using System.Collections;

public class LightPulse : MonoBehaviour
{

    // Use this for initialization
    public float maxDist;
    public float speed;
    private float timer = 0.0f;
    private Light glowLight;

    void Start()
    {
        glowLight = GetComponent<Light>();
        glowLight.range = 0;
    }
    //Update is called once per frame
    void Update()
    {
        //Pulse the light
        glowLight.range = Mathf.PingPong(timer * speed, maxDist);
        timer += Time.deltaTime;
    }
}
