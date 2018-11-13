using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIHandler : MonoBehaviour {

    public GameObject prefab;
    GameObject newchild;

	// Use this for initialization
	void Start () {
        //newchild = Instantiate(prefab,this.transform);
        //newchild.transform.rotation.Set(50, 50, 0,1);
	}
	
	// Update is called once per frame
	void Update ()
    {
        //newchild.transform.position.Set(10+ 10 * Mathf.Sin(Time.time), 0, 0);
        //Debug.Log("child x set to " + (10 + Mathf.Sin(Time.time)));

    }

    
    
}
