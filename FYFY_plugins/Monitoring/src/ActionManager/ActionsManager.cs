using UnityEngine;
using FYFY;
using System;
using System.IO;

namespace FYFY_plugins.Monitoring
{
    /// <summary>
    /// This system processes <c>ActionPerformed</c> components and uses <c>MonitoringManager</c> functions to build traces.
    /// </summary>
    public class ActionsManager : FSystem
    {

        private Family f_actions = FamilyManager.getFamily(new AllOfComponents(typeof(ActionPerformed)));

        /// <summary>
        /// As a singleton, this system can be accessed through this static instance.
        /// </summary>
        public static ActionsManager instance;

        private GameObject traces;
        private string tmpString;

        private string tmpPerformer;
        private string[] tmpLabels;

        /// <summary>
        /// The contructor of this system.
        /// </summary>
        public ActionsManager()
        {
            if (Application.isPlaying)
            {
                traces = new GameObject("Traces");
                GameObjectManager.bind(traces);
            }
            instance = this;
        }
		

        /// <summary>
        /// Use to process your families.
        /// </summary>
		protected override void onProcess(int familiesUpdateCount)
        {
            int nbActions = f_actions.Count;
            for (int i = 0; i < nbActions; i++)
                ActionProcessing(f_actions.getAt(i));
        }

        private void ActionProcessing(GameObject go)
        {
            if (!this.Pause)
            {
                ActionPerformed[] listAP = go.GetComponents<ActionPerformed>();
                int nb = listAP.Length;
                ActionPerformed ap;
                for (int j = 0; j < nb; j++)
                {
                    ap = listAP[j];
                    ComponentMonitoring cMonitoring = null;
                    string tmpActionName = "";
                    if (ap.name != "" && ap.overrideName != "")
                    {
                        //look for the ComponentMonitoring corresponding to name and overridename
                        int matched = 0;
                        foreach (ComponentMonitoring cm in go.GetComponents<ComponentMonitoring>())
                        {
                            foreach (TransitionLink tl in cm.transitionLinks)
                            {
                                if (tl.transition.label == ap.name && tl.transition.overridedLabel == ap.overrideName)
                                {
                                    if(matched == 0)
                                    {
                                        cMonitoring = cm;
                                        tmpActionName = ap.name;
                                        tmpString = cm.id.ToString();
                                    }
                                    else
                                        tmpString = string.Concat(tmpString, ", ", cm.id);
                                    matched++;
                                }
                            }
                            if (matched > 1)
                            {
                                Debug.LogException(new WarningException(string.Concat("Several ComponentMonitoring on ", go.name, " are matching the name \"", ap.name, "\" and the overrideName \"", ap.overrideName, "\". By default, the first one found is used to trace.",
                                    Environment.NewLine, "The ", matched, " corresponding are: ", tmpString), ap.exceptionStackTrace));
                                break;
                            }
                        }
                        if (matched == 0 && ap.family == null)
                        {
                            throw new InvalidTraceException(string.Concat("Unable to trace action on \"", go.name, "\" because the name \"", ap.name, "\" and the overrideName \"", ap.overrideName, "\" in its ActionPerformed don't correspond to any ComponentMonitoring."), ap.exceptionStackTrace);
                        }
                    }
                    else if (ap.name != "")
                    {
                        //look for the ComponentMonitoring corresponding to the name
                        int matched = 0;
                        foreach (ComponentMonitoring cm in go.GetComponents<ComponentMonitoring>())
                        {
                            foreach (TransitionLink tl in cm.transitionLinks)
                            {
                                if (tl.transition.label == ap.name)
                                {
                                    if (matched == 0)
                                    {
                                        cMonitoring = cm;
                                        tmpActionName = ap.name;
                                        tmpString = cm.id.ToString();
                                    }
                                    else
                                        tmpString = string.Concat(tmpString, ", ", cm.id);
                                    matched++;
                                }
                            }
                            if (matched > 1)
                            {
                                Debug.LogException(new WarningException(string.Concat("Several ComponentMonitoring on ", go.name, " are matching the name \"", ap.name, "\". By default, the first one found is used to trace.",
                                    Environment.NewLine, "The ", matched, " corresponding are: ", tmpString), ap.exceptionStackTrace));
                                break;
                            }
                        }
                        if (matched == 0 && ap.family == null)
                        {
                            throw new InvalidTraceException(string.Concat("Unable to trace action on \"", go.name, "\" because the name \"", ap.name, "\" in its ActionPerformed doesn't correspond to any ComponentMonitoring."), ap.exceptionStackTrace);
                        }
                    }
                    else if (ap.overrideName != "")
                    {
                        //look for the ComponentMonitoring corresponding to the overridename
                        int matched = 0;
                        foreach (ComponentMonitoring cm in go.GetComponents<ComponentMonitoring>())
                        {
                            foreach (TransitionLink tl in cm.transitionLinks)
                            {
                                if (tl.transition.overridedLabel == ap.overrideName)
                                {
                                    if (matched == 0)
                                    {
                                        cMonitoring = cm;
                                        tmpActionName = tl.transition.label;
                                        tmpString = cm.id.ToString();
                                    }
                                    else
                                        tmpString = string.Concat(tmpString, ", ", cm.id);
                                    matched++;
                                }
                            }
                            if (matched > 1)
                            {
                                Debug.LogException(new WarningException(string.Concat("Several ComponentMonitoring on ", go.name, " are matching the overrideName \"", ap.overrideName, "\". By default, the first one found is used to trace.",
                                    Environment.NewLine, "The ", matched, " corresponding are: ", tmpString), ap.exceptionStackTrace));
                                break;
                            }
                        }
                        if (matched == 0 && ap.family == null)
                        {
                            throw new InvalidTraceException(string.Concat("Unable to trace action on \"", go.name, "\" because the overrideName \"", ap.overrideName, "\" in its ActionPerformed don't correspond to any ComponentMonitoring."), ap.exceptionStackTrace);
                        }
                    }
                    else
                    {
                        throw new InvalidTraceException(string.Concat("Unable to trace action on \"", go.name, "\" because no name given in its ActionPerformed component."), ap.exceptionStackTrace);
                    }

                    if (cMonitoring)
                    {
                        if (ap.performedBy == "system")
                        {
                            tmpPerformer = ap.performedBy;
                        }
                        else
                        {
                            tmpPerformer = "player";
                        }
                        if (ap.orLabels == null)
                            tmpLabels = MonitoringManager.trace(cMonitoring, tmpActionName, tmpPerformer);
                        else
                            tmpLabels = MonitoringManager.trace(cMonitoring, tmpActionName, tmpPerformer, true, ap.orLabels);
                        GameObjectManager.addComponent<Trace>(traces, new
                        {
                            actionName = tmpActionName,
                            componentMonitoring = cMonitoring,
                            performedBy = tmpPerformer,
                            time = Time.time,
                            orLabels = ap.orLabels,
                            labels = tmpLabels
                        });
                        // if (tmpLabels.Length != 0)
                            // tmpString = tmpLabels[0];
                        // for (int i = 1; i < tmpLabels.Length; i++)
                        // {
                            // tmpString = string.Concat(tmpString, " ", tmpLabels[i]);
                        // }
                        //Debug.Log(string.Concat(tmpPerformer, " ", tmpActionName, " ", go.name, System.Environment.NewLine, tmpString));
                        //File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Log - ", tmpPerformer, " ", tmpActionName, " ", go.name, System.Environment.NewLine, tmpString));
                    }
                    else if (ap.family != null)
                    {
                        try
                        {
                            tmpActionName = ap.name;
                            if (ap.performedBy == "system")
                            {
                                tmpPerformer = ap.performedBy;
                            }
                            else
                            {
                                tmpPerformer = "player";
                            }
                            if (ap.orLabels == null)
                                tmpLabels = MonitoringManager.trace(ap.family, tmpActionName, tmpPerformer);
                            else
                                tmpLabels = MonitoringManager.trace(ap.family, tmpActionName, tmpPerformer, true, ap.orLabels);
                            GameObjectManager.addComponent<Trace>(traces, new
                            {
                                actionName = tmpActionName,
                                family = ap.family,
                                performedBy = tmpPerformer,
                                time = Time.time,
                                orLabels = ap.orLabels,
                                labels = tmpLabels
                            });
							// if (tmpLabels.Length != 0)
								// tmpString = tmpLabels[0];
                            // for (int i = 1; i < tmpLabels.Length; i++)
                            // {
                                // tmpString = string.Concat(tmpString, " ", tmpLabels[i]);
                            // }
                            //Debug.Log(string.Concat(tmpPerformer, " ", tmpActionName, " ", go.name, System.Environment.NewLine, tmpString));
                            //File.AppendAllText("Data/UnityLogs.txt", string.Concat(System.Environment.NewLine, "[", DateTime.Now.ToString("yyyy.MM.dd.hh.mm"), "] Log - ", tmpPerformer, " ", tmpActionName, " ", go.name, System.Environment.NewLine, tmpString));
                        }
                        catch (global::System.Exception)
                        {
                            throw new InvalidTraceException(string.Concat("Unable to trace action \"", tmpActionName, "\" on the family because of invalid arguments in the ActionPerformed component or the family is not monitored."), ap.exceptionStackTrace);
                        }
                    }
                }
                for (int j = nb - 1; j > -1; j--)
                {
                    GameObjectManager.removeComponent(listAP[j]);
                }
            }
        }
    }
}