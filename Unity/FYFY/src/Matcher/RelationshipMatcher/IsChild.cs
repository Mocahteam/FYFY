namespace FYFY {
	public class IsChild : Matcher {
		public IsChild() {
			_descriptor = "IsChild";
		}

		internal override bool matches(GameObjectWrapper gameObjectWrapper){
			return gameObjectWrapper._gameObject.transform.parent != null;
		}
	}
}