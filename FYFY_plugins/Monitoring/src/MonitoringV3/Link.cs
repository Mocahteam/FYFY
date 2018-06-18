using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FYFY_plugins.Monitoring{
	/// <summary>
	/// Contains Link data
	/// </summary>
	[Serializable]
	public class Link {

		/// <summary>Link label used in logic expression.</summary>
		public string label;
		/// <summary>The type of this link encoded as an int: 0 means Get, 1 means Produce, 2 means Require.</summary>
		public int type = 2;
		/// <summary>The flag type of this link encoded as an int: 0 means "at least", 1 means "less than".</summary>
		public int flagsType = 0;
		/// <summary>Link weight.</summary>
		public int weight = 1;
		/// <summary>Place Id associated to this link.</summary>
		public int placeId;
		/// <summary>The current scrolling value for this link.</summary>
		public Vector2 scroll = new Vector2(); // Updated by FYFY_plugins.Monitoring_Inspector.EditionView
		/// <summary>The Game Object associated to this link.</summary>
		public GameObject linkedObject;

		internal Link(string label, int type, int placeId, GameObject obj, int weight)
        {
            this.label = label;
			this.type = type;
            this.placeId = placeId;

			this.linkedObject = obj;
            this.weight = weight;
        }
		
		internal Link (Link template){
			this.label = template.label;
			this.type = template.type;
			this.flagsType = template.flagsType;
			this.weight = template.weight;
			this.placeId = template.placeId;
			this.scroll = template.scroll;
			this.linkedObject = template.linkedObject;
		}
		
		/// <summary>Initializes a new instance of the <see cref="FYFY_plugins.Monitoring.Link"/> class.</summary>
        public Link()
        {
        }

		/// <summary>Returns the list of places' name included into the linked Game Object.</summary>
		public string [] getPlacesNameFromLinkedObject (){
			List<string> places = new List<string> ();
			if (linkedObject != null){
				foreach (ComponentMonitoring m in linkedObject.GetComponents<ComponentMonitoring> ()) {
					if (m.PetriNet != null && m.PnmlFile != null){
						foreach (string newItem in m.PetriNet.getPlacesNames ())
							places.Add (newItem+" ("+m.PnmlFile.name+")");
					}
				}
			}
			return places.ToArray();
		}

		internal Node getPlace (){
			int i = placeId;
			if (linkedObject != null) {
				foreach (ComponentMonitoring m in linkedObject.GetComponents<ComponentMonitoring> ()) {
					if (m.PetriNet != null) {
						if (m.PetriNet.places.Count <= i)
							i -= m.PetriNet.places.Count;
						else
							return m.PetriNet.places[i];
					}
				}
			}
			return null;
		}
		
		internal bool isCompatibleWithPnName(int fullPnId){
			int i = placeId;
			if (linkedObject != null) {
				foreach (ComponentMonitoring m in linkedObject.GetComponents<ComponentMonitoring> ()) {
					if (m.PetriNet != null) {
						if (m.PetriNet.places.Count <= i || m.fullPnSelected != fullPnId)
							i -= m.PetriNet.places.Count;
						else
							return i >= 0;
					}
				}
				return false;
			} else
				return true;
		}
    }
}