using UnityEngine;

public class SignalRIdentity : MonoBehaviour
{

	private bool _isAuthority;
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

	private int _parentID;
	public int ParentID
	{
		get { return _parentID; }
		set { _parentID = value; }
	}
}
