using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FYFY_plugins.Monitoring {
	internal class BTreeNode<T>{

		private T value = default(T);
		private BTreeNode<T> lnode = null;
		private BTreeNode<T> rnode = null;

		internal BTreeNode ()
		{
		}

		internal BTreeNode (T val)
		{
			value = val;
		}

		internal BTreeNode (BTreeNode<T> node)
		{
			this.value = node.Value;
			this.lnode = node.Lnode;
			this.rnode = node.Rnode;
		}

		internal BTreeNode (BTreeNode<T> lnode,BTreeNode<T> rnode)
		{
			this.lnode = lnode;
			this.rnode = rnode;
		}

		internal BTreeNode (T val,BTreeNode<T> lnode,BTreeNode<T> rnode)
		{
			this.value = val;
			this.lnode = lnode;
			this.rnode = rnode;
		}
			
		internal T[] getPrefixValues(){
			List<T> val = new List<T> ();

			val.Add (value);

			if (lnode != null)
				val.AddRange(lnode.getPrefixValues ());
			if (rnode != null)
				val.AddRange (rnode.getPrefixValues ());

			return val.ToArray ();
		}

		internal T[] getInfixValues(){
			List<T> val = new List<T> ();

			if (lnode != null)
				val.AddRange(lnode.getInfixValues ());

			val.Add (value);

			if (rnode != null)
				val.AddRange (rnode.getInfixValues ());

			return val.ToArray ();
		}

		internal T[] getPostfixValues(){
			List<T> val = new List<T> ();

			if (lnode != null)
				val.AddRange(lnode.getPostfixValues ());
			
			if (rnode != null)
				val.AddRange (rnode.getPostfixValues ());
			val.Add (value);

			return val.ToArray ();
		}


		internal void addRNode(){
			rnode = new BTreeNode<T>();
		}

		internal void addRNode(T val){
			rnode = new BTreeNode<T>(val);
		}

		internal void addLNode(){
			lnode = new BTreeNode<T>();
		}

		internal void addLNode(T val){
			lnode = new BTreeNode<T>(val);
		}

		internal bool isLeaf(){
			if (lnode == null && rnode == null)
				return true;
			return false;
		}

		//Getters & Setters
		internal T Value {
			get {
				return this.value;
			}
			set {
				this.value = value;
			}
		}

		internal BTreeNode<T> Lnode {
			get {
				return this.lnode;
			}
			set {
				lnode = value;
			}
		}

		internal BTreeNode<T> Rnode {
			get {
				return this.rnode;
			}
			set {
				rnode = value;
			}
		}
	}
}