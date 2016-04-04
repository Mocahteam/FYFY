using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour {
	// DontDestroyOnLoad ();

	void Update(){
		if (Macrophage.macrophageComponents.Count == 0) {
			SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);
		} else if (Bactery.bacteryComponents.Count == 0) {
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
			#else
			Application.Quit();
			#endif
		}
	}
}
