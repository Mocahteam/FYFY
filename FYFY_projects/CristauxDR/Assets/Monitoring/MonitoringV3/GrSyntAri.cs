using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrSyntAri :MonoBehaviour{

    public char[] etiquettes;

    public void Start()
    {
        Debug.Log("Resultat : "+validSyntAri());
    }

    public bool validSyntAri()
    {
        int pos = 0;
        int curseur = 0;
        if (etiquettes.Length == 0)
            return false;
        return valid(pos,ref curseur);
    }

    public bool valid(int pos, ref int curseur)
    {
        bool v = m(pos,ref curseur);
        return v;
    }

    private bool m(int pos, ref int curseur)
    {
        Debug.Log("m" + pos);
        bool v = (o(pos, ref curseur) /*|| n(pos,ref curseur)*/ || label(pos, ref curseur));//|| o(pos, ref curseur));
        return v;
    }

    private bool n(int pos, ref int curseur)
    {
        Debug.Log("n" + pos);
        int curMot1 = 0, curO = 0, curMot2 = 0;
        bool v = (m(pos, ref curMot1) && op(curMot1, ref curO) && m(curO, ref curMot2));
        curseur = v ? curMot2 : pos;
        return v;
    }

    private bool o(int pos, ref int curseur)
    {
        Debug.Log("o" + pos);
        int curPo = 0, curMot = 0, curPf = 0;
        bool v = (po(pos, ref curPo) && m(curPo, ref curMot) && pf(curMot, ref curPf));
        curseur = v ? curPf : pos;
        return v;
    }

    private bool po(int pos,ref int curseur)
    {   Debug.Log("po" + pos);
        //Debug.Log("Debug dans po : " + etiquettes[pos]);
        bool v;
        if (v = (etiquettes[pos] == '('))
            curseur = pos + 1;
        else
            curseur = pos;
        return v;
    }

    private bool pf(int pos, ref int curseur)
    {
        Debug.Log("pf"+pos);
        //Debug.Log("Debug dans pf : " + etiquettes[pos]);
        bool v;
        if (v = (pos < etiquettes.Length) && ((etiquettes[pos] == ')')))
            curseur = pos + 1;
        else
            curseur = pos;
        return v;
    }

    private bool op(int pos, ref int curseur)
    {
        Debug.Log("op" + pos);
        //Debug.Log("Debug dans op : " + etiquettes[pos]);
        bool v;
        if (v = (etiquettes[pos] == 'o'))
            curseur = pos + 1;
        else
            curseur = pos;
        return v;
    }

    private bool label(int pos, ref int curseur)
    {
        Debug.Log("label" + pos);
        //Debug.Log("Debug dans label : "+etiquettes[pos]);
        bool v;
        if (v = (etiquettes[pos] == 'l'))
            curseur = pos + 1;
        else
            curseur = pos;
        return v;
    }
}
