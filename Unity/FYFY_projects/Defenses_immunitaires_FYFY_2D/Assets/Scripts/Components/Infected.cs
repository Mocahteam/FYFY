using UnityEngine;

public class Infected : MonoBehaviour {
	[HideInInspector]
	public VirusProperties _virusProperties;
	[HideInInspector]
	public int _virusNumberToCreate = 0;
	[HideInInspector]
	public float _virusProductionProgress = 0f;
}
