namespace FYFY {
	public class AnyOfTags : TagMatcher {
		public AnyOfTags(params string[] tags) : base(tags) {
		}

		internal override bool matches(GameObjectWrapper gameObjectWrapper){
			if(gameObjectWrapper == null || gameObjectWrapper._gameObject == null)
				throw new System.ArgumentNullException();
			
			string gameObjectTag = gameObjectWrapper._gameObject.tag;

			for(int i = 0; i < _tags.Length; ++i)
				if(gameObjectTag == _tags [i])
					return true;
			return false;
		}
	}
}