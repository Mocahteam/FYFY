﻿namespace FYFY {
	/// <summary>
	///		Matcher used to exclude all the <c>GameObjects</c> which are in
	///		one of layers of a specific list of layers.
	/// </summary>
	public class NoneOfLayers : LayerMatcher {
		/// <summary>
		/// 	Initializes a new instance of the <see cref="FYFY.NoneOfLayers"/> class.
		/// </summary>
		public NoneOfLayers(params int[] layers) : base(layers) {
		}

		internal override bool matches(GameObjectWrapper gameObjectWrapper){
			// Prevent user error if a GameObject is destroyed while it is still binded. It's a mistake, the user has to unbind game objects before destroying them.
			if (gameObjectWrapper._gameObject != null){
				int gameObjectLayer = gameObjectWrapper._gameObject.layer;

				for(int i = 0; i < _layers.Length; ++i){
					if(gameObjectLayer == _layers[i]){
						return false;
					}
				}
				return true;
			} else
				return false;
		}
	}
}