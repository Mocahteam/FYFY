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
			string gameObjectTag = gameObjectWrapper._gameObject.tag;

			for(int i = 0; i < _tags.Length; ++i){
				if(gameObjectTag == _tags [i]){
					return false;
				}
			}
			return true;
		}
	}
}