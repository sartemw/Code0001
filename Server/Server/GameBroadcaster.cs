﻿using Microsoft.AspNet.SignalR;
using SpaceAdSLibrary;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;


namespace Server
{
	public class GameBroadcaster
	{
		public static GameBroadcaster Instance
		{
			get
			{
				return _instance.Value;
			}
		}

		private readonly static Lazy<GameBroadcaster> _instance =
			new Lazy<GameBroadcaster>(() => new GameBroadcaster());

		//Обновление каждые 25 кадров
		private readonly TimeSpan BroadcastInterval =
			TimeSpan.FromMilliseconds(40);

		private readonly IHubContext _hubContext;
		private Timer _broadcastLoop;

		//Потокозащищенные Массивы

		//массив который нужно разослать всем остальным игрокам за 1 фрейм
		ConcurrentQueue<SyncObjectModel> _oneFrameSyncModels = new ConcurrentQueue<SyncObjectModel>();

		//массив который нужно разослать всем вновь зашедшим, чтобы прогрузились играющие
		ConcurrentQueue<SyncObjectModel> _registerModels = new ConcurrentQueue<SyncObjectModel>();	
		
		//массив который нужно разослать всем вновь зашедшим, чтобы прогрузились выстреленные пули
		ConcurrentQueue<SyncObjectModel> _registerBullets = new ConcurrentQueue<SyncObjectModel>();

		//массив зарегестрированных игроков
		ConcurrentQueue<PlayerModel> _bullets = new ConcurrentQueue<PlayerModel>();

		//было обновление или нет
		private bool _modelUpdated;


		public GameBroadcaster()
		{
			// Save our hub context so we can easily use it 
			// to send to its connected clients
			_hubContext = GlobalHost.ConnectionManager.GetHubContext<GameHub>();

			_modelUpdated = false;
			// Start the broadcast loop
			_broadcastLoop = new Timer(
				BroadcastShape,
				null,
				BroadcastInterval,
				BroadcastInterval);
		}

		//рассылка обновления
		public void BroadcastShape(object state)
		{
			if (_modelUpdated)
			{
				foreach (var item in _oneFrameSyncModels)
				{
					//если зарегестрирован значит имеет право слать свои координаты
					//шлет всем кроме себя
					if (_registerModels.Any(p => p.Authority == item.Authority))
						//вызывать (AllExcept) у всех, кроме отсылающего (item.Authority) метод  updateGameModels и слать в него item
						_hubContext.Clients.AllExcept(item.Authority).updateGameModels(item);
				}
				//здесть обнуляем список изменений
				_oneFrameSyncModels = new ConcurrentQueue<SyncObjectModel>();
				_modelUpdated = false;
			}
		}

		//обновление модели
		public void UpdateModel(SyncObjectModel clientmodel)
		{
			_oneFrameSyncModels.Enqueue(clientmodel);
			_modelUpdated = true;
		}		

		//добавление игрока в игру
		public void AddPlayerInGame(SyncObjectModel clientmodel)
		{
			//даем уникальный ID
			clientmodel.Id = _registerModels.Count();

			_registerModels.Enqueue(clientmodel);

			//тут надо сделать проверку был или не был этот игрок


			_hubContext.Clients.All.instantiatePrefab(clientmodel);

			//вновь зарегестрированному игроку отправляются все кто подключился до него
			foreach (var item in _registerModels)
			{
				if (item.Authority != clientmodel.Authority)
				_hubContext.Clients.Client(clientmodel.Authority).instantiatePrefab(item);
			}

		}

		//модели зарегестрированные на игровой карте
		public void RegisterModelBullet(SyncObjectModel bulletmodel)
		{
			////даем уникальный ID
			//bulletmodel.Id = _registerBullets.Count();

			//_registerBullets.Enqueue(bulletmodel);

			_hubContext.Clients.All.instantiateBullet(bulletmodel);
		}

		//добавление игрока в игру
		public void AddBulletInGame(PlayerModel bullet)
		{
			//Хранит сколько всего игроков
			int playerCount = _bullets.Count();

			//Добавляем номер игроку
			bullet.PlayerIndex = playerCount;

			//тут надо сделать проверку был или не был этот игрок

			//добавляем игкрока в потокозащищенный лист
			_bullets.Enqueue(bullet);
		}

		//Удаляем модель и игрока из всех списков
		public void DisconnectPlayer(string id)
		{
			SyncObjectModel x;			

			//удаляем игрока из коллекции отвечающую за появление вновь зашедшего.			 
			foreach (var item in _registerModels)
			{				
				if (item.Authority == id)
				{
					_registerModels.TryDequeue(out x);
					_hubContext.Clients.All.disconnectPrefab(item);
					break;
				}
				else //перекидываем в конец очереди	
				{
					_registerModels.TryDequeue(out x);
					_registerModels.Enqueue(item);
				}
			}
			
			//удаляем игрока из коллекции отвечающую за обновление состояний моделей.
			foreach (var item in _oneFrameSyncModels)
			{
				if (item.Authority == id)
				{
					_oneFrameSyncModels.TryDequeue(out x);
					break;
				}
				else //перекидываем в конец очереди				
				{
					_oneFrameSyncModels.TryDequeue(out x);
					_oneFrameSyncModels.Enqueue(item);
				}
			}
		}

	}
}