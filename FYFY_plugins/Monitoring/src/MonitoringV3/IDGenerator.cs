using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace FYFY_plugins.Monitoring
{
    internal static class IDGenerator
    {
        internal static int genID()
        {
            List<int> inscrits = new List<int>();

			// Get all used ids
            foreach (ComponentMonitoring go in Resources.FindObjectsOfTypeAll<ComponentMonitoring>())
            {
                if (go.id != -1)
                    inscrits.Add(go.id);
            }
            inscrits.Sort();
			// Find the first hole available
			int newId = 0;
            foreach (int i in inscrits)
            {
                if (newId != i)
                    return newId;
                newId++;
            }
            return newId;
        }

		internal static bool isUnique(ComponentMonitoring monitor)
        {
			// Look for an other monitor component with the same id
            foreach (ComponentMonitoring go in Resources.FindObjectsOfTypeAll<ComponentMonitoring>())
				if (go.id != -1 && go != monitor)
					if (go.id == monitor.id)
						return false;
            return true;
        }
    }
}
