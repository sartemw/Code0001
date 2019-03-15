using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundaryController : MonoBehaviour {

	private SignalRIdentity _signalRIdentity;

	private float xMin, xMax, yMin, yMax;
	private bool isStepOutStart = false;

	void Start ()
	{
		_signalRIdentity = GetComponent<SignalRIdentity>();

		xMax = Camera.main.GetComponent<CameraController>().xMax + 14f - 0.5f;
		xMin = Camera.main.GetComponent<CameraController>().xMin - 14f + 0.5f;
		yMin = Camera.main.GetComponent<CameraController>().yMin - 10.2f + 0.5f;
		yMax = Camera.main.GetComponent<CameraController>().yMax + 10.2f - 0.5f;
	}
	
	
	void Update ()
	{
		if (_signalRIdentity.IsAuthority)
		{
			if (transform.position.x > xMax || transform.position.x < xMin || transform.position.y > yMax || transform.position.y < yMin)
			{
				if (!isStepOutStart)
					StartCoroutine("StepOut");
			}
			else
			{
				StopCoroutine("StepOut");
				isStepOutStart = false;
			}
		}
	}

	IEnumerator StepOut()
	{
		isStepOutStart = true;
		yield return new WaitForSeconds(5);
		Destroy(gameObject);
	}	
}
