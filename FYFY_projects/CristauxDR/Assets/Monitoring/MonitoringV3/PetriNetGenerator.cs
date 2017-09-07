using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using petriNetV2;
using System;
using System.Linq;


namespace monitorV3{
	public class PetriNetGenerator : MonoBehaviour {

        public string filename;

        public void Start()
        {
            GeneratePN();
        }

        public void OnDestroy()
        {
            TraceHandler.save(filename);
        }

        void GeneratePN () {
			// Build final PetriNet
			PetriNet petriNet = new PetriNet ();

			float offsetX = 0;

			// Fill final PN
			foreach (ComponentMonitoring monitor in FindObjectsOfType<ComponentMonitoring> ()) {
                // Check if PN exists
				if (monitor.petriNet != null) {
					// Make a copy of local PN in order to organise
					PetriNet tmpPN = new PetriNet(monitor.petriNet, monitor.gameObject.name);
					tmpPN.addWidth (offsetX);
					petriNet.addSubNet (tmpPN);
                    
                    // Process links
                    foreach (TransitionLink transitionLink in monitor.transitionLinks)
                    {
                        Node curTransition = transitionLink.transition;
                        Node oldTransition = curTransition;
                        if (isNullOrWhiteSpace(transitionLink.logic))
                        {
							// Default : And of all link
							foreach(Link curLink in transitionLink.links)
                            {
                                // Get linked place
								Node linkedPlace = curLink.objLink.GetComponent<ComponentMonitoring>().petriNet.places.ElementAt(curLink.placeId);
								// Define arc type
								ArcType arcType = curLink.diffusion ? ArcType.regular : Arc.stringToArcType(Arc.optType.ElementAt(curLink.flagsType));
								// Create arc between Transition and linked place (depends on Produce/Require diffusion state)
								petriNet.arcs.Add(curLink.diffusion ? new Arc(curTransition, linkedPlace, arcType, curLink.poids) : new Arc(linkedPlace, curTransition, arcType, curLink.poids));
                            }
                        }
                        else
                        {
                            AriParser arip = new AriParser();
                            if (arip.validAri(transitionLink))
                            {
								// Logic expression is valid

								// Distribute expression
	                            string[] ari = arip.getDistribution(transitionLink.logic);

	                            int or = 0;

								// Parse distributed expression
								foreach (string token in ari)
	                            {
									// Check if current token is an operator
	                                if (!token.Equals("+") && !token.Equals("*"))
	                                {
										// It's not an operator => we load the link
										Link curLink = transitionLink.getLabeledLink(token);
	                                    // Get linked place
										Node linkedPlace = curLink.objLink.GetComponent<ComponentMonitoring>().petriNet.places.ElementAt(curLink.placeId);
										// Define arc type
										ArcType arcType = curLink.diffusion ? ArcType.regular : Arc.stringToArcType(Arc.optType.ElementAt(curLink.flagsType));
										// Add new arc
										petriNet.arcs.Add(curLink.diffusion ? new Arc(curTransition, linkedPlace, arcType, curLink.poids) : new Arc(linkedPlace, curTransition, arcType, curLink.poids));
	                                }
	                                else if (token.Equals("+"))
	                                {
										// We detect OR operator => add a new transition and set it as current node

	                                    // Build new transition, we keep old transition to build links after
										curTransition = new Node("or" + (or++) + "_" + monitor.gameObject.name + "_" + oldTransition.label, curTransition.id, curTransition.offset, curTransition.initialMarking, curTransition.position);
										// Add this new transition to PN
	                                    petriNet.transitions.Add(curTransition);
										// Duplicate arcs from ald transition
	                                    foreach (Arc a in tmpPN.getConcernedArcs(oldTransition))
	                                    {
	                                        if (a.target.label.Equals(oldTransition.label))
	                                        {
	                                            petriNet.arcs.Add(new Arc(a.source, curTransition, a.type, a.poid));
	                                        }
	                                        else if (a.source.label.Equals(oldTransition.label))
	                                        {
	                                            petriNet.arcs.Add(new Arc(curTransition, a.target, a.type, a.poid));
	                                        }
	                                    }
	                                }
								}
							}
							else
							{
								Debug.LogError("PetriNetGenerator: Logic expression not valid");
							}
                        }
                    }
					offsetX += monitor.petriNet.getWidth ();
				}
			}
				
			PnmlParser.SaveAtPath (petriNet, filename+".pnml");
            TraceHandler.saveTransitionsLabels(filename, petriNet.transitions);
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