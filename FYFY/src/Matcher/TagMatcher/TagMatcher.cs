﻿using System.Linq;

namespace FYFY {
	public abstract class TagMatcher : Matcher {
		protected readonly string[] _tags;

		public TagMatcher(params string[] tags) {
			if(tags.Length == 0) {
				throw new System.ArgumentNullException ();
			}
			
			System.Array.Sort(tags);

			System.Text.StringBuilder descriptor = new System.Text.StringBuilder(this.GetType() + ":" + tags[0]);
			for (int i = 1; i < tags.Length; ++i) {
				string tag = tags[i];
				if (tag == null) {
					throw new System.ArgumentNullException ();
				}

				descriptor.AppendFormat ("/{0}", tag);
			}

			_tags = tags;
			_descriptor = descriptor.ToString();
		}
	}
}