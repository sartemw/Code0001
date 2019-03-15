using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour {
	[Header("Скорость сети")]
	int SyncRate = 3;

	int speed, rotation, damage ;

	Vector3 _syncPosition;
	public Vector3 SyncPosition
	{
		get { return _syncPosition; }
		set { _syncPosition = value; }
	}
	Quaternion _syncRotation;
	public Quaternion SyncRotation
	{
		get { return _syncRotation; }
		set { _syncRotation = value; }
	}

	Rigidbody2D _rigidbody2D;
	BulletStats _bulletStats;
	private void Start()
	{
		_rigidbody2D = GetComponent<Rigidbody2D>();
		_bulletStats = GetComponent<BulletStats>();

		speed = _bulletStats.BulletSpeed*10;
		damage = _bulletStats.Damage;
		Destroy(gameObject, _bulletStats.LifeTime);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		
		if (GetComponent<SignalRIdentity>().IsAuthority
			&& collision.GetComponent<PlayerStats>().IsEnemy)
		{
			SignalRShooting.instance.RegisteredHitBullet(GetComponent<SignalRIdentity>().NetworkID, collision.GetComponent<SignalRIdentity>().NetworkID);			
		}
	}

	void FixedUpdate()
	{
		_rigidbody2D.AddForce(transform.right * speed);
	}
}
