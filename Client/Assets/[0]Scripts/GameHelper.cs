using SpaceAdSLibrary;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameHelper : MonoBehaviour {

	public Transform[] StartPosition;
	public Transform[] Boundaries;


	//Текущий игрок
	private SyncObjectModel _currentPlayer;
	public SyncObjectModel CurrentPlayer
	{
		get { return _currentPlayer; }
		set {_currentPlayer = value; }
	}
	public GameObject CurrentPlayerGameObject { get; internal set; }

	//все кроме меня
	private List<SignalRIdentity> _otherPlayers = new List<SignalRIdentity>();
	public List<SignalRIdentity> OtherPlayers
	{
		get { return _otherPlayers; }
		set { _otherPlayers = value; }
	}

	//Все игроки
	private List<GameObject> _allPlayers = new List<GameObject>();
	public List<GameObject> AllPlayers
	{
		get { return _allPlayers; }
		set { _allPlayers = value; }
	}

	

	private void Start()
	{
		//Ищим все спаун поинты
		SpawnPointsFinder();

		//Ищим границы
		BoundaryFinder();
	}

	private void BoundaryFinder()
	{
		GameObject boundaryShell = GameObject.Find("BoundaryShell");

		Boundaries = new Transform[boundaryShell.transform.childCount];

		for (int i = 0; i < boundaryShell.transform.childCount; i++)
		{
			Boundaries[i] = boundaryShell.transform.GetChild(i);
		}
	}

	private void SpawnPointsFinder()
	{
		GameObject spawnShell = GameObject.Find("SpawnShell");

		StartPosition = new Transform[spawnShell.transform.childCount];

		for (int i = 0; i<spawnShell.transform.childCount; i++)
		{
			StartPosition[i] = spawnShell.transform.GetChild(i);
		}
	}
}
