namespace FYFY {
	/// <summary>
	///		Matcher used to exclude all the <c>GameObjects</c> which have
	///		one of tags of a specific list of tags.
	/// </summary>
	public class NoneOfTags : TagMatcher {
		/// <summary>
		/// 	Initializes a new instance of the <see cref="FYFY.NoneOfTags"/> class.
		/// </summary>
		public NoneOfTags(params string[] tags) : base(tags) {
		}

		internal override bool matches(GameObjectWrapper gameObjectWrapper){
			// Prevent user error if a GameObject is destroyed while it is still binded. It's a mistake, the user has to unbind game objects before destroying them.
			if (gameObjectWrapper._gameObject != null){
				string gameObjectTag = gameObjectWrapper._gameObject.tag;

				for(int i = 0; i < _tags.Length; ++i){
					if(gameObjectTag == _tags [i]){
						return false;
					}
				}
				return true;
			} else
				return false;
		}
	}
}