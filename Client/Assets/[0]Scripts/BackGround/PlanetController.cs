using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetController : MonoBehaviour {

	public float _speed;

	public float _rotationSpeed;

	public Vector2 _direction;
	

	void FixedUpdate () {
		transform.Translate(_direction.x * _speed * 0.001f , _direction.y * _speed * 0.001f, 0);
		transform.Rotate(0, 0, _rotationSpeed * 0.001f);
	}
}
