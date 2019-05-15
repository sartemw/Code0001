using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForcingController : MonoBehaviour
{
	Rigidbody2D _rigidbody2D;
	PlayerStats _playerStats;
	ParticleSystem _forcing;
	SignalRIdentity _signalRIdentity;
	int speed;
	float forcingCooldown;

	//Почемуто не хочет работать с этим
	//public KeyCode _keyCode = KeyCode.LeftShift;

	private void Start()
	{
		_forcing = GetComponent<ParticleSystem>();
		_rigidbody2D = GetComponentInParent<Rigidbody2D>();
		_playerStats = GetComponentInParent<PlayerStats>();
		_signalRIdentity = GetComponentInParent<SignalRIdentity>();

		speed = _playerStats.MoveSpeed;
	}

	void Update()
	{
		if (forcingCooldown > 0)
			forcingCooldown -= Time.deltaTime;

		if (Input.GetKeyDown(KeyCode.LeftShift) && forcingCooldown <= 0 && _signalRIdentity.IsAuthority)
		{
			forcingCooldown = 3;
			_forcing.Play(true);
			_rigidbody2D.AddForce(transform.right * speed * _playerStats.Forcing);
		}
	}
}
