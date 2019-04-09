using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGBulletSpawner : MonoBehaviour {

	ObjectPooler objectPooler;

	private void Start()
	{
		objectPooler = ObjectPooler.Instance;
	}

	private void FixedUpdate()
	{
		objectPooler.SpawnFromPool("MGBullet", transform.position, Quaternion.identity);
	}
}
