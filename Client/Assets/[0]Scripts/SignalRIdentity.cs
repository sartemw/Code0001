using UnityEngine;

public class SignalRIdentity : MonoBehaviour
{

	public bool _isAuthority;
	public bool IsAuthority
	{
		get { return _isAuthority; }
		set { _isAuthority = value; }
	}

	private int _networkID;
	public int NetworkID
	{
		get { return _networkID; }
		set { _networkID = value; }
	}

	private string _playerName;
	public string PlayerName
	{
		get { return _playerName; }
		set { _playerName = value; }
	}

	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
