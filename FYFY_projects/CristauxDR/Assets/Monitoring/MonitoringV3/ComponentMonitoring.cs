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
        public Dictionary<string, traceFunc> traceHelper = new Dictionary<string, traceFunc>();
        
		public delegate void initFunc(int nbToken);
		// Dictionnary containing available places available for initialization
		public Dictionary<string, initFunc> initHelper = new Dictionary<string, initFunc>();

        //[HideInInspector]
        public UnityEngine.Object PnmlFile;

		public string comments;

		//[HideInInspector]
		public int id = -1;

		//[HideInInspector] //It's perhaps intersting to know the PN into inspector?
		public PetriNet petriNet;

		//[HideInInspector] 
		public List<TransitionLink> transitionLinks = new List<TransitionLink>();

		// Usefull to detect copies in order to generate new ID
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

        public void Start()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
#endif
				// Make association between transitions name and trace function
				foreach(Node transition in petriNet.transitions)
				{
					TraceHelper traceur = new TraceHelper(transition.label, this);
					traceHelper.Add(transition.label,traceur.trace);
				}
				// Make association between places name and trace function
				foreach(Node place in petriNet.places)
				{
					InitHelper initializer = new InitHelper(place.label, this);
					initHelper.Add(place.label, initializer.init);
				}
#if UNITY_EDITOR
            }
#endif            
        }


        public void debugTest()
        {
            GameObject cle1 = GameObject.Find("Cle1");
            GameObject cle2 = GameObject.Find("Cle2");
            GameObject cle3 = GameObject.Find("Cle3");
            GameObject cle4 = GameObject.Find("Cle4");
            traceHelper["ouvrir"]("player");
            traceHelper["ouvrir"]("player",cle1);
            traceHelper["deverouiller"]("player");
            traceHelper["deverouiller"]("player",cle2);
            traceHelper["deverouiller"]("player",cle2,cle1);
            traceHelper["deverouiller"]("player",cle1,cle2);
            traceHelper["deverouiller"]("player",cle1, cle3);
            traceHelper["deverouiller"]("player",cle4, cle2);
            traceHelper["deverouiller"]("player",cle4);
            traceHelper["deverouiller"]("player",cle3);
            traceHelper["deverouiller"]("player",cle4, cle3);
        }
#if UNITY_EDITOR

        //TODO: Move this code somewhere else to disconnect editor from go
        void Awake()
        {
            if (!Application.isPlaying && (id == -1 || !IDGenerator.isAviable(id)))
            {
                if (instanceID != GetInstanceID())
                {
                    if (instanceID == 0)
                    {
                        instanceID = GetInstanceID();
                        id = getID();
                        if (PnmlFile != null)
                            petriNet = PetriNet.loadFromFile(AssetDatabase.GetAssetPath(PnmlFile), id);
                    }
                    else
                    {
                        instanceID = GetInstanceID();
                        if (instanceID < 0)
                        {
                            id = getID();
                            if(PnmlFile != null)
                                petriNet = PetriNet.loadFromFile(AssetDatabase.GetAssetPath(PnmlFile), id);
                        }
                    }
                }

            }
        }

        internal void loadTransitionsLinks()
        {
            if (transitionLinks.Count != petriNet.transitions.Count)
            {
                foreach (Node transition in petriNet.transitions)
                {
                    TransitionLink t = new TransitionLink();
                    t.transition = transition;
                    transitionLinks.Add(t);
                }
            }
        }
#endif
    }
}