namespace FYFY {
	public class AnyOfLayers : LayerMatcher {
		public AnyOfLayers(params int[] layers) : base(layers) {
		}

		internal override bool matches(GameObjectWrapper gameObjectWrapper){
			int gameObjectLayer = gameObjectWrapper._gameObject.layer;

			for (int i = 0; i < _layers.Length; ++i)
				if(gameObjectLayer == _layers[i])
					return true;
			return false;
		}
	}
}