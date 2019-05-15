using UnityEngine;

public class CanvasFunctional : MonoBehaviour {
	GameObject _changeObject;
	
	//Определяет объект для действия
	public void ChoiceObject(GameObject gO)
	{
		_changeObject = gO;
	}
	//это выбранный объект делает таким каким выбрано значение, в данном случае FALSE - ВЫКЛЮЧАЕТ
	public void EnabledGO (bool val)
	{
		_changeObject.SetActive(val);
	}
}
