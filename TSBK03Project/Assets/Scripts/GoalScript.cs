using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalScript : MonoBehaviour {

	public GameObject[] treasureList;
	public GameObject AIList;


	// Use this for initialization
	void Start () {
		treasureList = GameObject.FindGameObjectsWithTag ("Treasure");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player") {
			if (GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerScript> ().treasureCount >= 10) {
				//WIN
				//this.gameObject.SetActive (false);
				GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerScript> ().treasureCount = 0;
				for (int i = 0; i < treasureList.Length; i++) {
					if (!treasureList [i].gameObject.activeSelf)
						treasureList [i].gameObject.SetActive (true);
				}
				for (int i = 0; i < AIList.transform.childCount; i++) {
					GameObject ai = AIList.transform.GetChild (i).gameObject;
					ai.GetComponent<AIScript>().SetSpeed (0.0f);
				}
			}
		}
	}
}
