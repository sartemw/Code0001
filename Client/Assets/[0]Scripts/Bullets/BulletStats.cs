using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class BulletStats : MonoBehaviour {

	public int _damage;
	public int Damage
	{
		get { return _damage; }
		set { _damage = value; }
	}

	public float _attackSpeed;
	public float AttackSpeed
	{
		get { return _attackSpeed; }
		set { _attackSpeed = value; }
	}

	public int _patrons;
	public int Patrons
	{

		get { return _patrons; }
		set { _patrons = value; }
	}

	public float _recharge;
	public float Recharge
	{

		get { return _recharge; }
		set { _recharge = value; }
	}

	public float _scatter;
	public float Scatter
	{

		get { return _scatter; }
		set { _scatter = value; }
	}

	public float _lifeTime;
	public float LifeTime
	{

		get { return _lifeTime; }
		set { _lifeTime = value; }
	}

	public int _bulletSpeed;
	public int BulletSpeed
	{

		get { return _bulletSpeed; }
		set { _bulletSpeed = value; }
	}

	public int _bulletSpeedRotation;
	public int BulletSpeedRotation
	{

		get { return _bulletSpeedRotation; }
		set { _bulletSpeedRotation = value; }
	}

	public enum Penetration { kinematic, thermal, energy };
	public Penetration[] _resistanceState;

	public int[] _penetrationValue;
	public int[] PenetrationValue
	{
		get { return _penetrationValue; }
		set { _penetrationValue = value; }
	}

	public int _critical;
	public int Critical
	{

		get { return _critical; }
		set { _critical = value; }
	}

	public bool _perforation;
	public bool Perforation
	{
		get { return _perforation; }
		set { _perforation = value; }
	}



}
