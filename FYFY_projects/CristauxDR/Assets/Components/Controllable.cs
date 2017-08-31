using UnityEngine;

public class Controllable : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public int maxSpeed = 10;
	[HideInInspector]
	public string currentAnim = "";
}