using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Linq;
using System.Linq;

namespace monitoring{

	/*
	 * See XDocument class documentation
	 * Doc : 
	 * 
	 * */

	public class PnmlParser {

		/// <summary>
		/// Namespace defined to read pnml file format
		/// </summary>
		private static XNamespace ns = XNamespace.Get("http://www.pnml.org/version-2009/grammar/pnml"); 

		/// <summary>
		/// Create a PetriNet class whithin data filled from a pnml file 
		/// </summary>
		/// <returns>PetriNet or null if doesn't exists</returns>
		/// <param name="path">Full path of the pnml file</param>
		public static PetriNet loadFromPath(string path,int id){
			Dictionary<string,Node> idNodeMatcher = new Dictionary<string,Node> ();
			//New PN
			PetriNet petriNet = new PetriNet ();

			//Read file
			XDocument xDoc = XDocument.Load (path);

			//Load Header
			petriNet.label = xDoc.Element ("{" + ns + "}pnml").Element ("{" + ns + "}net").Element ("{" + ns + "}name").Value;

			//Load Transitions
			IEnumerable<XElement> transitions = xDoc.Descendants ("{" + ns + "}transition");

			foreach (XElement transition in transitions) {

				Node newTransition = new Node (
					transition.Element ("{" + ns + "}name").Element ("{" + ns + "}text").Value,// + "_" + petriNet.Label,// + "_" +id,
					new Vector2 (float.Parse (transition.Element ("{" + ns + "}name").Element ("{" + ns + "}graphics").Element ("{" + ns + "}offset").Attribute ("x").Value), float.Parse (transition.Element ("{" + ns + "}name").Element ("{" + ns + "}graphics").Element ("{" + ns + "}offset").Attribute ("y").Value)),
					-1,
					new Vector2 (float.Parse (transition.Element ("{" + ns + "}graphics").Element ("{" + ns + "}position").Attribute ("x").Value), float.Parse (transition.Element ("{" + ns + "}graphics").Element ("{" + ns + "}position").Attribute ("y").Value))
				);

				idNodeMatcher.Add (transition.Attribute ("id").Value, newTransition);
				petriNet.transitions.Add (newTransition);
			}

			//Load Places
			IEnumerable<XElement> places = xDoc.Descendants ("{" + ns + "}place");

			foreach(XElement place in places){

                Node newPlace = new Node(
                    place.Element("{" + ns + "}name").Element("{" + ns + "}text").Value,// + "_" + petriNet.Label,// + "_" +id,
					new Vector2 (float.Parse (place.Element ("{" + ns + "}name").Element ("{" + ns + "}graphics").Element ("{" + ns + "}offset").Attribute ("x").Value), float.Parse (place.Element ("{" + ns + "}name").Element ("{" + ns + "}graphics").Element ("{" + ns + "}offset").Attribute ("y").Value)),
					place.Element ("{" + ns + "}initialMarking") != null ? int.Parse (place.Element ("{" + ns + "}initialMarking").Element ("{" + ns + "}text").Value) : 0,
					new Vector2 (float.Parse (place.Element ("{" + ns + "}graphics").Element ("{" + ns + "}position").Attribute ("x").Value), float.Parse (place.Element ("{" + ns + "}graphics").Element ("{" + ns + "}position").Attribute ("y").Value))
				);

				idNodeMatcher.Add(place.Attribute("id").Value,newPlace);
				petriNet.places.Add (newPlace);
			}

			//Load Arcs
			IEnumerable<XElement> arcs = xDoc.Descendants ("{" + ns + "}arc");

			foreach(XElement arc in arcs){

				Node source; idNodeMatcher.TryGetValue( arc.Attribute ("source").Value, out source);
				Node target; idNodeMatcher.TryGetValue( arc.Attribute ("target").Value, out target);

				Arc newArc = new Arc (source,
					             target,
								arc.Element ("{" + ns + "}type") != null ? Arc.stringToArcType(arc.Element ("{" + ns + "}type").Attribute ("value").Value) : ArcType.regular,
					             arc.Element ("{" + ns + "}inscription") != null ? int.Parse (arc.Element ("{" + ns + "}inscription").Element ("{" + ns + "}text").Value) : 1
				             );
				petriNet.arcs.Add (newArc);
			}
		
			return petriNet;
		}

		public static void SaveAtPath(PetriNet pn,string path){

			List<XElement> xelemList = new List<XElement> ();
			XDocument doc = new XDocument ();
			foreach(Node transition in pn.transitions){
				xelemList.Add ( 
					new XElement(ns+"transition",new XAttribute("id",transition.label+"_"+transition.id),
						new XElement(ns+"name",
							new XElement(ns+"text",transition.label + "_" + transition.id),
							new XElement(ns+"graphics",
								new XElement(ns+"offset",new XAttribute("x",transition.offset.x),new XAttribute("y",transition.offset.y))
							)
						),
						new XElement(ns+"graphics",
							new XElement(ns+"position",new XAttribute("x",transition.position.x),new XAttribute("y",transition.position.y))
						)
					)

				);		
			}

			foreach(Node place in pn.places){
				xelemList.Add ( 
					new XElement(ns+"place",new XAttribute("id",place.label + "_" + place.id),
						new XElement(ns+"name",
							new XElement(ns+"text",place.label + "_" + place.id),
							new XElement(ns+"graphics",
								new XElement(ns+"offset",new XAttribute("x",place.offset.x),new XAttribute("y",place.offset.y))
							)
						),
						new XElement(ns+"initialMarking",
							new XElement(ns+"text",place.initialMarking)
						),
						new XElement(ns+"graphics",
							new XElement(ns+"position",new XAttribute("x",place.position.x),new XAttribute("y",place.position.y))
						)
					)

				);

			}

			int cptNodes = pn.places.Count + pn.transitions.Count;
//			Debug.Log (pn.arcs);
			foreach(Arc arc in pn.arcs){
				xelemList.Add ( 
					new XElement(ns+"arc",new XAttribute("id",++cptNodes),new XAttribute("source",arc.source.label+"_"+ arc.source.id),new XAttribute("target",arc.target.label+"_"+arc.target.id),
						new XElement(ns+"inscription",
							new XElement(ns+"text",arc.weight),
							new XElement(ns+"graphics",
								new XElement(ns+"offset",new XAttribute("x",0),new XAttribute("y",-10))
							)
						),
						new XElement(ns+"type",new XAttribute("value",arc.type))
					)

				);
			}

			doc = new XDocument(
				new XElement(ns+"pnml",
					new XElement(ns+"net", new XAttribute("id","A-A-0"),new XAttribute("type","http://www.laas.fr/tina/tpn"),
						new XElement(ns+"name",
							new XElement(ns+"text","gen0")
						),

						new XElement(ns+"page",new XAttribute("id","A-A-1"),
							xelemList.Select(x => x)
						)
					)
				)
			);
            System.IO.Directory.CreateDirectory("completeNet");
            doc.Save ("completeNet\\"+path);
		}


	}
}
