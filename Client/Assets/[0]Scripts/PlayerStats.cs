using UnityEngine;

public class PlayerStats : MonoBehaviour {

	[SerializeField] private string _name;
	public string Name
	{
		get { return _name; }
		set { _name = value; }
	}

	[SerializeField] private bool _isEnemy;
	public bool IsEnemy
	{
		get { return _isEnemy; }
		set { _isEnemy = value; }
	}

	[SerializeField] private int _health;
	public int Health
	{
		get { return _health; }
		set { _health = value; }
	}

	[SerializeField] private int _energy;
	public int Energy
	{
		get { return _energy; }
		set { _energy = value; }
	}

	[SerializeField] private int _experience;
	public int Experience
	{

		get { return _experience; }
		set { _experience = value; }
	}

	[SerializeField] private int _defence;
	public int Defence
	{

		get { return _defence; }
		set { _defence = value; }
	}

	[SerializeField] private int _moveSpeed;
	public int MoveSpeed
	{

		get { return _moveSpeed; }
		set { _moveSpeed = value; }
	}

	[SerializeField] private int _forcing;
	public int Forcing
	{

		get { return _forcing; }
		set { _forcing = value; }
	}
	
	public enum Resistance { kinematic, thermal, energy};
	[SerializeField] private Resistance [] _resistanceState;

	[SerializeField] private int[] _resistanceValue;
	public int[] ResistanceValue
	{
		get { return _resistanceValue; }
		set { _resistanceValue = value; }
	}

	[SerializeField] private int _dodge;
	public int Dodge
	{

		get { return _dodge; }
		set { _dodge = value; }
	}

	[SerializeField] private GameObject[] _bullets;
	public GameObject[] Bullets
	{
		get { return _bullets; }
		set { _bullets = value; }
	}

	[SerializeField] private int[] _clips;
	public int[] Clips
	{

		get { return _clips; }
		set { _clips = value; }
	}
}
