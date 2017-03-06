using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour {

    private static GameObject activate=null;
    public Light weaponLight;
    public ParticleSystem laserProducer;
    public GameObject coolDown, ouch, jetPackUI, pauseUI;
    public float shootCooldown, jetPackCooldown, jetPackPeriod, jetPackPower;
    public AudioClip jetPackClip;
    private bool canShoot = true, canJet = true, isJet = false, zoom = false;
    AudioSource AS;


    public float health = 100;

    void Start () {
        Cursor.visible = false;
        ouch.GetComponent<Image>().canvasRenderer.SetAlpha(0.01f);
        coolDown.GetComponent<Image>().canvasRenderer.SetAlpha(0.01f);
        AS = GetComponent<AudioSource>();
    }

	// Update is called once per frame
	void Update () {
        //Input handling


        if (Input.GetButtonDown("Interact") && Time.timeScale != 0 /*not paused*/)
        {
            if (activate != null && !AttackControllerScript.consoles[activate]) //Nearby console needs activating
            {
                damage(-30f); //heal
                //Activate Console
                AttackControllerScript.consoles[activate] = true;
                activate.GetComponentInChildren<Light>().color = new Color(0, 128, 0);
                activate.GetComponentInChildren<Light>().intensity = 0.2f;
                activate.GetComponentInChildren<Light>().range = 2f;
                activate.GetComponent<AudioSource>().Play();
                GameObject.Find("ConsoleCounter").GetComponent<Text>().text = int.Parse(GameObject.Find("ConsoleCounter").GetComponent<Text>().text[0].ToString())+1 + "/" + AttackControllerScript.consoles.Count();

            }
        }if (Input.GetKeyDown(KeyCode.F) && Time.timeScale!=0)
        {
            weaponLight.gameObject.SetActive(!weaponLight.gameObject.activeSelf); //Illuminate flashlight
        }
        if (Input.GetKeyDown(KeyCode.Q) && Time.timeScale != 0)
        {
            if (canJet)
            {
                //Use jetpack and disable further jetpacks until jetpack cooldown completed
                jetPackUI.GetComponentInChildren<Image>().canvasRenderer.SetAlpha(0f);
                AS.PlayOneShot(jetPackClip);
                StartCoroutine(jetPackDelay());
                canJet = false;
            }
        }
        if (Input.GetButtonDown("Fire") && canShoot && Time.timeScale != 0)
        {
            coolDown.GetComponent<Image>().canvasRenderer.SetAlpha(1f); //Show shoot cooldown
            canShoot = false; //disable shooting until cooldown completed
            StartCoroutine(shootDelay()); //begin shoot cooldown
            laserProducer.Play(); //fire the weapon
            //Change the pitch of the weapon to avoid monotony
            if (Random.value < 0.5)
            {
                AS.pitch += 0.1f;
            }
            else
            {
                AS.pitch -= 0.1f;
            }
            AS.pitch = Mathf.Clamp(AS.pitch, 1f, 2f);
            AS.Play();

        }
        if (Input.GetButtonDown("Zoom") && Time.timeScale != 0)
        {
            zoom = !zoom;
            GetComponentInChildren<Camera>().fieldOfView = zoom ? 50 : 90;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            //Pause
            Time.timeScale = Time.timeScale == 0 ? 1 : 0;
            pauseUI.SetActive(Time.timeScale == 0);
        }

    }

    void FixedUpdate()
    {
        if (isJet)
        {
            GetComponent<Rigidbody>().AddForce(jetPackPower * Vector3.up);
        }
    }

    void OnParticleCollision(GameObject other)
    {
        if (other.name != "Laser") //not friendly fire, but any other laser
        {
            damage(1);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        //Slow down on enterring water
        if (other.gameObject.name == "Water4Simple")
        {
            UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController.MovementSettings.ForwardSpeed /= 3;
            UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController.MovementSettings.BackwardSpeed /= 3;
            UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController.MovementSettings.StrafeSpeed /= 3;
        }else if (other.gameObject.name == "Console")
        {
            //Set the activate variable to the nearby console
            activate = other.gameObject;
        }
    }
    void OnTriggerExit(Collider other)
    {
        //Return to normal speed when exiting water
        if (other.gameObject.name == "Water4Simple")
        {
            UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController.MovementSettings.ForwardSpeed *= 3;
            UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController.MovementSettings.BackwardSpeed *= 3;
            UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController.MovementSettings.StrafeSpeed *= 3;
        }
        else if (other.gameObject.name == "Console")
        {
            //The console is out of range again so cannot be activated
            activate = null;
        }
    }

    //Delays without freezing the game
    IEnumerator shootDelay()
    {
        coolDown.GetComponent<Image>().CrossFadeAlpha(0.01f, shootCooldown, false);
        yield return new WaitForSeconds(shootCooldown);
        canShoot = true;
    }

    IEnumerator jetPackDelay()
    {
        isJet = true;
        StartCoroutine(jetPackBurst());

        jetPackUI.GetComponentInChildren<Image>().CrossFadeAlpha(1f, jetPackCooldown, false);
        yield return new WaitForSeconds(jetPackCooldown-jetPackPeriod);
        canJet = true;
    }
    IEnumerator jetPackBurst()
    {
        yield return new WaitForSeconds(jetPackPeriod);
        isJet = false;
    }
    public void damage(float damageValue)
    {
        //decrease health and affect HUD
        health -= damageValue;
        health = Mathf.Clamp(health, -20f, 100f); //stops heal going too far
        ouch.GetComponent<Image>().CrossFadeAlpha( 1- health / 100f, 0.1f, false);
    }
}
