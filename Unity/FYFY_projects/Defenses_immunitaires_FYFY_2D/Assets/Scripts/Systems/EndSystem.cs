using FYFY;

public class EndSystem : FSystem {
	private Family _macrophages = FamilyManager.getFamily(
		new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED),
		new AllOfComponents(typeof(Macrophage))
	);
	private Family _bacteries = FamilyManager.getFamily(
		new AllOfProperties(PropertyMatcher.PROPERTY.ENABLED),
		new AllOfComponents(typeof(Bactery))
	);

	protected override void onPause(int currentFrame) {}

	protected override void onResume(int currentFrame) {}

	protected override void onProcess(int currentFrame) {
		if (_macrophages.Count == 0) {
			GameObjectManager.loadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
		} else if(_bacteries.Count == 0) {
			UnityEditor.EditorApplication.isPlaying = false;
		}
	}
}
