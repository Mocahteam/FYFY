namespace FYFY {
	/// <summary>
	/// 	Base class every matcher working on <c>Layer</c> derives from.
	/// </summary>
	public abstract class LayerMatcher : Matcher {
		/// <summary>
		/// 	Targets used to match.
		/// </summary>
		protected readonly int[] _layers;

		internal LayerMatcher(params int[] layers) {
			if(layers.Length == 0) {
				throw new System.ArgumentException("It is not allowed to provide a LayerMatcher without at least one layer defined.");
			}

			System.Array.Sort(layers);

			System.Text.StringBuilder descriptor = new System.Text.StringBuilder(this.GetType() + ":" + layers[0]);
			for(int i = 1; i < layers.Length; ++i){
				descriptor.AppendFormat("/{0}", layers[i]);
			}

			_descriptor = descriptor.ToString();
			_layers = layers;
		}
	}
}