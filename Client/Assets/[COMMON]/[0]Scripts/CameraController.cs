using System.Collections;
using System.Collections.Generic;
using UnityEngine;


	





public class CameraController : MonoBehaviour {

	public float xMin, xMax, yMin, yMax;

	private GameObject target;
	private Vector3 pos;
	private Transform[] boundaries;

	void Start () {		
		StartCoroutine(FindTarget());

		boundaries = new Transform[SignalRClient.instance.GameHelper.Boundaries.Length];
		boundaries = SignalRClient.instance.GameHelper.Boundaries;

		yMax = boundaries[0].position.y - 10.2f;
		yMin = boundaries[1].position.y + 10.2f;
		xMin = boundaries[2].position.x + 14f;
		xMax = boundaries[3].position.x - 14f;
	}

	void FixedUpdate () {

		if (target == null)
			return;

		pos = new Vector3(target.transform.position.x,
						  target.transform.position.y,
						  target.transform.position.z - 10);

		transform.position = Vector3.Lerp(transform.position, pos, 0.07f);

		transform.position = new Vector3
			(Mathf.Clamp(transform.position.x, xMin, xMax),
			Mathf.Clamp(transform.position.y, yMin, yMax),
			target.transform.position.z - 10);
	}

	IEnumerator FindTarget()
	{
		while (target == null)
		{
			SignalRIdentity[] s = GameObject.FindObjectsOfType<SignalRIdentity>();
			
			foreach (var item in s)
			{
				if (item.GetComponent<SignalRIdentity>().IsAuthority)
					target = item.gameObject;
			}
			yield return new WaitForSeconds(1);
		}		
	}
}
