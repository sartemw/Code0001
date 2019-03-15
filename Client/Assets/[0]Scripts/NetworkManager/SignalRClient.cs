using Newtonsoft.Json;
using Microsoft.AspNet.SignalR.Client.Hubs;
using SpaceAdSLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.AspNet.SignalR.Client;

public class SignalRClient : MonoBehaviour
{

	public string signalRUrl;

	//массив тех кого добавить в игру, очищается после добавления
	List<SyncObjectModel> _instantiatePlayerPool = new List<SyncObjectModel>();
	
	//массив на обновление позиций
	[HideInInspector] public List<SyncObjectModel> _syncObjectPool = new List<SyncObjectModel>();
	//массив на удаление игроков
	List<SyncObjectModel> _disconnectedModels = new List<SyncObjectModel>();
	//сообщение
	


	//для чата
	public Text Show;
	public InputField InputText;
	public InputField NickName;
	string _chatModels;

	public static SignalRClient instance = null; // Экземпляр объекта
	public IHubProxy HubProxy;

	private string result;	
	private Vector3 _playerSpawnPosition;
	private PlayerController _playerController;

	[HideInInspector] public GameHelper GameHelper;
	[HideInInspector] public HubConnection HubConnection = null;
	[HideInInspector] public bool IsPlayerRegister = false;




	// Метод, выполняемый при старте игры	
	IEnumerator Start()
	{
		#region Singlton
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
		#endregion
		
		Debug.Log("Start()");
		GameHelper = transform.parent.Find("GameHelper").GetComponent<GameHelper>();

		yield return new WaitForSeconds(1);
		Debug.Log("Start() 1 second");
		
		StartSignalR();


		StartCoroutine(SendAndSyncData());
	}

	private IEnumerator SendAndSyncData()
	{

		yield return new WaitForSeconds(0.06f); //каждые 60 мс шлет Update



		if (IsPlayerRegister)
		{
			//реализация дисконекта
			DisconnectInUpdate();

			if (GameHelper.CurrentPlayerGameObject)
			{

				Vector3 pos = _playerController.SyncPosition;
				Quaternion rot = _playerController.SyncRotation;

				SyncObjectModel syncObjectModel = new SyncObjectModel()
				{
					Id = GameHelper.CurrentPlayerGameObject.GetComponent<SignalRIdentity>().NetworkID,

					X = pos.x,
					Y = pos.y,
					Z = pos.z,

					aX = rot.x,
					aY = rot.y,
					aZ = rot.z,
					aQ = rot.w
				};
				//отправляет на обновление syncObjectModel
				HubProxy.Invoke("UpdateModel", syncObjectModel);
			}

			//синкаем позицию
			if (_syncObjectPool.Count > 0)
			{
				foreach (var syncObjectModel in _syncObjectPool)
				{
					SignalRIdentity signalRIdentity = GameHelper.OtherPlayers.FirstOrDefault(p => p.NetworkID == syncObjectModel.Id);
					if (signalRIdentity)
					{
						PlayerController playerController = signalRIdentity.GetComponent<PlayerController>();

						Vector3 pos = new Vector3(syncObjectModel.X, syncObjectModel.Y, syncObjectModel.Z);
						Quaternion rot = new Quaternion(syncObjectModel.aX,
														syncObjectModel.aY,
														syncObjectModel.aZ,
														syncObjectModel.aQ);

						playerController.SyncPosition = pos;
						playerController.SyncRotation = rot;
					}
				}
			}
			_syncObjectPool.Clear();



		}
		StartCoroutine(SendAndSyncData());
	}

	//Конект к хабу!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
	private void StartSignalR()
	{
		Debug.Log("StartSignalR");
		if (HubConnection == null)
		{
			try
			{
				HubConnection = new HubConnection(signalRUrl);
				Debug.Log(signalRUrl);
				HubConnection.Error += hubConnection_Error;

				HubProxy = HubConnection.CreateHubProxy ("h");


				//подписываемся на рассылку сообщений
				Subscription subscriptionMessage = HubProxy.Subscribe("broadcastMessage");
				subscriptionMessage.Received += subscription_DataMessage =>
				{
					Debug.Log("Message1");
					foreach (var item in subscription_DataMessage)
					{
						Debug.Log(item);
						//десериализуем объект в item
						ChatModel chatModel = new ChatModel();
						chatModel = JsonConvert.DeserializeObject<ChatModel>(item.ToString());

						//добавляем эти объекты в Pool и когда смогу, то Instantiate их в LateUpdate
						_chatModels = (chatModel.Name + ": " + chatModel.Message + Environment.NewLine);

					}
				};


				//подписываемся на рассылку обновления игры
				Subscription subscription = HubProxy.Subscribe("updateGameModels");
				subscription.Received += subscription_Data =>
				{
					foreach (var item in subscription_Data)
					{

						//десериализуем объект syncObjectModel в item
						SyncObjectModel syncObjectModel = new SyncObjectModel();
						syncObjectModel = JsonConvert.DeserializeObject<SyncObjectModel>(item.ToString());

						//добавляем эти объекты в syncPool
						_syncObjectPool.Add(syncObjectModel);
					}
				};

				//подписываемся на рассылку спауна
				Subscription subscriptionInstantiatePrefab = HubProxy.Subscribe("instantiatePrefab");
				subscriptionInstantiatePrefab.Received += subscription_DataInstantiatePrefab =>
				{
					if (!IsPlayerRegister) return;

					foreach (var item in subscription_DataInstantiatePrefab)
					{						
						//десериализуем объект в item
						SyncObjectModel syncObjectModel = new SyncObjectModel();
						syncObjectModel = JsonConvert.DeserializeObject<SyncObjectModel>(item.ToString());

						if (syncObjectModel.Authority == HubConnection.ConnectionId)						
							GameHelper.CurrentPlayer = syncObjectModel;						

						//добавляем эти объекты в Pool и когда смогу, то Instantiate их в LateUpdate
						_instantiatePlayerPool.Add(syncObjectModel);
					}
				};

				////подписываемся на рассылку пуль
				GetComponent<SignalRShooting>().StartShooting();
				//Subscription subscriptionInstantiateBullet = HubProxy.Subscribe("instantiateBullet");
				//subscriptionInstantiateBullet.Received += subscription_DataInstantiateBullet =>
				//{
				//	foreach (var item in subscription_DataInstantiateBullet)
				//	{
				//		//десериализуем объект в item
				//		SyncObjectModel syncObjectModel = new SyncObjectModel();
				//		syncObjectModel = JsonConvert.DeserializeObject<SyncObjectModel>(item.ToString());
				//		//добавляем эти объекты в Pool и когда смогу, то Instantiate их в LateUpdate
				//		_instantiateBulletPool.Add(syncObjectModel);
				//	}
				//};

				//подписываемся на рассылку выхода игрока
				Subscription subscriptionDisconnectPrefab = HubProxy.Subscribe("disconnectPrefab");
				subscriptionDisconnectPrefab.Received += subscription_DataDisconnectPrefab =>
				{
					foreach (var item in subscription_DataDisconnectPrefab)
					{
						//десериализуем объект в item
						SyncObjectModel disModel = new SyncObjectModel();
						disModel = JsonConvert.DeserializeObject<SyncObjectModel>(item.ToString());

						//добавляем эти объекты в Pool и когда смогу, то удалят их в LateUpdate
						_disconnectedModels.Add(disModel);
					}
				};



				Debug.Log("hubConnection.Start");
				HubConnection.Start();
			}
			catch (InvalidCastException e)
			{
				Debug.Log("Ошибка: " + e);
				OnApplicationQuit();
			}
		}
		else Debug.Log("SignalR already connected...");
	}

	private void SignalRClient_Received(IList<Newtonsoft.Json.Linq.JToken> obj)
	{
		throw new NotImplementedException();
	}

	#region Ошибки
	
	private void hubConnection_Error(System.Exception obj)
	{
		Debug.Log("Hub Error - " + obj.Message +
		Environment.NewLine + obj.InnerException +
		Environment.NewLine + obj.Data +
		Environment.NewLine + obj.StackTrace +
		Environment.NewLine + obj.TargetSite);
	}

	private void hubConnection_Closed()
	{
		Debug.Log("hubConnection_Closed()");
	}

	#endregion

	#region Рассылка
	//Рассылка сообщения в чат
	public void SendMessage()
	{
		ChatModel model = new ChatModel()
		{
			ID = 0,
			Name = NickName.text,
			Message = InputText.text
		};
		HubProxy.Invoke("send", model);
		InputText.text = "";
	}

	//Подключение игрока к серверу и запуск метода рассылки игрока
	public void RegisterPlayer()
	{
		if (IsPlayerRegister) return;

		//PlayerModel player = new PlayerModel();

		//Куда кинуть игрока
		_playerSpawnPosition =
			GameHelper.StartPosition[UnityEngine.Random.Range(0, GameHelper.StartPosition.Length)].position;

		//SignalR.Instantiate в точках спауна
		InstantiatePlayer("FirstSentinel", _playerSpawnPosition, Quaternion.identity);

		IsPlayerRegister = true;
	}

	//добавляем объект на сцену
	public void InstantiatePlayer(string prefabName, Vector3 pos, Quaternion rot)
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

		HubProxy.Invoke("RegisterPlayer", syncObjectModel);
	}

	//Игрок отключился
	public void DisconnectPlayer()
	{
		if (GameHelper.CurrentPlayer != null)
		{
			if (GameHelper.CurrentPlayer.Authority != null)
			{
				string id = GameHelper.CurrentPlayer.Authority;
				HubProxy.Invoke("DisconnectPlayer", id);
			}
		}
	
	}

	#endregion

	//если вышли с приложения нужно остановить Connection
	public void OnApplicationQuit()
	{
		DisconnectPlayer();
		Debug.Log("OnApplicationQuit() " + Time.time + " seconds");
		HubConnection.Error -= hubConnection_Error;
		Invoke("Stop", 2);

	}

	private void Stop()
	{
		HubConnection.Stop();
	}

	public void Update()
	{
		Show.text += _chatModels;
		_chatModels = null;
	}

	private void LateUpdate()
	{
		//реализация появления игрока
		RegisteredInUpdate();
		//BulletInUpdate();
	}

	#region InUpdate

	//Мотоды чтобы в Update много не было
	private void DisconnectInUpdate()
	{

		if (_disconnectedModels.Count > 0)
		{
			SignalRIdentity signalRIdentity;

			foreach (var disconnectedModel in _disconnectedModels)
			{
				//ищем по ID совпадение и удаляем объект
				foreach (var o in GameHelper.AllPlayers)
				{
					if (o.GetComponent<SignalRIdentity>().NetworkID == disconnectedModel.Id)
					{

						signalRIdentity = o.GetComponent<SignalRIdentity>();

						foreach (var item in _syncObjectPool)
						{
							if (item.Id == disconnectedModel.Id)
								_syncObjectPool.Remove(disconnectedModel);
							break;
						}

						GameHelper.OtherPlayers.Remove(signalRIdentity);
						GameHelper.AllPlayers.Remove(o);

						Destroy(o, 0.5f);
						break;
					}
				}
			}
			_disconnectedModels.Clear();
		}

	}

	private void RegisteredInUpdate()
	{
		if (_instantiatePlayerPool.Count > 0)
		{
			foreach (var syncObjectModel in _instantiatePlayerPool)
			{
				GameObject gameObj = GameObject.Instantiate(Resources.Load<GameObject>(syncObjectModel.PrefabName),
					new Vector3(syncObjectModel.X, syncObjectModel.Y, syncObjectModel.Z),
					new Quaternion(syncObjectModel.aX,
									syncObjectModel.aY,
									syncObjectModel.aZ,
									syncObjectModel.aQ)) as GameObject;

				SignalRIdentity signalRIdentety = gameObj.GetComponent<SignalRIdentity>();

				signalRIdentety.NetworkID = syncObjectModel.Id;

				if (HubConnection.ConnectionId == syncObjectModel.Authority)
				{
					signalRIdentety.IsAuthority = true;
					GameHelper.CurrentPlayerGameObject = gameObj;

					_playerController = GameHelper.CurrentPlayerGameObject.GetComponent<PlayerController>();
				}

				GameHelper.OtherPlayers.Add(signalRIdentety);
				GameHelper.AllPlayers.Add(gameObj);
			}
			//объект призван, очищает список			
			_instantiatePlayerPool.Clear();
		}
	}

	//private void BulletInUpdate()
	//{
	//	if (_instantiateBulletPool.Count > 0)
	//	{
	//		foreach (var syncObjectModel in _instantiateBulletPool)
	//		{
	//			GameObject gameObj = GameObject.Instantiate(Resources.Load<GameObject>("Bullets/" + syncObjectModel.PrefabName),
	//				new Vector3(syncObjectModel.X, syncObjectModel.Y, syncObjectModel.Z),
	//				new Quaternion(syncObjectModel.aX,
	//								syncObjectModel.aY,
	//								syncObjectModel.aZ,
	//								syncObjectModel.aQ)) as GameObject;

	//			SignalRIdentity signalRIdentety = gameObj.GetComponent<SignalRIdentity>();

	//			signalRIdentety.NetworkID = syncObjectModel.Id;

	//			if (HubConnection.ConnectionId == syncObjectModel.Authority)
	//			{
	//				signalRIdentety.IsAuthority = true;
	//			}
	//		}
	//		//объект призван, очищает список			
	//		_instantiateBulletPool.Clear();
	//	}
	//}
	#endregion
}
