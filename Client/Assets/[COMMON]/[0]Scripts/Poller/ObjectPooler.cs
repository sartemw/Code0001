using System.Collections.Generic;
using UnityEngine;


/*
Скрипт вешать на объект с WeaponController'ом
Для работы нужен объект с PlayerStats'ом,
С заполенным полем Bullets[].
Так же используется BulletStats, для определения числа пулов.
*/
public class ObjectPooler : MonoBehaviour {

	public List<Pool> Pools;
	public Dictionary<string, Queue<GameObject>> PoolDictionary;
		
	void Start ()
	{

		PlayerStats _playerStats = GetComponentInParent<PlayerStats>();
		GameObject _bulletsContainer = GameObject.Find("BulletsContainer");
		for (int i = 0; i < _playerStats.Bullets.Length; i++)
		{
			int _size = 0;
			BulletStats _bulletStats = _playerStats.Bullets[i].GetComponent<BulletStats>();

			if (_bulletStats.Patrons >= 10)
				_size = 10;
			else _size = _bulletStats.Patrons;

			Pool _poolObj = new Pool {Size = _size,
									  Tag = _bulletStats.gameObject.name,
									  Prefab = _bulletStats.gameObject};

			Pools.Add(_poolObj);
		}
		
		PoolDictionary = new Dictionary<string, Queue<GameObject>>();

		foreach (Pool _pool in Pools)
		{
			Queue<GameObject> _objectPool = new Queue<GameObject>();
			
			for (int i = 0; i < _pool.Size; i++)
			{
				GameObject _obj = Instantiate(_pool.Prefab);
				_obj.SetActive(false);
				_objectPool.Enqueue(_obj);
				_obj.transform.SetParent(_bulletsContainer.transform);
				
				SignalRIdentity signalRIdentity = new SignalRIdentity
				{
					NetworkID = i,
					ParentID = GetComponentInParent<SignalRIdentity>().NetworkID 
				};
			}

			PoolDictionary.Add(_pool.Tag, _objectPool);
		}
	}
	
	public GameObject SpawnFromPool (string _tag, Vector3 _position,Quaternion _rotation)
	{
		if (!PoolDictionary.ContainsKey(_tag))
		{
			Debug.LogWarning("Pool with tag " + _tag + " doesn't excist");
			return null;
		}

		GameObject _objectToSpawn = PoolDictionary[_tag].Dequeue();

		_objectToSpawn.SetActive(true);
		_objectToSpawn.transform.position = _position;
		_objectToSpawn.transform.rotation = _rotation;
		
		IPooledObject _pooledObject = _objectToSpawn.GetComponent<IPooledObject>();

		if (_pooledObject != null)
		{
			_pooledObject.OnObjectSpawn();
		}

		PoolDictionary[_tag].Enqueue(_objectToSpawn);
		return _objectToSpawn;
	}
}
