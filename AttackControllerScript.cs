using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;

public class AttackControllerScript : MonoBehaviour
{

    public Canvas HUDCanvas;
    int NumOfConsoles;
    public GameObject Player, consolePrefab;
    public static Dictionary<GameObject, bool> consoles;
    public float fallSpeed;
    public GameObject ScoutDrone, KamikazeDrone, HunterDrone;

    private bool cameraFalling = false;
    
    // Use this for initialization
    void Start()
    {
        RaycastHit hit;

        Vector3 playerLoc = new Vector3(Random.Range(-200f, 200f), 1000, Random.Range(-200f, 200f));
        if (Physics.Raycast(playerLoc, -Vector3.up, out hit))
        {
            playerLoc.y = 1000 - hit.distance;
        }
        Player.transform.position = playerLoc;

        UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController.MovementSettings.ForwardSpeed = 6;
        UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController.MovementSettings.BackwardSpeed = 3;
        UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController.MovementSettings.StrafeSpeed = 3;
        UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController.MovementSettings.RunMultiplier = 1.7f;

        
        NumOfConsoles = (int)Mathf.Clamp((GameController.difficulty * 10f) + (int)Random.Range(-1, 1), 1, 10);
        consoles = new Dictionary<GameObject, bool>();
        GameObject.Find("ConsoleCounter").GetComponent<Text>().text = "0/" + NumOfConsoles;

        for (int i = 0; i < NumOfConsoles; i++)
        {
            Vector2 gridLoc = new Vector2(Random.Range(-200f, 200f), Random.Range(-200f, 200f));
            float height = 5f;
            if (Physics.Raycast(new Vector3(gridLoc.x, 1000, gridLoc.y), -Vector3.up, out hit))
            {
                height = 1000 - hit.distance;
            }
            Vector3 fullLoc = new Vector3(gridLoc.x, height, gridLoc.y);
            GameObject tempObj = (GameObject)Instantiate(consolePrefab, fullLoc, Quaternion.identity);
            tempObj.gameObject.transform.Rotate(new Vector3(Random.Range(-10, 10), Random.Range(-180, 180), Random.Range(-10, 10)));
            tempObj.name = "Console";
            consoles.Add(tempObj, false);

            //spawn Kamikazes or Hunters at consoles
            if (Random.Range(0f, 1f) < GameController.difficulty)
            {
                GameObject tempDrone = (GameObject)Instantiate((Random.value < 0.5f) ? KamikazeDrone : HunterDrone, tempObj.transform.position + new Vector3(1, 0, 1), Quaternion.identity);
                tempDrone.GetComponentInChildren<DroneBehaviour>().target = Player;
                tempDrone.GetComponent<DroneBehaviour>().isGaurd = true;

            }
        }


        //make Scouts
        for (int i = 0; i<Random.Range(80*GameController.difficulty,150*GameController.difficulty); i++)
        {
            //set destinations
            Vector3 point1 = new Vector3(Random.Range(-200f, 200f), 1000, Random.Range(-200f, 200f));
            Vector3 point2 = new Vector3(Random.Range(-200f, 200f), 1000, Random.Range(-200f, 200f));
            if (Physics.Raycast(point1, -Vector3.up, out hit))
            {
                point1.y = 1000 - hit.distance-3;
            }
            
            if (Physics.Raycast(point2, -Vector3.up, out hit))
            {
                point2.y = 1000 - hit.distance-3;
            }

            GameObject tempDrone = (GameObject)Instantiate(ScoutDrone, point2, Quaternion.identity);
            tempDrone.GetComponent<DroneBehaviour>().target = Player;
            tempDrone.GetComponent<ScoutBehaviour>().targetLoc = point1;
            tempDrone.GetComponent<ScoutBehaviour>().secondTargetLoc = point2;
            tempDrone.GetComponent<DroneBehaviour>().isGaurd = false;
        }

        //make Kamikazes
        for (int i = 0; i < Random.Range(18*GameController.difficulty,40*GameController.difficulty); i++)
        {
            Vector3 point1 = new Vector3(Random.Range(-200f, 200f), 1000, Random.Range(-200f, 200f));
            if (Physics.Raycast(point1, -Vector3.up, out hit))
            {
                point1.y= 1000 - hit.distance-3;
            }

            GameObject tempDrone = (GameObject)Instantiate(KamikazeDrone, point1, Quaternion.identity);
            tempDrone.GetComponent<DroneBehaviour>().target = Player;
            tempDrone.GetComponent<DroneBehaviour>().isGaurd = false;
        }

        //make Hunters
        for (int i = 0; i < Random.Range(15 * GameController.difficulty, 35 * GameController.difficulty); i++)
        {
            Vector3 point1 = new Vector3(Random.Range(-200f, 200f), 1000, Random.Range(-200f, 200f));
            if (Physics.Raycast(point1, -Vector3.up, out hit))
            {
                point1.y = 1000 - hit.distance-3;
            }

            GameObject tempDrone = (GameObject)Instantiate(HunterDrone, point1, Quaternion.identity);
            tempDrone.GetComponent<DroneBehaviour>().target = Player;
            tempDrone.GetComponent<DroneBehaviour>().isGaurd = false;
        }

    }

    // Update is called once per frame
    void Update()
    {
        float distance = 250f;
        foreach (GameObject tempConsole in consoles.Keys)
        {
            if (!consoles[tempConsole])
            {
                distance = Mathf.Min(distance, Vector3.Distance(Player.transform.position, tempConsole.transform.position));
            }
        }
        GameObject.Find("CloseSlider").GetComponent<Slider>().value = Mathf.Clamp(Mathf.RoundToInt((200 - distance) / 10), 0, 20);


        if (consoles.Keys.Where(x => !consoles[x]).Count() == 0)
        {
            GameController.playerWon = true;
            GameController.backFromScene = true;
            SceneManager.LoadScene("blank");
            foreach (GameObject item in GameController.holder)
            {
                item.SetActive(true);
            }
        }else if (Player.GetComponent<PlayerManager>().health <= 0) //Death
        {
            cameraFalling = true;
        }
        GetComponent<AudioSource>().volume = (100f - Player.GetComponent<PlayerManager>().health) / 100f;

        if (cameraFalling)
        {
            if (Player.GetComponentInChildren<Camera>().fieldOfView <= 0f)
            {
                cameraFalling = false;
                GameController.playerWon = false;
                GameController.backFromScene = true;
                SceneManager.LoadScene("blank");
                foreach (GameObject item in GameController.holder)
                {
                    item.SetActive(true);
                }
                Destroy(gameObject);
                return;
            }else
            {
                Player.GetComponentInChildren<Camera>().fieldOfView -= fallSpeed;
            }
        }
    }

}
