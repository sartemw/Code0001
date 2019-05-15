using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstSentinelAnimationsController : MonoBehaviour {

	Animator anim;
	float _rotation;	

	void Start () {
		anim = GetComponent<Animator>();
		_rotation = GetComponent<PlayerController>().Rotation;
	}
	
	
	void Update () {
		if (_rotation < 0) anim.Play("FirstSentinelLeft");
		if (_rotation == 0) anim.Play("FirstSentinelIdle");
		if (_rotation > 0) anim.Play("FirstSentielRight");		
	}
}
