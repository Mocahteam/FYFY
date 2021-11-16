using UnityEngine;

[System.Serializable]
public class VirusProperties {
	public float _damages = 2;
	public float _productionTime = 10f;
	public float _infectionChance = 0.25f; // [0, 1]
	public GameObject _virusPrefab;
}

public class Virus : UnityEngine.MonoBehaviour {
	public VirusProperties _properties = new VirusProperties();
}
