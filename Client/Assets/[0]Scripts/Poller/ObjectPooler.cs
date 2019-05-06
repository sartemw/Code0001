using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour {

	public List<Pool> Pools;
	public Dictionary<string, Queue<GameObject>> PoolDictionary;

	#region Singlton
	public static ObjectPooler Instance = null;
	//RunSinglton надо добавить в Start
	private void RunSinglton()
	{
		if (Instance == null)
		{ // Экземпляр менеджера был найден
			Instance = this; // Задаем ссылку на экземпляр объекта
		}
		else if (Instance == this)
		{ // Экземпляр объекта уже существует на сцене
			Destroy(gameObject); // Удаляем объект
		}

		// Теперь нам нужно указать, чтобы объект не уничтожался
		// при переходе на другую сцену игры
		DontDestroyOnLoad(gameObject);
	}
	
	private void Awake()
	{
		RunSinglton();
	}


	#endregion


	void Start () {

		PoolDictionary = new Dictionary<string, Queue<GameObject>>();

		foreach (Pool _pool in Pools)
		{
			Queue<GameObject> _objectPool = new Queue<GameObject>();

			for (int i = 0; i < _pool.Size; i++)
			{
				GameObject _obj = Instantiate(_pool.Prefab);
				_obj.SetActive(false);
				_objectPool.Enqueue(_obj);
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
