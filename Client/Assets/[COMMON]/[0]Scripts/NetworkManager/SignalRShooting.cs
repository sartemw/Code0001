using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Newtonsoft.Json;
using SpaceAdSLibrary;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SignalRShooting : MonoBehaviour {

	public static SignalRShooting instance = null; // Экземпляр объекта

	//лист пуль на регистрацию
	private List<BulletModel> _instantiateBulletPool = new List<BulletModel>();
	//лист пуль на попадание
	private List<HitModel> _hitBulletPool = new List<HitModel>();
	//лист существующих пуль
	public List<GameObject> BulletsInGame = new List<GameObject>();

	private SignalRClient _signalRClient;
	private GameHelper _gameHelper;
	private HubConnection _hubConnection = null;
	private IHubProxy _hubProxy;

	#region Singlton
	private void Awake()
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
						BulletModel bulletModel = new BulletModel();
						bulletModel = JsonConvert.DeserializeObject<BulletModel>(item.ToString());
						//добавляем эти объекты в Pool и когда смогу, то Instantiate их в LateUpdate
						_instantiateBulletPool.Add(bulletModel);
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
			foreach (var bulletModel in _instantiateBulletPool)
			{
				ObjectPooler objectPooler = new ObjectPooler();
				GameObject gameObj = new GameObject();
				//думаю тут как-то можно оптимизировать с удержанием пула от одного игрока, и если его пуль нету, то тогда переходится на другого
				//хотя похоже это бред, ведь в один пул не должно попадать две пули от одного человека, ну я так считаю
				//в любом случае это как-то долго и нужно чтото с этим сделать.

				foreach (var player in _gameHelper.AllPlayers)
				{					
					if (bulletModel.PlayerId == player.GetComponent<SignalRIdentity>().NetworkID)
					{
						objectPooler = player.GetComponentInChildren<ObjectPooler>();

						gameObj =
								objectPooler.SpawnFromPool(bulletModel.PrefabName,
								new Vector3(bulletModel.X, bulletModel.Y, 0),
								new Quaternion(0, 0, bulletModel.aZ, bulletModel.aQ)) as GameObject;
					}
				}

				BulletsInGame.Add(gameObj);
				//	Instantiate(Resources.Load<GameObject>("Bullets/" + bulletModel.PrefabName),
				//	new Vector3(bulletModel.X, bulletModel.Y, 0),
				//  new Quaternion(0,
				//				0,
				//				bulletModel.aZ,
				//				bulletModel.aQ)) as GameObject;								
			}
			//объект призван, очищает список			
			_instantiateBulletPool.Clear();
		}
	}

	#region Попадание пули
	public void RegisteredHitBullet(HitModel hitModel)
	{
		_hubProxy.Invoke("RegisteredHitBullet", hitModel);
	}
	//удаляем пулю, регистрируем попадание у остальных игроков
	private void HitInUpdate()
	{
		if(_hitBulletPool.Count > 0)
		{
			foreach (var hitModel in _hitBulletPool)
			{
				foreach (var bullet in BulletsInGame)
				{
					int bulletNetworkID = bullet.GetComponent<SignalRIdentity>().NetworkID;
					int bulletDamage = bullet.GetComponent<BulletStats>().Damage;

					if (bulletNetworkID == hitModel.bulletID)
					{
						foreach (var target in _gameHelper.AllPlayers)
						{
							int targetHealth = target.GetComponent<PlayerStats>().Health;
							int targetNetworkID = target.GetComponent<SignalRIdentity>().NetworkID;
							BulletController bulletController = bullet.GetComponent<BulletController>();

							if (bulletNetworkID == hitModel.targetID)
							{
								targetHealth -= bulletDamage;						

								//если мало здоровья то отключаем игрока, надо будет переделать под рестарт игрока, либо переход его в меню со статистикой.
								if (targetHealth <= 0)
								{
									SyncObjectModel syncObjectModel = new SyncObjectModel();
									syncObjectModel.Id = targetNetworkID;
									_signalRClient._disconnectedModels.Add(syncObjectModel);
								}

								if (!bullet.GetComponent<BulletStats>().Perforation)
								{
									bulletController.BulletOff();
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
