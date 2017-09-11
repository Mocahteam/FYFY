using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using petriNetV2;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace monitorV3{
	/// <summary>
	/// Composant à attacher au gameObject pour gérer sa représentation par un rdp
	/// </summary>
	[Serializable]
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    public class ComponentMonitoring : MonoBehaviour
    {

        public delegate void traceFunc(string system ,params GameObject[] objects);
		// Dictionnary containing available traces
        public Dictionary<string, traceFunc> traceHelper = null;
        
		public delegate void initFunc(int nbToken);
		// Dictionnary containing available places available for initialization
		public Dictionary<string, initFunc> initHelper = null;

        [HideInInspector]
		[SerializeField]
		private UnityEngine.Object pnmlFile;
		[HideInInspector]
		public UnityEngine.Object PnmlFile{
			get { return pnmlFile; }
			set {
				pnmlFile = value;
				if (value == null) {
					transitionLinks.Clear ();
					petriNet = null;
				} else {
					// Load petriNet
					petriNet = PetriNet.loadFromFile (AssetDatabase.GetAssetPath (PnmlFile), id);
					// Reset transitionLinks
					transitionLinks.Clear ();
					foreach (Node transition in petriNet.transitions)
					{
						TransitionLink t = new TransitionLink();
						t.transition = transition;
						transitionLinks.Add(t);
					}
				}
			}
		}

		[HideInInspector]
		public string comments;

		[HideInInspector]
		public int id = -1;

		//[HideInInspector] //It's perhaps intersting to know the PN into inspector?
		public PetriNet petriNet;

		[HideInInspector] 
		public List<TransitionLink> transitionLinks = new List<TransitionLink>();

		// Usefull to detect copies in order to generate new ID
		[HideInInspector]
		[SerializeField]
		private int instanceID = 0;

        //Fonctions utilitaires :
        public int getID(){
			id = IDGenerator.genID ();
			return id;
		}

        public TransitionLink getTransitionLinkByTransitionLabel(String label)
        {
			//Debug.Log("tc count : "+transitionLinks.Count);
            foreach(TransitionLink tc in transitionLinks)
            {
                //Debug.Log("getByLabel : "+tc.label);
                if (tc.transition.label.Equals(label))
                    return tc;
            }
            return null;
        }

        string printList(List<int> list)
        {
            string str = "";
            foreach (int i in list)
            {
                str += (i + " ");
            }
            //Debug.Log("List : " + str);
            return str;
        }

#if UNITY_EDITOR

        void Awake()
        {
			// Need to init Dictionaries here because Dictionaries are'nt serializable
			// Reset helpers
			traceHelper = new Dictionary<string, traceFunc>();
			initHelper = new Dictionary<string, initFunc>();
			if (petriNet != null) {
				// Make association between transitions name and trace function
				foreach (Node transition in petriNet.transitions) {
					TraceHelper traceur = new TraceHelper (transition.label, this);
					traceHelper.Add (transition.label, traceur.trace);
				}
				// Make association between places name and trace function
				foreach (Node place in petriNet.places) {
					InitHelper initializer = new InitHelper (place.label, this);
					initHelper.Add (place.label, initializer.init);
				}
			}

			// Possible when stop playing and come back in edit mode
			// TODO: Move this code somewhere else to disconnect editor from go
            if (!Application.isPlaying && (id == -1 || !IDGenerator.isAviable(id)))
			{
                if (instanceID != GetInstanceID())
                {
                    if (instanceID == 0)
                    {
                        instanceID = GetInstanceID();
                        id = getID();
                    }
                    else
                    {
                        instanceID = GetInstanceID();
                        if (instanceID < 0)
                            id = getID();
                    }
                }

            }
        }
#endif
    }
}