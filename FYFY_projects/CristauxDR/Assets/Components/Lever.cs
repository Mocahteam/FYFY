using UnityEngine;

public class Lever : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	[HideInInspector]
	public bool isOn = false;

	public Sprite on;
	public Sprite off;
}