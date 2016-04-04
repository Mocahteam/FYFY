using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class Bactery : MonoBehaviour {
	public static List<Bactery> bacteryComponents = new List<Bactery>();

	public int toxinDamages;
	public double reloadTime;

	private double reloadProgress = 0;
	private static Object toxinPrefab = Resources.Load("Toxin");

	void Awake(){
		bacteryComponents.Add (this);
	}

	void OnDestroy(){
		bacteryComponents.Remove (this);
	}

	void FixedUpdate(){
		reloadProgress += Time.deltaTime;
		if (reloadProgress >= reloadTime) {
			reloadProgress = 0;

			GameObject toxinGO = (GameObject) Instantiate(toxinPrefab);
			toxinGO.transform.position = this.gameObject.transform.position;
			toxinGO.GetComponent<MovingGO> ().target = toxinGO.transform.position;
			toxinGO.GetComponent<Toxin>().damages = toxinDamages;
		}
	}
}