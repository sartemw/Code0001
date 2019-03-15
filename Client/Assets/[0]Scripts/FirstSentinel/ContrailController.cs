using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContrailController : MonoBehaviour {

	ParticleSystem _contrail;
	PlayerController _playerController;

	float _translation;

	void Start()
	{		
		_contrail = GetComponent<ParticleSystem>();
		_playerController = GetComponentInParent<PlayerController>();
	}

	void Update()
	{
		_translation = _playerController.Translation;

		if (_translation != 0)
			_contrail.Play(true);
		else
			_contrail.Stop(true);
	}
}
