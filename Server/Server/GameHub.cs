using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using SpaceAdSLibrary;

namespace Server
{
	[HubName("h")]
	public class GameHub : Hub
	{
		// обращение к GameBroadcast _instance через _broadcast
		private GameBroadcaster _broadcaster;
		public GameHub()
			: this(GameBroadcaster.Instance) { }

		public GameHub(GameBroadcaster broadcaster)
		{
			_broadcaster = broadcaster;
		}

		#region Player
		//обновление модели
		public void UpdateModel(SyncObjectModel clientModel)
		{
			//определяет кто главный над объектом
			clientModel.Authority = Context.ConnectionId;

			_broadcaster.updateModel(clientModel);
		}

		//регистрация игрока
		public void RegisterPlayer(SyncObjectModel obj)
		{
			obj.Authority = Context.ConnectionId;

			_broadcaster.registerPlayer(obj);
		}

		//дисконект игрока
		public void DisconnectPlayer(string ID)
		{
			_broadcaster.DisconnectPlayer(ID);
		}
		#endregion

		#region Bullet
		//Регистрируем объект (капсулы)
		public void RegisterObjectBullet (SyncObjectModel obj)
		{
			obj.Authority = Context.ConnectionId;

			_broadcaster.RegisterModelBullet(obj);
		}
		
		//Регистрируем попадание пули
		public void RegisteredHitBullet(HitModel hitModel)
		{			
			_broadcaster.registeredHitBullet(hitModel);
		}
		#endregion

		/*
		public override Task OnDisconnected(bool stopCalled)
		{
			if (stopCalled)
			{
				// We know that Stop() was called on the client,
				// and the connection shut down gracefully.
			}
			else
			{
				// This server hasn't heard from the client in the last ~35 seconds.
				// If SignalR is behind a load balancer with scaleout configured, 
				// the client may still be connected to another SignalR server.
			}
			return base.OnDisconnected(stopCalled);
		}
		*/

		//рассылка сообщения
		public void Send(ChatModel model)
		{
			// Call the broadcastMessage method to update clients.
			Clients.All.broadcastMessage(model);
		}
	}
}