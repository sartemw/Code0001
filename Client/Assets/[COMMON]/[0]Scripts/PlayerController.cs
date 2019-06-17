using UnityEngine;

public class PlayerController : MonoBehaviour {

	SignalRIdentity _signalRIdentity;
	Rigidbody2D _rigidbody2D;
	PlayerStats _playerStats;		

		[Header ("Скорость сети")]
	int SyncRate = 3;

	float inclineX; //наклон
	float rotZ;
	float curVelocity;

	int speed;
	int rotationSpeed;

	float translation;
	public float Translation
	{
		get { return translation; }
		set { translation = value; }
	}

	float rotation;
	public float Rotation
	{
		get { return rotation; }
		set { rotation = value; }
	}
	
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

	private void Start()
	{
		_signalRIdentity = GetComponent<SignalRIdentity>();
		_rigidbody2D = GetComponent<Rigidbody2D>();
		_playerStats = GetComponent<PlayerStats>();

		speed = _playerStats.MoveSpeed;

		rotationSpeed = speed * 30;
		inclineX = 0;
	}


	private void Update()
	{
		
		if (!_signalRIdentity.IsAuthority)
			return;
		
			//Иду! Передвигаюсь!
			Move();	

		_syncPosition = transform.position;
		_syncRotation = transform.rotation;

		
	}

	void FixedUpdate()
	{
		
		if (_signalRIdentity.IsAuthority ||
   (_syncPosition == transform.position && _syncRotation == transform.rotation))
			return;

		//синкаем позицию
		transform.position = Vector3.Lerp(transform.position, _syncPosition, Time.deltaTime * SyncRate);
		transform.rotation = Quaternion.Lerp(transform.rotation, _syncRotation, Time.deltaTime * SyncRate);
		
	}

	private void Move()
	{
		translation = Input.GetAxis("Vertical") * speed * Time.deltaTime;
		rotation = Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime;

		if (rotation != 0)
		{
			inclineX = Mathf.Clamp(inclineX += rotation, -30, 30);
		}
		else
		{
			inclineX = Mathf.SmoothDamp(inclineX, 0, ref curVelocity, 1);
		}

		rotZ += rotation;
		transform.rotation = Quaternion.Euler(inclineX, 0, -rotZ);

		_rigidbody2D.AddForce(transform.right* 60 * translation);
	}
}
