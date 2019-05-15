using Microsoft.AspNet.SignalR.Client;
using SpaceAdSLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour {

	SignalRIdentity _signalRIdentity;

	//префаб снаряда для стрельбы
	GameObject shotPrefab;

	

	//время перезарядки в секунду
	[HideInInspector]
	public float shootingRate = 0.25f;
	//ПЕРЕЗАРЯДКА
	[HideInInspector]
	public float shotCooldown;

	Vector3 mousePosition;
	Vector3 difference;

	private PlayerStats playerStats;
	private SignalRClient signalRClient;
	private Vector3 bulletPosition;
	private Quaternion bulletRotation;
	private string bulletName;
	private IHubProxy hubProxy;
	private int bulletID = 0;

	private int clip;
	private List<GameObject> shotedBullets = new List<GameObject>();

	void Start () {
		signalRClient = SignalRClient.instance;
		hubProxy = signalRClient.HubProxy;
		_signalRIdentity = GetComponentInParent<SignalRIdentity>();
		playerStats = GetComponentInParent<PlayerStats>();

		clip = playerStats.Clips[0];
		shootingRate = playerStats.Bullets[0].GetComponent<BulletStats>().AttackSpeed;
		shotCooldown = 0f;
		shotPrefab = playerStats.Bullets[0];
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
		BulletModel bulletModel = new BulletModel()
		{
			//тут же добавить команду teamID
			PlayerId = GetComponentInParent<SignalRIdentity>().NetworkID,
			BulletId = bulletID,

			PrefabName = prefabName,
			
			X = pos.x,
			Y = pos.y,

			aZ = rot.z,
			aQ = rot.w
		};

		if (bulletID >= 10) bulletID = 0;
		else bulletID++;

		hubProxy.Invoke("RegisterObjectBullet", bulletModel);
	}
}
