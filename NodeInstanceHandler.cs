using UnityEngine;
using System.Collections;

public class NodeInstanceHandler : MonoBehaviour {
    //Handles the gameobject instance of a node for rendering

    Rigidbody rb;
    public Material neutralMaterial, redMaterial, blueMaterial;
    public NodeClass node;

    public NodeInstanceHandler()
    {
        node = new NodeClass(this);
    }


	void Start () {
        rb = GetComponent<Rigidbody>();
        rb.angularVelocity = Random.onUnitSphere * 3;

	}
	public void setBlue()
    {
        this.transform.Find("Sphere").GetComponent<Renderer>().material =  blueMaterial;
        node.isBlue = true;
        node.isNeutral = false;
    }
    public void setRed()
    {
        this.transform.Find("Sphere").GetComponent<Renderer>().material = redMaterial;
        node.isBlue = false;
        node.isNeutral = false;
    }
}
