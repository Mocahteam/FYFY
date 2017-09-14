using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System;
using System.Linq;


namespace monitoring{
	public class MonitoringManager : MonoBehaviour {

		public static class Source {
			public static string SYSTEM = "system";
			public static string PLAYER = "player";
		};

        public string filename;

        public void Start()
        {
            GeneratePNandSpecifs ();
        }

        public void OnDestroy()
        {
            XmlHandler.saveTraces(filename);
        }

        private void GeneratePNandSpecifs () {
			// Build final PetriNet
			PetriNet petriNet = new PetriNet ();

			float offsetX = 0;

			// Fill final PN
			foreach (ComponentMonitoring monitor in FindObjectsOfType<ComponentMonitoring> ()) {
                // Check if PN exists
				if (monitor.PetriNet != null) {
					// Make a copy of local PN in order to organise it spatially without changing original PN
					PetriNet tmpPN = new PetriNet(monitor.PetriNet, monitor.gameObject.name);
					tmpPN.addWidth (offsetX);
					petriNet.addSubNet (tmpPN);
                    
                    // Process links
                    foreach (TransitionLink transitionLink in monitor.transitionLinks)
                    {
						// Make a copy of current transition and prefix its name with its game object name
						Node curTransition_copy = new Node(transitionLink.transition);
						curTransition_copy.label = monitor.gameObject.name+"_"+curTransition_copy.label;
						// Add this transition to Specifications
						XmlHandler.addSpecif(curTransition_copy.label+"_"+monitor.id, transitionLink.isSystemAction, transitionLink.isEndAction);
                        Node oldTransition = curTransition_copy;
                        if (isNullOrWhiteSpace(transitionLink.logic))
                        {
							// Default : And of all link
							foreach(Link curLink in transitionLink.links)
                            {
								if (curLink.linkedObject != null) {
									// Make a copy of linked place and prefix its name with its game object name
									Node linkedPlace = curLink.getPlaceFromLinkedObject (curLink.placeId);
									if (linkedPlace != null) {
										Node linkedPlace_copy = new Node (linkedPlace);
										linkedPlace_copy.label = curLink.linkedObject.name + "_" + linkedPlace_copy.label;
										// Define arc type
										ArcType arcType = curLink.type == 2 ? Arc.stringToArcType (Arc.optType.ElementAt (curLink.flagsType)) : ArcType.regular;
										// Create arc between Transition and linked place (depends on Get/Produce/Require diffusion state)
										petriNet.arcs.Add (curLink.type != 1 ? new Arc (linkedPlace_copy, curTransition_copy, arcType, curLink.weight) : new Arc (curTransition_copy, linkedPlace_copy, arcType, curLink.weight));
									}
								}
                            }
                        }
                        else
                        {
							ExpressionParser expr_parser = new ExpressionParser();
                            if (expr_parser.isValid(transitionLink))
                            {
								// Logic expression is valid

								// Distribute expression
								string[] exp = expr_parser.getDistribution(transitionLink.logic);

	                            int or = 0;

								// Parse distributed expression
								foreach (string token in exp)
	                            {
									// Check if current token is an operator
	                                if (!token.Equals("+") && !token.Equals("*"))
	                                {
										// It's not an operator => we load the link
										Link curLink = transitionLink.getLabeledLink(token);
										if (curLink.linkedObject != null) {
											// Make a copy of linked place and prefix its name with its game object name
											Node linkedPlace = curLink.getPlaceFromLinkedObject (curLink.placeId);
											if (linkedPlace != null) {
												Node linkedPlace_copy = new Node (linkedPlace);
												linkedPlace_copy.label = curLink.linkedObject.name + "_" + linkedPlace_copy.label;
												// Define arc type
												ArcType arcType = curLink.type == 2 ? Arc.stringToArcType (Arc.optType.ElementAt (curLink.flagsType)) : ArcType.regular;
												// Create arc between Transition and linked place (depends on Get/Produce/Require diffusion state)
												petriNet.arcs.Add (curLink.type != 1 ? new Arc (linkedPlace_copy, curTransition_copy, arcType, curLink.weight) : new Arc (curTransition_copy, linkedPlace_copy, arcType, curLink.weight));
											}
										}
	                                }
	                                else if (token.Equals("+"))
	                                {
										// We detect OR operator => add a new transition and set it as current node

	                                    // Build new transition, we keep old transition to build links after
										// Add offset to position
										curTransition_copy.position.x += offsetX;
										curTransition_copy.position.y += 50;
										curTransition_copy = new Node("or" + (or++) + "_" + oldTransition.label, curTransition_copy.id, curTransition_copy.offset, curTransition_copy.initialMarking, curTransition_copy.position);
										// Add this new transition to PN
	                                    petriNet.transitions.Add(curTransition_copy);
										// and to specifications
										XmlHandler.addSpecif(curTransition_copy.label+"_"+monitor.id, transitionLink.isSystemAction, transitionLink.isEndAction);
										// Duplicate arcs from old transition
	                                    foreach (Arc a in tmpPN.getConcernedArcs(oldTransition))
	                                    {
	                                        if (a.target.label.Equals(oldTransition.label))
	                                        {
												petriNet.arcs.Add(new Arc(a.source, curTransition_copy, a.type, a.weight));
	                                        }
	                                        else if (a.source.label.Equals(oldTransition.label))
	                                        {
												petriNet.arcs.Add(new Arc(curTransition_copy, a.target, a.type, a.weight));
	                                        }
	                                    }
	                                }
								}
							}
							else
								Debug.LogError("Petri Net Building aborted: Logic expression of \""+transitionLink.transition.label+"\" action from \""+monitor.gameObject.name+"\" is not valid => \""+transitionLink.logic+"\". Please check it from Monitor edit view.");
                        }
                    }
					offsetX += monitor.PetriNet.getWidth ()+50; // Add spaces between PN
				}
			}
				
			PnmlParser.SaveAtPath (petriNet, filename+".pnml");
			XmlHandler.saveSpecifications(filename);
        }

        private static bool isNullOrWhiteSpace(string str)
        {
            return string.IsNullOrEmpty(str) || onlySpaces(str);
        }

        private static bool onlySpaces(string str)
        {
           foreach(char c in str)
            {
                if (!char.IsWhiteSpace(c))
                    return false;
            }
            return true;
        }
    }
}