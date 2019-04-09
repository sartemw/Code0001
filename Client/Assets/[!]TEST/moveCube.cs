using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveCube : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GetComponent<Rigidbody>().AddForce(Vector3.up*5);
	}
	
	
}
