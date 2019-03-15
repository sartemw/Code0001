﻿using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Newtonsoft.Json;
using SpaceAdSLibrary;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SignalRShooting : MonoBehaviour {

	public static SignalRShooting instance = null; // Экземпляр объекта

	//лист пуль на регистрацию
	private List<SyncObjectModel> _instantiateBulletPool = new List<SyncObjectModel>();
	//лист пуль на попадание
	private List<HitModel> _hitBulletPool = new List<HitModel>();
	//лист существующих пуль
	private List<GameObject> _BulletsInGame = new List<GameObject>();

	private SignalRClient _signalRClient;
	private GameHelper _gameHelper;
	private HubConnection _hubConnection = null;
	private IHubProxy _hubProxy;

	#region Singlton
	private void Start()
	{
		
		// Теперь, проверяем существование экземпляра
		if (instance == null)
		{ // Экземпляр менеджера был найден
			instance = this; // Задаем ссылку на экземпляр объекта
		}
		else if (instance == this)
		{ // Экземпляр объекта уже существует на сцене
			Destroy(gameObject); // Удаляем объект
		}
		// Теперь нам нужно указать, чтобы объект не уничтожался
		// при переходе на другую сцену игры
		
	}
	#endregion

	//Типа старт, запускается при выполеном подключении
	public void StartShooting () {
		_signalRClient = GetComponent<SignalRClient>();
		_hubConnection = _signalRClient.HubConnection;
		_hubProxy = _signalRClient.HubProxy;
		_gameHelper = _signalRClient.GameHelper;
				
				//подписываемся на рассылку пуль
				Subscription subscriptionInstantiateBullet = _signalRClient.HubProxy.Subscribe("instantiateBullet");
				subscriptionInstantiateBullet.Received += subscription_DataInstantiateBullet =>
				{
					foreach (var item in subscription_DataInstantiateBullet)
					{
						//десериализуем объект в item
						SyncObjectModel syncObjectModel = new SyncObjectModel();
						syncObjectModel = JsonConvert.DeserializeObject<SyncObjectModel>(item.ToString());
						//добавляем эти объекты в Pool и когда смогу, то Instantiate их в LateUpdate
						_instantiateBulletPool.Add(syncObjectModel);
					}
				};

		//подписываемся на рассылку пуль
		Subscription subscriptionRegisteredHitBullet = _signalRClient.HubProxy.Subscribe("registeredHitBullet");
		subscriptionRegisteredHitBullet.Received += subscription_DataRegisteredHitBullet =>
		{
			foreach (var item in subscription_DataRegisteredHitBullet)
			{
				//десериализуем объект в item
				HitModel hitModel = JsonConvert.DeserializeObject<HitModel>(item.ToString());
				
				//добавляем в пул на регистрацию попаданий
				_hitBulletPool.Add(hitModel);				
			}
		};
	}

	private void LateUpdate()
	{
		BulletInUpdate();
		HitInUpdate();
	}

	private void BulletInUpdate()
	{
		if (_instantiateBulletPool.Count > 0)
		{
			foreach (var syncObjectModel in _instantiateBulletPool)
			{
				GameObject gameObj = Instantiate(Resources.Load<GameObject>("Bullets/" + syncObjectModel.PrefabName),
					new Vector3(syncObjectModel.X, syncObjectModel.Y, syncObjectModel.Z),
					new Quaternion(syncObjectModel.aX,
									syncObjectModel.aY,
									syncObjectModel.aZ,
									syncObjectModel.aQ)) as GameObject;

				SignalRIdentity signalRIdentety = gameObj.GetComponent<SignalRIdentity>();

				_BulletsInGame.Add(gameObj);

				signalRIdentety.NetworkID = syncObjectModel.Id;
				
				if (_signalRClient.HubConnection.ConnectionId == syncObjectModel.Authority)
				{
					signalRIdentety.IsAuthority = true;
				}
			}
			//объект призван, очищает список			
			_instantiateBulletPool.Clear();
		}
	}

	#region Попадание пули
	public void RegisteredHitBullet(int bulletID, int targetID)
	{
		HitModel hitModel = new HitModel();
		hitModel.bulletID = bulletID;
		hitModel.targetID = targetID;
		_hubProxy.Invoke("RegisteredHitBullet", hitModel);
	}
	//удаляем пулю, регистрируем попадание у остальных игроков
	private void HitInUpdate()
	{
		if(_hitBulletPool.Count > 0)
		{
			foreach (var hitModel in _hitBulletPool)
			{
				foreach (var bullet in _BulletsInGame)
				{
					if (bullet.GetComponent<SignalRIdentity>().NetworkID == hitModel.bulletID)
					{
						foreach (var target in _gameHelper.AllPlayers)
						{
							if (target.GetComponent<SignalRIdentity>().NetworkID == hitModel.targetID)
							{
								target.GetComponent<PlayerStats>().Health -= bullet.GetComponent<BulletStats>().Damage;

								//если мало здоровья то отключаем игрока, надо будет переделать под рестарт чтоли.
								if (target.GetComponent<PlayerStats>().Health <= 0)
								{
									SyncObjectModel syncObjectModel = new SyncObjectModel();
									syncObjectModel.Id = target.GetComponent<SignalRIdentity>().NetworkID;
									_signalRClient._disconnectedModels.Add(syncObjectModel);
								}

								if (!bullet.GetComponent<BulletStats>().Perforation)
								{
									_BulletsInGame.Equals(bullet);
									Destroy(gameObject);
								}
							}
						}
					}
				}
			}
		}
		_hitBulletPool.Clear();
	}
	#endregion
}
