﻿using Microsoft.AspNet.SignalR.Client;
using SpaceAdSLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour {

	SignalRIdentity _signalRIdentity;

	//префаб снаряда для стрельбы
	GameObject shotPrefab;

	public GameObject[] Bullets;

	//время перезарядки в секунду
	[HideInInspector]
	public float shootingRate = 0.25f;
	//ПЕРЕЗАРЯДКА
	[HideInInspector]
	public float shotCooldown;

	Vector3 mousePosition;
	Vector3 difference;

	private SignalRClient signalRClient;
	private Vector3 bulletPosition;
	private Quaternion bulletRotation;
	private string bulletName;
	private IHubProxy hubProxy;

	void Start () {
		signalRClient = SignalRClient.instance;
		hubProxy = signalRClient.HubProxy;
		_signalRIdentity = GetComponentInParent<SignalRIdentity>();
		shootingRate = Bullets[0].GetComponent<BulletStats>().AttackSpeed;
		shotCooldown = 0f;
		shotPrefab = Bullets[0];
	}
		
	void Update ()
	{
		if (!_signalRIdentity.IsAuthority)
			return;

		RotateWeapon();

		if (shotCooldown > 0)		
			shotCooldown -= Time.deltaTime;
		
		if (Input.GetButton("Fire1"))
			Attack();
	}
	
	//3 -СТРЕЛЬБА ИЗ ДРУГОГО СКРИПТА
	public void Attack()
	{
		if (CanAttack)
		{
			shotCooldown = shootingRate;

			//создайте новый выстрел
			RegisterBullet();
		//Instantiate(shotPrefab, transform.position, transform.rotation);
		}
	}
			

//готово ли оружие выпустить новый снаряд ?
public bool CanAttack
{
	get { return shotCooldown <= 0f; }
}

void RotateWeapon()
	{		
		var mousePosition = Input.mousePosition;
		//mousePosition.z = transform.position.z - Camera.main.transform.position.z; // это только для перспективной камеры необходимо 
		mousePosition = Camera.main.ScreenToWorldPoint(mousePosition); //положение мыши из экранных в мировые координаты 
		var angle = Vector2.Angle(Vector2.right, mousePosition - transform.position);//угол между вектором от объекта к мыше и осью х 
		transform.eulerAngles = new Vector3(0f, 0f, transform.position.y > mousePosition.y ? -angle : angle);//немного магии на последок 
	}

		//Показываем пулю всем
	public void RegisterBullet()
	{	
		//Куда кинуть пулю
		bulletPosition = transform.position;
		bulletRotation = transform.rotation;
		bulletName = shotPrefab.name;
		
		InstantiateBullet(bulletName, bulletPosition, bulletRotation);
	}

	//добавляем объект на сцену
	public void InstantiateBullet(string prefabName, Vector3 pos, Quaternion rot)
	{
		//это отправится на сервер и вышлется всем игрокам
		SyncObjectModel syncObjectModel = new SyncObjectModel()
		{
			PrefabName = prefabName,

			X = pos.x,
			Y = pos.y,
			Z = pos.z,

			aX = rot.x,
			aY = rot.y,
			aZ = rot.z,
			aQ = rot.w
		};

		hubProxy.Invoke("RegisterObjectBullet", syncObjectModel);
	}
}
