using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureScript : MonoBehaviour {


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.Rotate (new Vector3(15,30,45) * Time.deltaTime);
		
	}

	public void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player") {
			GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerScript> ().pickUpTreasure();
			//pS.pickUpTreasure ();
			this.gameObject.transform.parent.gameObject.SetActive(false);
		}
	}
}
