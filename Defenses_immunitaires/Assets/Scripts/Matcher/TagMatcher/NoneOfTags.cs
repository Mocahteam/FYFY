public class NoneOfTags : TagMatcher {
	public NoneOfTags(params string[] tags) : base(tags) {
	}

	internal override bool matches(GameObjectWrapper gameObjectWrapper){
		string gameObjectTag = gameObjectWrapper._gameObject.tag;

		for(int i = 0; i < _tags.Length; ++i)
			if(gameObjectTag == _tags [i])
				return false;
		return true;
	}
}