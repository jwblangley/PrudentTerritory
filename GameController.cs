using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public bool traversalOnly;
    public GameObject neutralNode;
    static System.Random rand = new System.Random();

    public float posHalfWidth, posHalfHeight;
    public float pointGenMinBoundary, pointGenRound;
    private static int pointGenCount;
    public int minNodeCount;

    public GameObject arcPrefab;
    public Material blueMaterial, redMaterial, greenMaterial;

    public GameObject background, selectRing;
    public GameObject winnerUI;

    private static Point[] pointArray;
    private static fullArcObject[] arcArray;

    public static GameObject[] NodeArray;

    private static bool cpuReady = false;

    public static bool playerWon = false, CPUWon = false, backFromScene = false;

    public static float difficulty = 0f;

    bool gameOver = false;
    string winner;

    bool isPlayersTurn = true;
    static NodeInstanceHandler currentPlayersNode, currentCPUsNode;
    GameObject blueRing, redRing;

    static AudioSource OST;
    static float audioTime;

    public static GameObject[] holder;


    void Start()
    {
        Application.targetFrameRate = 80;
        background.transform.localEulerAngles = new Vector3(0f, 180 * (rand.Next(0, 2)), 0f);
        generateAllGraph();
        currentPlayersNode = GameObject.Find("Node_BlueRoot").GetComponent<NodeInstanceHandler>();
        foreach (NodeClass node in currentPlayersNode.node.adjacents)
        {
            node.NIH.gameObject.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
        }
        currentCPUsNode = GameObject.Find("Node_RedRoot").GetComponent<NodeInstanceHandler>();
        redRing = (GameObject)Instantiate(selectRing, currentCPUsNode.transform.position, Quaternion.identity);
        redRing.name = "RedRing";
        redRing.GetComponent<Renderer>().material = redMaterial;
        redRing.GetComponent<Rigidbody>().angularVelocity = Random.onUnitSphere;
        blueRing = (GameObject)Instantiate(selectRing, currentPlayersNode.transform.position, Quaternion.identity);
        blueRing.name = "BlueRing";
        blueRing.GetComponent<Renderer>().material = blueMaterial;
        blueRing.GetComponent<Rigidbody>().angularVelocity = Random.onUnitSphere;

        OST = background.GetComponent<AudioSource>();
    }

    public static float difficultyCalc(NodeInstanceHandler hitNode, bool blue, bool traversalOnly=false)
    {
        if (blue && NodeArray.Where(x => !x.GetComponent<NodeInstanceHandler>().node.isBlue && !x.GetComponent<NodeInstanceHandler>().node.isNeutral).ToArray().Count() == 1 /*last red*/ && hitNode == currentCPUsNode)
        {
            return traversalOnly?0.8f: 0.95f;
        }
        else if (!blue && NodeArray.Where(x => x.GetComponent<NodeInstanceHandler>().node.isBlue).ToArray().Count() == 1 /*last blue*/ && hitNode == currentPlayersNode)
        {
            return traversalOnly?0.8f:0.95f;
        }
        else
        {
            float temp = Mathf.Clamp(blue ? (0.5f - hitNode.node.getBlueOrder() / 10f + hitNode.node.getRedOrder() / 10f) : (0.5f - hitNode.node.getRedOrder() / 10f + hitNode.node.getBlueOrder() / 10f), 0.05f, 0.95f);
            if (hitNode == currentCPUsNode || hitNode == currentPlayersNode)
            {
                temp += 0.2f;
            }
            return temp;
        }
    }

    
    void weighting(ref List<NodeClass> adjacents, bool blue)
    {
        foreach (GameObject node in NodeArray)
        {
            node.GetComponent<NodeInstanceHandler>().node.blueWeight = 0;
            node.GetComponent<NodeInstanceHandler>().node.redWeight = 0;
        }
        foreach (NodeClass node in adjacents)
        {
            if (blue)
            {
                node.blueWeight = difficultyCalc(node.NIH, blue);
            }
            else
            {
                node.redWeight = difficultyCalc(node.NIH, blue);
            }
        }
    }
    NodeInstanceHandler hitNode;
    // Update is called once per frame
    void Update()
    {
        if (!gameOver)
        {

            if (isPlayersTurn)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, 100))
                    {
                        hitNode = hit.transform.gameObject.GetComponent<NodeInstanceHandler>();
                        if (currentPlayersNode.node.adjacents.Contains(hitNode.node))
                        {
                            if (traversalOnly)
                            {
                                playerWon = hitNode.node.isBlue|| (float)Random.value > difficultyCalc(hitNode, true, true);
                                backFromScene = true;
                            }
                            else if (!hitNode.node.isBlue)
                            {
                                //Run Battle to see if win
                                audioTime = OST.time;
                                OST.Stop();
                                holder = (GameObject[])Object.FindObjectsOfType(typeof(GameObject));
                                foreach (GameObject item in GameController.holder)
                                {
                                    DontDestroyOnLoad(item);
                                    item.SetActive(false);
                                }
                                difficulty = difficultyCalc(hitNode, true);
                                SceneManager.LoadScene("Attack");
                            }
                            else
                            {
                                playerWon = true;
                                backFromScene = true;
                            }
                        }
                    }
                }

                if (!backFromScene) { return; }
                backFromScene = false;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                //we're back!
                if (!traversalOnly)
                {
                    OST.time = audioTime;
                    OST.Play();
                }
                if (playerWon)
                {
                    //set blue
                    hitNode.setBlue();
                    foreach (fullArcObject arc1 in arcArray)
                    {
                        if (arc1.arcNode.node1.GetComponent<NodeInstanceHandler>().node.isBlue && arc1.arcNode.node2.GetComponent<NodeInstanceHandler>().node.isBlue)
                        {
                            arc1.arcObject.transform.FindChild("Glow").GetComponent<Light>().color = Color.blue;
                            arc1.arcObject.GetComponent<Renderer>().material = blueMaterial;
                        }
                    }


                    currentPlayersNode = hitNode;
                    blueRing.transform.position = currentPlayersNode.transform.position;


                    //set between red and blue to green
                    foreach (fullArcObject arc1 in arcArray)
                    {
                        if ((arc1.arcNode.node1.GetComponent<NodeInstanceHandler>().node.isBlue && !(arc1.arcNode.node2.GetComponent<NodeInstanceHandler>().node.isBlue || arc1.arcNode.node2.GetComponent<NodeInstanceHandler>().node.isNeutral)) || (arc1.arcNode.node2.GetComponent<NodeInstanceHandler>().node.isBlue && !(arc1.arcNode.node1.GetComponent<NodeInstanceHandler>().node.isBlue || arc1.arcNode.node1.GetComponent<NodeInstanceHandler>().node.isNeutral)))
                        {
                            arc1.arcObject.transform.FindChild("Glow").GetComponent<Light>().color = Color.green;
                            arc1.arcObject.GetComponent<Renderer>().material = greenMaterial;
                        }
                    }

                    gameOver = checkWin(out winner);
                    if (!gameOver)
                    {
                        if (hitNode == currentCPUsNode)
                        {
                            currentCPUsNode = NodeClass.nearestAvailable(currentCPUsNode.node, false, false).NIH;
                            redRing.transform.position = currentCPUsNode.transform.position;
                        }
                    }
                    else
                    {
                        winnerUI.GetComponent<Text>().text = winner + "WON!";
                        winnerUI.SetActive(true);
                        Time.timeScale = 0;
                    }
                    
                }
                foreach (GameObject obj in NodeArray)
                {
                    obj.transform.localScale = new Vector3(1, 1, 1);
                }
                isPlayersTurn = false;
                StartCoroutine(waitForCPUTurn(Random.Range(0.8f, 2f)));
            }


            
            //CPU's Turn
            if (cpuReady)
            {
                weighting(ref currentCPUsNode.node.adjacents, false);
                NodeInstanceHandler redHit;

                //CPU CHOOSING

                //If only a few blue left, seek and destroy
                //else easiest non red to go to if adjacent
                if ((float)NodeArray.Where(x => x.GetComponent<NodeInstanceHandler>().node.isBlue).Count() / (float)NodeArray.Count() < 0.25f && (float)NodeArray.Where(x => !x.GetComponent<NodeInstanceHandler>().node.isBlue && !x.GetComponent<NodeInstanceHandler>().node.isNeutral).Count() / (float)NodeArray.Count() > 0.5f)
                {
                    redHit = NodeClass.nextInDjikstras(currentCPUsNode.node, currentPlayersNode.node, false).NIH;
                }
                else
                {
                    try
                    {
                        redHit = currentCPUsNode.node.adjacents.Where(x => x.redWeight != 0 && (x.isBlue || x.isNeutral)).OrderBy(x => x.redWeight).ToArray()[0].NIH;
                    }
                    catch (System.IndexOutOfRangeException) //all adjacent nodes are red
                    {
                        //Djikstra's to nearest
                        redHit = NodeClass.nextInDjikstras(currentCPUsNode.node, NodeClass.nearestAvailable(currentCPUsNode.node, true, true), false).NIH;
                        
                    }
                }

                //put the ring around red hit here and then wait again, before seeing if win
                redRing.transform.position = redHit.gameObject.transform.position;
                if (!Input.GetKey(KeyCode.Space))
                {
                    return;
                }

                //Run Battle to see if Red wins
                CPUWon = (float)Random.value > difficultyCalc(redHit, false, traversalOnly) || (!redHit.node.isBlue && !redHit.node.isNeutral);

                if (CPUWon)
                {
                    //set red
                    redHit.setRed();
                    currentCPUsNode = redHit;
                    redRing.transform.position = currentCPUsNode.transform.position;
                    foreach (fullArcObject arc1 in arcArray)
                    {
                        if (!(arc1.arcNode.node1.GetComponent<NodeInstanceHandler>().node.isBlue || arc1.arcNode.node1.GetComponent<NodeInstanceHandler>().node.isNeutral) && !(arc1.arcNode.node2.GetComponent<NodeInstanceHandler>().node.isBlue || arc1.arcNode.node2.GetComponent<NodeInstanceHandler>().node.isNeutral))
                        {
                            arc1.arcObject.transform.FindChild("Glow").GetComponent<Light>().color = Color.red;
                            arc1.arcObject.GetComponent<Renderer>().material = redMaterial;
                        }
                    }

                    //set between red and blue to green
                    foreach (fullArcObject arc1 in arcArray)
                    {
                        if ((arc1.arcNode.node1.GetComponent<NodeInstanceHandler>().node.isBlue && !(arc1.arcNode.node2.GetComponent<NodeInstanceHandler>().node.isBlue || arc1.arcNode.node2.GetComponent<NodeInstanceHandler>().node.isNeutral)) || (arc1.arcNode.node2.GetComponent<NodeInstanceHandler>().node.isBlue && !(arc1.arcNode.node1.GetComponent<NodeInstanceHandler>().node.isBlue || arc1.arcNode.node1.GetComponent<NodeInstanceHandler>().node.isNeutral)))
                        {
                            arc1.arcObject.transform.FindChild("Glow").GetComponent<Light>().color = Color.green;
                            arc1.arcObject.GetComponent<Renderer>().material = greenMaterial;
                        }
                    }
                    gameOver = checkWin(out winner);
                    if (!gameOver)
                    {
                        if (redHit == currentPlayersNode)
                        {
                            currentPlayersNode = NodeClass.nearestAvailable(currentPlayersNode.node, true, false).NIH;
                            blueRing.transform.position = currentPlayersNode.transform.position;
                        }
                    }
                    else
                    {
                        winnerUI.GetComponent<Text>().text = winner + "WON!";
                        winnerUI.SetActive(true);
                        Time.timeScale = 0;
                    }
                }
                else
                {
                    redRing.transform.position = currentCPUsNode.gameObject.transform.position;
                }

                isPlayersTurn = true;

                foreach (NodeClass node in currentPlayersNode.node.adjacents)
                {
                    node.NIH.gameObject.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
                }
            }
        }
    }
    bool checkWin(out string winner)
    {
        winner = "";
        if (NodeArray.Where(x => x.GetComponent<NodeInstanceHandler>().node.isBlue).ToArray().Count() == 0 /*red win*/ || NodeArray.Where(x => !x.GetComponent<NodeInstanceHandler>().node.isBlue && !x.GetComponent<NodeInstanceHandler>().node.isNeutral).ToArray().Count() == 0 /*blue win*/)
        {
            winner = (NodeArray.Where(x => x.GetComponent<NodeInstanceHandler>().node.isBlue).ToArray().Count() == 0) ? "THE DRONES HAVE " : "HUMANITY HAS ";
            if (winner == "Blue")
            {
                Destroy(GameObject.Find("RedRing"));
            }
            else
            {
                Destroy(GameObject.Find("BlueRing"));
            }
            return true;
        }
        else
        {
            return false;
        }
    }
    void generateAllGraph()
    {
        do
        {
            pointArray = new Point[1000];
            pointGenCount = 0;
            generateCoordinates(new Rect(0, 0, posHalfWidth * 2, posHalfHeight * 2));
            pointArray = pointArray.Where(i => i.x > 0 && i.x < posHalfWidth * 2 && i.y > 0 && i.y < posHalfHeight * 2).ToArray(); //remove any on the border
            pointArray = pointArray.Select(i => new Point(i.x - posHalfWidth, i.y - posHalfHeight)).ToArray(); //map to centre of (0,0)
            pointArray = pointArray.Select(i => new Point(roundToNearest(i.x, pointGenRound), roundToNearest(i.y, pointGenRound))).ToArray(); //remove unecessary precision and delete nodes that are much too close
            pointArray = pointArray.Distinct().ToArray(); //remove duplicates

        } while (pointArray.Length < minNodeCount);

        NodeArray = new GameObject[pointArray.Length + 2];

        GameObject blueRoot = (GameObject)Instantiate(neutralNode, new Vector3(-12.5f, 0, 0), Quaternion.identity);
        blueRoot.name = "Node_BlueRoot";
        blueRoot.GetComponent<NodeInstanceHandler>().setBlue();
        NodeArray[0] = blueRoot;

        GameObject redRoot = (GameObject)Instantiate(neutralNode, new Vector3(12.5f, 0, 0), Quaternion.identity);
        redRoot.name = "Node_RedRoot";
        redRoot.GetComponent<NodeInstanceHandler>().setRed();
        NodeArray[1] = redRoot;

        for (int i = 0; i < NodeArray.Length - 2; i++)
        {
            NodeArray[i + 2] = (GameObject)Instantiate(neutralNode, new Vector3(pointArray[i].x, 0, pointArray[i].y), Quaternion.identity);
            NodeArray[i + 2].name = "Node_" + (i + 1);
        }

        arcNodes[] possibleArcs = new arcNodes[NodeArray.Length * (NodeArray.Length - 1) / 2];
        int counter = 0;
        for (int i = 0; i < NodeArray.Length; i++)
        {
            for (int j = i + 1; j < NodeArray.Length; j++)
            {
                possibleArcs[counter] = new arcNodes(NodeArray[i], NodeArray[j]);
                counter++;
            }
        }
        arcArray = new fullArcObject[NodeArray.Length * (NodeArray.Length - 1) / 2];
        int arcCounter = 0;

        //Minimum spanning tree ensures all nodes are connected and reduces chance of crosses
        //Kruskal's Algorithm for minimum spanning
        possibleArcs = possibleArcs.OrderBy(x => x.physicalLength).ToArray();
        foreach (arcNodes arc in possibleArcs)
        {
            arc.node1.GetComponent<NodeInstanceHandler>().node.connect(arc.node2.GetComponent<NodeInstanceHandler>().node);
            bool isWholeCyclic = false;
            foreach (GameObject tempNode in NodeArray)
            {
                if (NodeClass.isCyclic(tempNode.GetComponent<NodeInstanceHandler>().node, new List<NodeClass>()))
                {
                    isWholeCyclic = true;
                    break;
                }
            }

            if (isWholeCyclic)
            {
                arc.node1.GetComponent<NodeInstanceHandler>().node.disconnect(arc.node2.GetComponent<NodeInstanceHandler>().node);
            }
            else
            {
                GameObject ArcObject = (GameObject)Instantiate(arcPrefab, new Vector3(arc.midPoint.x, 0, arc.midPoint.y), Quaternion.identity);
                ArcObject.name = "Arc_" + arc.node1.name.Split('_')[1] + "-" + arc.node2.name.Split('_')[1];
                LineRenderer tempLine = ArcObject.GetComponent<LineRenderer>();
                tempLine.startWidth = 0.2f;
                tempLine.endWidth = 0.2f;
                Vector3[] tempPoints = new Vector3[2];
                tempPoints[0] = arc.node1.transform.position;
                tempPoints[1] = arc.node2.transform.position;
                tempLine.SetPositions(tempPoints);

                arcArray[arcCounter] = new fullArcObject(arc, ArcObject);
                arcCounter++;
            }
        }

        //Fill out other short arcs
        arcNodes[] remainingArcs = possibleArcs.Where(x => !arcArray.Select(y => y.arcNode).ToArray().Contains(x)).OrderBy(z => z.physicalLength).ToArray();
        if (remainingArcs.Length > 8)
        {
            for (int i = 0; i < Random.Range(5, remainingArcs.Length - 2); i += 2)
            {
                arcNodes arc = remainingArcs[i + Random.Range(0, 1)];

                bool valid = true; //checking if arcs overlap
                Vector3 heading = arc.node2.transform.position - arc.node1.transform.position;
                Vector3 direction = heading / heading.magnitude;
                valid = !Physics.Raycast(arc.node1.transform.position, direction, Vector3.Distance(arc.node1.transform.position, arc.node2.transform.position) - 1f);

                if (valid)
                {
                    GameObject ArcObject = (GameObject)Instantiate(arcPrefab, new Vector3(arc.midPoint.x, 0, arc.midPoint.y), Quaternion.identity);
                    ArcObject.name = "Arc_" + arc.node1.name.Split('_')[1] + "-" + arc.node2.name.Split('_')[1];
                    LineRenderer tempLine = ArcObject.GetComponent<LineRenderer>();
                    tempLine.startWidth = 0.2f;
                    tempLine.endWidth = 0.2f;
                    Vector3[] tempPoints = new Vector3[2];
                    tempPoints[0] = arc.node1.transform.position;
                    tempPoints[1] = arc.node2.transform.position;
                    tempLine.SetPositions(tempPoints);

                    arcArray[arcCounter] = new fullArcObject(arc, ArcObject);
                    arcCounter++;

                    arc.node1.GetComponent<NodeInstanceHandler>().node.connect(arc.node2.GetComponent<NodeInstanceHandler>().node);
                }
            }
        }

        System.Array.Resize(ref arcArray, arcCounter);
    }

    private int counter = 0;
    void generateCoordinates(Rect box)
    {

        if ((box.width - (2f * pointGenMinBoundary)) > 1 && (box.height - (2f * pointGenMinBoundary)) > 1)
        {
            counter++;
            if (!pointArray.Contains(new Point(box.x, box.y)))
            {
                pointArray[pointGenCount] = new Point(box.x, box.y); //top left
                pointGenCount++;
            }
            if (!pointArray.Contains(new Point(box.x + box.width, box.y)))
            {
                pointArray[pointGenCount] = new Point(box.x + box.width, box.y); //top right
                pointGenCount++;
            }
            if (!pointArray.Contains(new Point(box.x, box.y + box.height)))
            {
                pointArray[pointGenCount] = new Point(box.x, box.y + box.height); //bottom left
                pointGenCount++;
            }
            if (!pointArray.Contains(new Point(box.x + box.width, box.y + box.height)))
            {
                pointArray[pointGenCount] = new Point(box.x + box.width, box.y + box.height); //bottom right
                pointGenCount++;
            }

            float xCross = Random.Range(pointGenMinBoundary, box.width - pointGenMinBoundary);
            float yCross = Random.Range(pointGenMinBoundary, box.height - pointGenMinBoundary);

            generateCoordinates(new Rect(box.x, box.y, xCross, yCross)); //Top Left
            generateCoordinates(new Rect(box.x + xCross, box.y, box.width - xCross, yCross));//Top Right
            generateCoordinates(new Rect(box.x, box.y + yCross, xCross, box.height - yCross)); //Bottom Left
            generateCoordinates(new Rect(box.x + xCross, box.y + yCross, box.width - xCross, box.height - yCross)); //Bottom Right
        }
    }
    public float roundToNearest(float input, float to)
    {
        return Mathf.Round(input / to) * to;
    }

    IEnumerator waitForCPUTurn(float seconds)
    {
        cpuReady = false;
        yield return new WaitForSeconds(seconds);
        cpuReady = true;

    }

    public struct Point
    {
        public float x, y;
        public Point(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

    }

    public struct arcNodes
    {
        public float physicalLength;
        public Point midPoint;
        public GameObject node1, node2;
        public arcNodes(GameObject node1, GameObject node2)
        {
            this.node1 = node1;
            this.node2 = node2;
            midPoint = new Point((node1.transform.position.x + node2.transform.position.x) / 2, (node1.transform.position.z + node2.transform.position.z) / 2);
            physicalLength = Mathf.Sqrt(
            Mathf.Pow(node1.transform.position.x - node2.transform.position.x, 2) +
            Mathf.Pow(node1.transform.position.z - node2.transform.position.z, 2)
            );
        }
    }

    public struct fullArcObject
    {
        public arcNodes arcNode;
        public GameObject arcObject;
        public LineRenderer line;

        public fullArcObject(arcNodes arcNode, GameObject arcObject)
        {
            this.arcNode = arcNode;
            this.arcObject = arcObject;
            line = arcObject.GetComponent<LineRenderer>();
        }
    }

}
