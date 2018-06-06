using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

namespace FYFY_plugins.Monitoring {
	/// <summary>Parser of logic expressions.</summary>
	public class ExpressionParser{

		internal static string[] getDistribution(string exp){
			string[] tokens = parseExpression (exp);
		
			BTreeNode<string> tree = treeBuilder (tokens);
	        if (tree == null)
	            return new string[] {};
			distribute (tree);
			string[] distrib = tree.getInfixValues ();
	        return distrib;
		}
		
		/// <summary>Check if a linkLabel doesn't contain one of these reserved tokens "()[]*+ ".</summary>
		public static bool checkPrerequisite (string linkLabel){
			return !linkLabel.Contains ("(") && !linkLabel.Contains (")") && !linkLabel.Contains ("[") && !linkLabel.Contains ("]") && !linkLabel.Contains ("*") && !linkLabel.Contains ("+") && !linkLabel.Contains (" ");
		}

		/// <summary>Check if a transitionLink contains a valid logix expression.</summary>
		public static bool isValid(TransitionLink tLink){
			string exp = tLink.logic;

	        if (exp == null || exp.Length == 0)
	            return true;

			// Change all known link with true
			foreach (Link l in tLink.links) {
				if (!checkPrerequisite(l.label))
					return false;
				exp = Regex.Replace (exp, "^" + l.label + "$", "true");
				exp = Regex.Replace (exp, "^" + l.label + "([*+ ])", "true$1");
				exp = Regex.Replace (exp, "([(*+ ])(" + l.label + ")([)*+ ])", "$1true$3");
				exp = Regex.Replace (exp, "([(*+ ])(" + l.label + ")([)*+ ])", "$1true$3"); // need to be done twice in case of (l1+l1) in this case "+" has to be used to replace "(l1+" and "+l1)" but Regex.Replace can't do this in a same replacement, only the first one is replaced.
				exp = Regex.Replace (exp, "([*+ ])" + l.label + "$", "$1true");
			}
			// Remove all spaces
			exp = exp.Replace(" ", "");
			// Try to resolve expression
			bool stable = false;
			do {
				string prevExp = exp;
				exp = exp.Replace("true+true", "true");
				exp = exp.Replace("true*true", "true");
				exp = exp.Replace("(true)", "true");
				stable = prevExp == exp;
			} while (!stable);

			return exp == "true";

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
					if (c == '(') {
						po++;
					}
					if (c == ')') {
						pf++;
					}
					if (((precedent == '(' || precedent == '*' || precedent == '+') && (c == '*' || c == '+')) || ((precedent == '*' || precedent == '+') && (c == ')' || c == '+' || c == '*'))) {
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

		static string[] parseExpression(string exp){
			List<string> lStr = new List<string> ();
			List<char> word = new List<char> ();
			for (int i = 0; i < exp.Length; i++) {
				
				char c = exp [i];
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

		static void distribute(BTreeNode<string> tree){

			distribute (tree,tree);
		}


		static void distribute(BTreeNode<string> root,BTreeNode<string> tree){

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

		static string[] removeParentesis(string[] tokens){
			
			List<string> tmpList = new List<string> ();
			foreach(string s in tokens){
				if (!s.Equals ("(") && !s.Equals (")")) {
					tmpList.Add (s);
				}
			}
			return tmpList.ToArray ();
		}

		static BTreeNode<string> treeBuilder(string[] tokens){
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
			
			
		static int getIndexOfMinPriority(List<string> expressions){ 
			return getIndexOfMinPriority (expressions.ToArray());
		}

		static int getIndexOfMinPriority(string[] expressions){
			int tmpVal = int.MaxValue;
			int tmpIndex = -1;

			int priorite = 0;
			int cpt = 0;

			foreach(string c in expressions){
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
	}
}