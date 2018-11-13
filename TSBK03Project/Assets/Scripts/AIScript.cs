using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class AIScript : MonoBehaviour {

    int choice;
    int[][] influenceMap;
    const double talkDistance = 5.0;


	// Use this for initialization
	void Start () {
        choice = -1;
        influenceMap = new int[256][];
        for(int i = 0; i < influenceMap.Length; i++)
        {
            influenceMap[i] = new int[256];
            for(int j = 0; j < influenceMap[0].Length; j++)
            {
                influenceMap[i][j] = 0;
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
        DoAction();
        ShareInfluenceMap();


	}

    private void DoAction()
    {

        

    }

    private void ShareInfluenceMap()
    {
        int children = this.transform.parent.childCount;
        for (int i = 0; i < children; i++)
        {
            Transform child = this.transform.parent.GetChild(i);
            if(GetDistance(this.transform, child) <= talkDistance){
                //MergeInfluenceMaps(influenceMap, this.transform.parent.GetChild(i).parent.GetComponent<AIScript>().GetInfluenceMap());
            }
        }
    }

    private void MergeInfluenceMaps(int[][] influenceMap, int[][] v)
    {
        throw new NotImplementedException();
    }

    private int[][] GetInfluenceMap()
    {
        return influenceMap;
    }

    private double GetDistance(Transform transform, Transform child)
    {
        return ((Math.Sqrt(Math.Pow(transform.position.x, 2) + Math.Pow(transform.position.y, 2) + Math.Pow(transform.position.z, 2)) - (Math.Sqrt(Math.Pow(child.position.x, 2) + Math.Pow(child.position.y, 2) + Math.Pow(child.position.z, 2)))));
    }
}
