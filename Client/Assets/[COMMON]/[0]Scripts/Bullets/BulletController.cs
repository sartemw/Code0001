using SpaceAdSLibrary;
using UnityEngine;

public class BulletController : MonoBehaviour {
	[Header("Скорость сети")]
	int SyncRate = 3;

	int speed, damage ;

	Vector3 _syncPosition;
	public Vector3 SyncPosition
	{
		get { return _syncPosition; }
		set { _syncPosition = value; }
	}
	Quaternion _syncRotation;
	public Quaternion SyncRotation
	{
		get { return _syncRotation; }
		set { _syncRotation = value; }
	}

	private bool _onPlay = false;


	Rigidbody2D _rigidbody2D;
	BulletStats _bulletStats;
	SignalRShooting _signalRShooting;
	SignalRIdentity _signalRIdentity;

	private void Start()
	{
		_rigidbody2D = GetComponent<Rigidbody2D>();
		_bulletStats = GetComponent<BulletStats>();
		_signalRShooting = SignalRShooting.instance;
		_signalRIdentity = GetComponent<SignalRIdentity>();

		speed = _bulletStats.BulletSpeed*10;
		damage = _bulletStats.Damage;

		Invoke("BulletOff", _bulletStats.LifeTime);

		//выдается ошибка так, как поля еще не обозначены, а OnEnable запускается быстрее Start.
		//А сам OnEnable нужен потому, что Start работает лишь при 1ом включении
		_onPlay = true;
	}

	private void OnEnable()
	{
		if (_onPlay == true)
		Invoke("BulletOff", _bulletStats.LifeTime);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		
		if (GetComponent<SignalRIdentity>().IsAuthority
			&& collision.GetComponent<PlayerStats>().IsEnemy)
		{
			HitModel hitModel = new HitModel()
			{
				bulletID = _signalRIdentity.NetworkID,
				targetID = collision.GetComponent<SignalRIdentity>().NetworkID,
				playerID = _signalRIdentity.ParentID,
				damage = _bulletStats.Damage

			};

			SignalRShooting.instance.RegisteredHitBullet(hitModel);			
		};
	}

	void FixedUpdate()
	{
		_rigidbody2D.AddForce(transform.right * speed);
	}
	
	public void BulletOff()
	{
		this.gameObject.SetActive(false);
		_signalRShooting.BulletsInGame.Equals(this.gameObject);
	}
}
