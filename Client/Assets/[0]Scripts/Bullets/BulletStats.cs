using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class BulletStats : MonoBehaviour {

	[SerializeField] private int _damage;
	public int Damage
	{
		get { return _damage; }
		set { _damage = value; }
	}

	[SerializeField] private float _attackSpeed;
	public float AttackSpeed
	{
		get { return _attackSpeed; }
		set { _attackSpeed = value; }
	}

	[SerializeField] private int _patrons;
	public int Patrons
	{

		get { return _patrons; }
		set { _patrons = value; }
	}

	[SerializeField] private float _recharge;
	public float Recharge
	{

		get { return _recharge; }
		set { _recharge = value; }
	}

	[SerializeField] private float _scatter;
	public float Scatter
	{

		get { return _scatter; }
		set { _scatter = value; }
	}

	[SerializeField] private float _lifeTime;
	public float LifeTime
	{

		get { return _lifeTime; }
		set { _lifeTime = value; }
	}

	[SerializeField] private int _bulletSpeed;
	public int BulletSpeed
	{

		get { return _bulletSpeed; }
		set { _bulletSpeed = value; }
	}

	[SerializeField] private int _bulletSpeedRotation;
	public int BulletSpeedRotation
	{

		get { return _bulletSpeedRotation; }
		set { _bulletSpeedRotation = value; }
	}

	public enum Penetration { kinematic, thermal, energy };
	public Penetration[] _resistanceState;

	[SerializeField] private int[] _penetrationValue;
	public int[] PenetrationValue
	{
		get { return _penetrationValue; }
		set { _penetrationValue = value; }
	}

	[SerializeField] private int _critical;
	public int Critical
	{

		get { return _critical; }
		set { _critical = value; }
	}

	[SerializeField] private bool _perforation;
	public bool Perforation
	{
		get { return _perforation; }
		set { _perforation = value; }
	}



}
