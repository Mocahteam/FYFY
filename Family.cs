﻿using UnityEngine;
using System.Collections.Generic;

public class Family : IEnumerable<GameObject> {
	internal readonly string _descriptor;
	internal readonly HashSet<int> _entitiesIds;
	internal readonly Matcher[] _matchers;

	internal Family(string descriptor, Matcher[] matchers){
		_descriptor = descriptor;
		_entitiesIds = new HashSet<int> ();
		_matchers = matchers;
	}

	public string Descriptor { get { return _descriptor; } }
	public int Count { get { return _entitiesIds.Count; } }

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator(){
		return this.GetEnumerator();
	}

	public IEnumerator<GameObject> GetEnumerator(){
		foreach (int id in _entitiesIds) {
			GameObject go = EntityManager._entities[id]._gameObject;
			yield return go;
		}
	}
}

// Famille -> signaler quand entrée / sortie dans la famille, car on veut pe qu'un systeme travaille sur une famille quand une entite rentre ou sort
// property vs autoproperty
// equivalent to _entityAdded : Signal in Genome
// internal marche vraiment uniquement dans la dll ??