using System.Linq;

namespace FYFY {
	public abstract class TagMatcher : Matcher {
		protected readonly string[] _tags;

		public TagMatcher(params string[] tags) {
			if(tags == null || tags.Length == 0 || tags.Contains(null) == true)
				throw new System.ArgumentNullException();
			
			System.Array.Sort(tags);

			System.Text.StringBuilder descriptor = new System.Text.StringBuilder(this.GetType() + ":" + tags[0]);
			for (int i = 1; i < tags.Length; ++i)
				descriptor.AppendFormat("/{0}", tags[i]);

			_tags = tags;
			_descriptor = descriptor.ToString();
		}
	}
}