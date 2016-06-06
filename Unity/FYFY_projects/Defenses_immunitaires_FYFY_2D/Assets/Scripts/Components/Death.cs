using UnityEngine;

public class Death : MonoBehaviour {
	[HideInInspector]
	public float _duration = 0.8f;
	[HideInInspector]
	public float _progress = 0f;
	[HideInInspector]
	public int _wastesNumber = 0;
	[HideInInspector]
	public int _virusNumber = 0;
	[HideInInspector]
	public VirusProperties _virusProperties = null;
}
