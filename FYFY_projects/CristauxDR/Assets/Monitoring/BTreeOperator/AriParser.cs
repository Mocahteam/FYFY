using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using monitorV3;
using System.Text.RegularExpressions;

public class AriParser{

	// Use this for initialization
	void Start () {
		//a b c * d e f * g h * j k l * f + + + * + * + 
		//a b c d e f g h * j k l * f + + + * * + * * + 
		string ari = "((( aa + bb ) * mlopi)+kkk)*z"; //Doit pouvoir séparer les labels ? (format auto non libre pour simplifier ça ? réfuté : label libre)
		string[] tokens = parseAri (ari);
		/*//Debug.Log ("Print ari ");
		foreach (string s in debug) {
			//Debug.Log (s);
		}

		//Debug.Log (printList(debug));*/

		//lex(ari); //Objectif du jour : ça et le lien avec l'interface
		//string[] tokens = ari.Split(' ');
		//foreach (string s in tokens) {
			//Debug.Log (s);
		//}
		////Debug.Log ("pouet"+printList(tokens));
		BTreeNode<string> tree = treeBuilder (tokens);
		//string[] debug = tree.getPostfixValues ();
		//Debug.Log ("Print postfix :");

		//Debug.Log (printList(debug));

		distribute (tree);

		//string[] debug2 = tree.getInfixValues ();
		//Debug.Log ("Print postfix distrib :");

		//Debug.Log (printList(debug2));
	}

	public string[] getDistribution(string ari){
		string[] tokens = parseAri (ari);
	
		BTreeNode<string> tree = treeBuilder (tokens);
        if (tree == null)
            return new string[] {};
		distribute (tree);
		string[] distrib = tree.getInfixValues ();
        return distrib;
	}

	public bool validAri(TransitionLink tLink){
		string ari = tLink.logic;

        if (ari == null || ari.Length == 0)
            return true;

		// Change all known link with true
		foreach (Link l in tLink.links) {
			ari = Regex.Replace (ari, "^" + l.label + "$", "true");
			ari = Regex.Replace (ari, "^" + l.label + "([*+ ])", "true$1");
			ari = Regex.Replace (ari, "([(*+ ])(" + l.label + ")([)*+ ])", "$1true$3");
			ari = Regex.Replace (ari, "([(*+ ])(" + l.label + ")([)*+ ])", "$1true$3"); // need to be done twice in case of (l1+l1) in this case "+" has to be used to replace "(l1+" and "+l1)" but Regex.Replace can't do this in a same replacement, only the first one is replaced.
			ari = Regex.Replace (ari, "([*+ ])" + l.label + "$", "$1true");
		}
		// Remove all spaces
		ari = ari.Replace(" ", "");
		// Try to resolve expression
		bool stable = false;
		do {
			string prevExp = ari;
			ari = ari.Replace("true+true", "true");
			ari = ari.Replace("true*true", "true");
			ari = ari.Replace("(true)", "true");
			stable = prevExp == ari;
		} while (!stable);

		return ari == "true";

		// Previous solution
		/*
		int po = 0;
		int pf = 0;
		bool badOp = false;
		char precedent = ' ';

        //manque vérif cohérence parenthèses
        //vérif label ne peut pas être suivi ou précédé par un label

		foreach (char c in ari) {
			
			if (!char.IsWhiteSpace (c)) {
				
				//Debug.Log ("precedent : " + precedent + " c :  " + c);
				if (c == '(') {
					//Debug.Log ("parenthèse ouvrante");
					po++;
				}
				if (c == ')') {
					//Debug.Log ("parenthèse fermante");
					pf++;
				}
				if (((precedent == '(' || precedent == '*' || precedent == '+') && (c == '*' || c == '+')) || ((precedent == '*' || precedent == '+') && (c == ')' || c == '+' || c == '*'))) {
					//Debug.Log ("BadOp");
					badOp = true;
				}
				precedent = c;
			}
		}

        //Le début et la fin ne doivent pas être des opérateurs
        if (ari[(ari.Length - 1)] == '+' || ari[(ari.Length - 1)] == '*' || ari[0] == '+' || ari[0] == '*')
            badOp = true;

		return po == pf && !badOp;*/
	}

	string[] parseAri(string ari){
		List<string> lStr = new List<string> ();
		List<char> word = new List<char> ();
		for (int i = 0; i < ari.Length; i++) {
			
			char c = ari [i];
			// build words
			if (char.IsLetterOrDigit(c)) {
				word.Add (c);
			} 
			else if (c == '(' || c == ')' || c == '+' || c == '*') { //If operator or parenthesis; push word
				if(word.Count != 0){
					lStr.Add (new string (word.ToArray ()));
					word = new List<char> (); // Free buffer
				}
				lStr.Add (c.ToString ()); //Push operator or parenthesis
			} 


		}
		if(word.Count != 0)
			lStr.Add (new string (word.ToArray ())); //Push last word

		return lStr.ToArray ();
	
	}

	void distribute(BTreeNode<string> tree){

		distribute (tree,tree);
	}


	void distribute(BTreeNode<string> root,BTreeNode<string> tree){

		// Stop condition
		if (tree == null || tree.isLeaf ()) {
			// Nothing to do

		}
		// Recursive calls
		//Deux cas : ou/l-et, ou/r-et (ou/l-et/r-et? pas réfléchi à celui-la encore... trouver un cas !) 
		//Du coup c'est bon : Réduction possible de la complexité en mettant convenablement la variable root à jour, pour backtrack quand modification
		else if (tree.Value.Equals ("*") && tree.Lnode.Value.Equals ("+")) { 

			tree.Value = "+";

			BTreeNode<string> tmpLNode = new BTreeNode<string> ("*", tree.Lnode.Lnode, tree.Rnode);
			BTreeNode<string> tmpRNode = new BTreeNode<string> ("*", tree.Lnode.Rnode, tree.Rnode);

			tree.Lnode = tmpLNode;
			tree.Rnode = tmpRNode;

			distribute (root,root);

		} else if (tree.Value.Equals ("*") && tree.Rnode.Value.Equals ("+")) {

			tree.Value = "+";

			BTreeNode<string> tmpLNode = new BTreeNode<string> ("*", tree.Lnode, tree.Rnode.Lnode);
			BTreeNode<string> tmpRNode = new BTreeNode<string> ("*", tree.Lnode, tree.Rnode.Rnode);

			tree.Lnode = tmpLNode;
			tree.Rnode = tmpRNode;

			distribute (root,root);

		} else {

			distribute (root,tree.Lnode);
			distribute (root,tree.Rnode);
		}
	}

	string[] removeParentesis(string[] tokens){
		
		List<string> tmpList = new List<string> ();
		foreach(string s in tokens){
			if (!s.Equals ("(") && !s.Equals (")")) {
				tmpList.Add (s);
			}
		}
		return tmpList.ToArray ();
	}

	BTreeNode<string> treeBuilder(string[] tokens){
		// Stop condition
		if (tokens.Length == 0) {
			return null;
		}
		else if (removeParentesis(tokens).Length  == 1) {
			return new BTreeNode<string> (removeParentesis(tokens) [0]);
		}
		else{
			int index = getIndexOfMinPriority(tokens);
			string op = tokens [index];
			List<string> Llist = new List<string>(tokens).GetRange(0,index);
			List<string> Rlist = new List<string>(tokens).GetRange(index+1,tokens.Length-index-1);

			// Recursive call
			return new BTreeNode<string>(op,treeBuilder(Llist.ToArray()),treeBuilder(Rlist.ToArray()));
		}
	}
		
		
	int getIndexOfMinPriority(List<string> ari){ 
		return getIndexOfMinPriority (ari.ToArray());
	}

	int getIndexOfMinPriority(string[] ari){
		int tmpVal = int.MaxValue;
		int tmpIndex = -1;

		int priorite = 0;
		int cpt = 0;

		foreach(string c in ari){
			if (c.Equals("*") && 1+priorite <= tmpVal) {
				tmpIndex = cpt;
				tmpVal = 1+priorite;
			}
			if (c.Equals("+") && priorite <= tmpVal) {
				tmpIndex = cpt;
				tmpVal = priorite;
			}
			if (c.Equals("(")) {
				priorite+=2;

			}
			if (c.Equals(")")) {
				priorite-=2;
			}
			cpt++;
		}

		return tmpIndex;
	}

	string printList(List<string> str){
		return printList (str.ToArray ());
	}
	string printList(string[] str){
		string strDebug = "";
		foreach (string s in str)
			strDebug += s + " ";
		return strDebug;
	}
}