namespace FYFY {
	/// <summary>
	///		Matcher used to exclude all the <c>GameObjects</c> which are in
	///		one of layers of a specific list of layers.
	/// </summary>
	public class NoneOfLayers : LayerMatcher {
		public NoneOfLayers(params int[] layers) : base(layers) {
		}

		internal override bool matches(GameObjectWrapper gameObjectWrapper){
			int gameObjectLayer = gameObjectWrapper._gameObject.layer;

			for(int i = 0; i < _layers.Length; ++i){
				if(gameObjectLayer == _layers[i]){
					return false;
				}
			}
			return true;
		}
	}
}