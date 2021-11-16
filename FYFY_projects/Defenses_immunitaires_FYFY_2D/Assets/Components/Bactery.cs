using UnityEngine;

public class Bactery : MonoBehaviour {
	public float _toxinDamages;
	public double _reloadTime;
	public GameObject _toxinPrefab;

	[HideInInspector]
	public double _reloadProgress;
}