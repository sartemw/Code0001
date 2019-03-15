using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Newtonsoft.Json;
using SpaceAdSLibrary;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SignalRShooting : MonoBehaviour {

	//массив пуль
	private List<SyncObjectModel> _instantiateBulletPool = new List<SyncObjectModel>();

	private SignalRClient _signalRClient;
	private HubConnection _hubConnection = null;

	public void StartShooting () {
		_signalRClient = GetComponent<SignalRClient>();
		_hubConnection = _signalRClient.HubConnection;

				
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
	}

	private void LateUpdate()
	{
		BulletInUpdate();
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
}
