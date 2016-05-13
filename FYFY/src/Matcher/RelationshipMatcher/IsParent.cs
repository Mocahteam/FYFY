namespace FYFY {
	public class IsParent : Matcher {
		public IsParent() {
			_descriptor = "IsParent";
		}

		internal override bool matches(GameObjectWrapper gameObjectWrapper){
			return gameObjectWrapper._gameObject.transform.childCount != 0;
		}
	}
}