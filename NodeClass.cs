using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class NodeClass{

    public List<NodeClass> adjacents;
    public float blueWeight, redWeight;
    public bool isBlue=false, isNeutral=true;
    public NodeInstanceHandler NIH;

	public NodeClass(NodeInstanceHandler InstanceHandler)
    {
        NIH = InstanceHandler;
        adjacents = new List<NodeClass>();

    }

    public int getOrder()
    {
        return adjacents.Count;
    }

    public int getBlueOrder()
    {
        return adjacents.Where(x => x.isBlue).Count();
    }

    public int getRedOrder()
    {
        return adjacents.Where(x => !x.isBlue&& !x.isNeutral).Count();
    }

    public void connect(NodeClass otherNode)
    {
        adjacents.Add(otherNode);
        otherNode.adjacents.Add(this);
        adjacents = adjacents.Distinct().ToList();
        otherNode.adjacents = otherNode.adjacents.Distinct().ToList();
    }

    public void disconnect(NodeClass otherNode)
    {
        adjacents.Remove(otherNode);
        otherNode.adjacents.Remove(this);
        adjacents = adjacents.Distinct().ToList();
        otherNode.adjacents = otherNode.adjacents.Distinct().ToList();
    }

    public static bool isCyclic(NodeClass from, List<NodeClass> path)
    {
        if (path.Contains(from))
        {
            return true;
        }

        List<NodeClass> extendedPath = new List<NodeClass>();
        extendedPath.AddRange(path);
        extendedPath.Add(from);


        foreach(NodeClass node in from.adjacents)
        {
            node.adjacents.Remove(from); //do not use disconnect and connect as that affects from as well as node and therefore the flow of the loop
            if(isCyclic(node, extendedPath))
            {
                node.adjacents.Add(from);
                return true;
            }
            node.adjacents.Add(from);
        }

        return false;
    }

    public static NodeClass nearestAvailable(NodeClass from, bool blue, bool orNeutral)

        //Do not run if no remaining nodes
        //Validate this beforehand
        //breadth first search
    {
        Queue<NodeClass> q = new Queue<NodeClass>();
        Dictionary<NodeClass, bool> visited = new Dictionary<NodeClass, bool>();

        foreach (GameObject node in GameController.NodeArray)
        {
            visited.Add(node.GetComponent<NodeInstanceHandler>().node, false);
        }
        visited[from] = true;
        q.Enqueue(from);
        while (q.Any())
        {
            NodeClass node = q.Dequeue();
            visited[node] = true;
            if (blue)
            {
                if (node.isBlue || (orNeutral&&node.isNeutral))
                {
                    return node;
                }
                else
                {
                    foreach(NodeClass child in node.adjacents)
                    {
                        if (!visited[child])
                        {
                            q.Enqueue(child);
                        }
                    }
                }
            }
            else
            {
                if ((!node.isBlue && !node.isNeutral) /*red*/ || (orNeutral && node.isNeutral))
                {
                    return node;
                }
                else
                {
                    foreach (NodeClass child in node.adjacents)
                    {
                        if (!visited[child])
                        {
                            q.Enqueue(child);
                        }
                    }
                }
            }
        }
        return null;//should not be reached
    }

    struct DjikstraBox{
        public float workingVal;
        public bool isFixed;
        public DjikstraBox(float workingVal, bool isFixed)
        {
            this.workingVal = workingVal;
            this.isFixed = isFixed;
        }
    }
    public static NodeClass nextInDjikstras(NodeClass from, NodeClass to, bool blue)
    {
        //setup
        foreach(GameObject obj in GameController.NodeArray)
        {
            if (blue)
            {
                if (obj.GetComponent<NodeInstanceHandler>().node.isBlue)
                {
                    obj.GetComponent<NodeInstanceHandler>().node.blueWeight = 0.1f; //a move is worth a very easy fight for time consumption
                }
                else
                {
                    obj.GetComponent<NodeInstanceHandler>().node.blueWeight = GameController.difficultyCalc(obj.GetComponent<NodeInstanceHandler>(), blue);
                }
            }
            else
            {
                if (!obj.GetComponent<NodeInstanceHandler>().node.isBlue && !obj.GetComponent<NodeInstanceHandler>().node.isNeutral)
                {
                    obj.GetComponent<NodeInstanceHandler>().node.redWeight = 0.1f;
                }
                else
                {
                    obj.GetComponent<NodeInstanceHandler>().node.redWeight = GameController.difficultyCalc(obj.GetComponent<NodeInstanceHandler>(), blue);
                }
            }
        }
        Dictionary<NodeClass, DjikstraBox> values = new Dictionary<NodeClass, DjikstraBox>();
        foreach (GameObject tempObj in GameController.NodeArray)
        {
            values.Add(tempObj.GetComponent<NodeInstanceHandler>().node, new DjikstraBox(99, false));
        }

        //calculate
        values[from] = new DjikstraBox(0, true);
        NodeClass lastAdded = from;
        while (!values[to].isFixed)
        {
            foreach (NodeClass node in lastAdded.adjacents)
            {
                if (!values[node].isFixed)
                {
                    values[node] = new DjikstraBox(Mathf.Min(values[lastAdded].workingVal + (blue ? node.blueWeight : node.redWeight), values[node].workingVal), false);
                }
            }
            lastAdded = GameController.NodeArray.Where(x => !values[x.GetComponent<NodeInstanceHandler>().node].isFixed).OrderBy(x => values[x.GetComponent<NodeInstanceHandler>().node].workingVal).ToArray()[0].GetComponent<NodeInstanceHandler>().node;
            values[lastAdded] = new DjikstraBox(values[lastAdded].workingVal, true);
        }

        //retrace
        NodeClass traceFrom;
        NodeClass traceNext = to;
        do
        {
            traceFrom = traceNext;
            traceNext = traceFrom.adjacents.Where(x => values[traceFrom].isFixed && Mathf.Abs(values[traceFrom].workingVal - (blue ? (traceFrom.blueWeight) : (traceFrom.redWeight)) - values[x].workingVal)<0.001f/*due to float rounding error*/).ToArray()[0];
        } while (traceNext.NIH.gameObject.name!=from.NIH.gameObject.name);
        return traceFrom;
    }
}
