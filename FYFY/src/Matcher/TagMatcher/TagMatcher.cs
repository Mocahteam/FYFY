using System.Linq;

namespace FYFY {
	/// <summary>
	/// 	Base class every matcher working on <c>Tag</c> derives from.
	/// </summary>
	public abstract class TagMatcher : Matcher {
		/// <summary>
		/// 	Targets used to match.
		/// </summary>
		protected readonly string[] _tags;

		internal TagMatcher(params string[] tags) {
			if(tags.Length == 0) {
				throw new System.ArgumentNullException();
			}
			
			System.Array.Sort(tags);

			System.Text.StringBuilder descriptor = new System.Text.StringBuilder(this.GetType() + ":" + tags[0]);
			for(int i = 1; i < tags.Length; ++i) {
				string tag = tags[i];
				if (tag == null) {
					throw new System.ArgumentNullException();
				}

				descriptor.AppendFormat("/{0}", tag);
			}

			_tags = tags;
			_descriptor = descriptor.ToString();
		}
	}
}