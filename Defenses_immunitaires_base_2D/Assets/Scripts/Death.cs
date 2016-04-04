using UnityEngine;

[DisallowMultipleComponent]
public class Death : MonoBehaviour {
	[HideInInspector]
	public float duration = 0.8f;
	[HideInInspector]
	public int wastesNumber;
	[HideInInspector]
	public int virusNumber;
	[HideInInspector]
	public VirusProperties virusProperties;

	private float progress = 0f;
	private Material material;

	private static Object wastePrefab = Resources.Load("Waste");
	private static Object virusPrefab = Resources.Load("Virus");

	void Awake(){
		material = this.gameObject.GetComponent<Renderer> ().material;
	}

	void FixedUpdate () {
		progress += Time.deltaTime;

		if (progress < duration) {
			Color color = material.color;
			float a = 1f - (progress / (float)duration);

			material.color = new Color (color.r, color.g, color.b, a);
		} else {
			for (int i = 0; i < wastesNumber; ++i) {
				GameObject wasteGO = (GameObject)Instantiate (wastePrefab);
				wasteGO.transform.position = this.gameObject.transform.position;
				wasteGO.GetComponent<MovingGO> ().target = wasteGO.transform.position;
				wasteGO.transform.Rotate (new Vector3 (0f, 0f, Random.Range (-180f, 180f)));
			}

			for(int i = 0; i < virusNumber; ++i){
				GameObject virusGO = (GameObject)Instantiate (virusPrefab);
				virusGO.transform.position = this.gameObject.transform.position;
				virusGO.GetComponent<MovingGO> ().target = virusGO.transform.position;
				virusGO.GetComponent<Virus> ().properties = virusProperties;
			}

			Destroy (this.gameObject);
		}
	}
}
