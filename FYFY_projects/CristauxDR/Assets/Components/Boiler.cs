using UnityEngine;

public class Boiler : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	[HideInInspector]
	public bool isOn = false;
	public float defaultTemperature = -3f;
	public GameObject constraint;
}