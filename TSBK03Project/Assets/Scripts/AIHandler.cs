using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIHandler : MonoBehaviour {

    public GameObject prefab;
    GameObject newchild;
    int children = 0;

	// Use this for initialization
	void Start () {
        newchild = Instantiate(prefab,this.transform);
        Quaternion tmp = newchild.transform.rotation;
        tmp.Set(50, 50, 0, 1);
        newchild.transform.rotation = tmp;
        this.transform.position.Set(5, 5, 5);
	}
	
	// Update is called once per frame
	void Update ()
    {
        Vector3 tmp = newchild.transform.position;
        tmp.Set(10+ 10 * Mathf.Sin(Time.time), 0, 0);
        newchild.transform.position = tmp;
        Vector3 pos = this.transform.position;
        pos.Set(5 + Mathf.Sin(Time.time), 0, 5 + Mathf.Cos(Time.time));
        this.transform.position = pos;
    }

    
    
}
