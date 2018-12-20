using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIHandler : MonoBehaviour {

    public GameObject prefab;
    public int agentsWanted = 5;
    public GameObject wayPointList;
	public bool comLockShort;
	public GameObject comLockShortHolder;
	public bool releaseLock;

    private int children = 0;
    private GameObject newchild;
    private int[] waypoints;
    private int textureChildIndex;
    private bool jumpPressed = false;


    // Use this for initialization
    void Start()
    {
        comLockShort = false;
        waypoints = new int[4];
        textureChildIndex = 2;
        for (int i = 0; i < agentsWanted; i++)
        {
            children++;
            for (int a = 0; a < waypoints.Length; a++)
            {
                waypoints[a] = (a + 4 * i) % wayPointList.transform.childCount;
                //Debug.Log(waypoints[a]);
            }
            newchild = Instantiate(prefab, this.transform);
            Quaternion tmp = newchild.transform.rotation;
            tmp = Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0);
            newchild.transform.rotation = tmp;
            newchild.transform.position = new Vector3(UnityEngine.Random.Range(0, 256) - 128, 1, UnityEngine.Random.Range(0, 256) - 128);
            //Debug.Log(newchild.transform.position);
            for (int j = 0; j < this.waypoints.Length; j++)
            {
                //Debug.Log("j = " + j + ", waypoint children = " + wayPointList.transform.childCount);
                newchild.GetComponent<AIScript>().wayPoints[j] =
                    wayPointList.transform.GetChild(waypoints[j]).gameObject;
            }
        }


    }

	public bool AcquireComLockShort(GameObject holder){
		//Debug.Log ("trying to get lock");
		if (comLockShort) {
			//Debug.Log ("DENIED");
			return false;
		} else {
			//Debug.Log ("got lock");
			comLockShort = true;
			comLockShortHolder = holder;
			return true;
		}
	}

	public bool ReleaseComLockShort(GameObject releaser){
		if (comLockShortHolder == releaser) {
			if (comLockShort) {
				releaseLock = true;
				return true;
			}
			return false;
		} else {
			return false;
		}
	} 
	
	// Update is called once per frame
	void Update ()
    {
		if (releaseLock) {
		
		comLockShort = false;
		comLockShortHolder = null;
		}
        if (!Input.GetButton("Jump") && jumpPressed)
            jumpPressed = false;
        if (Input.GetButton("Jump") && !jumpPressed)
        {
            jumpPressed = true;
            textureChildIndex = (textureChildIndex + 1) % agentsWanted;
            Debug.Log("textureChildIndex = " + textureChildIndex);
        }
        if (textureChildIndex < 2)
            textureChildIndex = 2;
        this.transform.GetChild(textureChildIndex).GetComponent<AIScript>().SetInfMapUpdate();
        /*Vector3 tmp = newchild.transform.position;
        tmp.Set(10+ 10 * Mathf.Sin(Time.time), 0, 0);
        newchild.transform.position = tmp;
        Vector3 pos = this.transform.position;
        pos.Set(5 + Mathf.Sin(Time.time), 0, 5 + Mathf.Cos(Time.time));
        this.transform.position = pos;*/
    }

    internal int GetTextureChild()
    {
        return textureChildIndex;
    }
}
