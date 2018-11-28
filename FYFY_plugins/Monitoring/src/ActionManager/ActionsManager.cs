using UnityEngine;
using FYFY;
using System;

namespace FYFY_plugins.Monitoring {
    public class ActionsManager : FSystem
    {

        private Family f_actions = FamilyManager.getFamily(new AllOfComponents(typeof(ActionPerformed)));

        public static ActionsManager instance;

        private GameObject traces;
        private string tmpString;

        private string tmpPerformer;
        private string[] tmpLabels;

        public ActionsManager()
        {
            if (Application.isPlaying)
            {
                traces = new GameObject("Traces");
                GameObjectManager.bind(traces);
                f_actions.addEntryCallback(ActionProcessing);
            }
            instance = this;
        }

        // Use this to update member variables when system resume.
        // Advice: avoid to update your families inside this function.
        protected override void onResume(int currentFrame)
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
                                    cMonitoring = cm;
                                    tmpActionName = ap.name;
                                    matched++;
                                }
                            }
                            if (matched > 1)
                            {
                                Debug.LogWarning(string.Concat("Several ComponentMonitoring on ", go.name, " are matching the name \"", ap.name, "\" and the overrideName \"", ap.overrideName, "\". By default, the second one found is used to trace."));
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
                                    cMonitoring = cm;
                                    tmpActionName = ap.name;
                                    matched++;
                                }
                            }
                            if (matched > 1)
                            {
                                Debug.LogWarning(string.Concat("Several ComponentMonitoring on ", go.name, " are matching the name \"", ap.name, "\". By default, the second one found is used to trace."));
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
                                    cMonitoring = cm;
                                    tmpActionName = tl.transition.label;
                                    matched++;
                                }
                            }
                            if (matched > 1)
                            {
                                Debug.LogWarning(string.Concat("Several ComponentMonitoring on ", go.name, " are matching the overrideName \"", ap.overrideName, "\". By default, the second one found is used to trace."));
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
                        if (tmpLabels.Length != 0)
                            tmpString = tmpLabels[0];
                        for (int i = 1; i < tmpLabels.Length; i++)
                        {
                            tmpString = string.Concat(tmpString, " ", tmpLabels[i]);
                        }
                        GameObjectManager.addComponent<Trace>(traces, new
                        {
                            actionName = tmpActionName,
                            componentMonitoring = cMonitoring,
                            performedBy = tmpPerformer,
                            time = Time.time,
                            orLabels = ap.orLabels,
                            labels = tmpLabels
                        });
                        Debug.Log(string.Concat(tmpPerformer, " ", tmpActionName, " ", go.name, System.Environment.NewLine, tmpString));
                    }
                    else if (ap.family != null)
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
                        tmpString = tmpLabels[0];
                        for (int i = 1; i < tmpLabels.Length; i++)
                        {
                            tmpString = string.Concat(tmpString, " ", tmpLabels[i]);
                        }
                        GameObjectManager.addComponent<Trace>(traces, new
                        {
                            actionName = tmpActionName,
                            family = ap.family,
                            performedBy = tmpPerformer,
                            time = Time.time,
                            orLabels = ap.orLabels,
                            labels = tmpLabels
                        });
                        Debug.Log(string.Concat(tmpPerformer, " ", tmpActionName, " ", go.name, System.Environment.NewLine, tmpString));
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