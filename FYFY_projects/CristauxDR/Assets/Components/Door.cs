using UnityEngine;

public class Door : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	[HideInInspector]
	public bool isOpen = false;

	public GameObject constraint;
}