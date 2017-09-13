using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace monitoring {
	public class BTreeNode<T>{

		private T value = default(T);
		private BTreeNode<T> lnode = null;
		private BTreeNode<T> rnode = null;

		public BTreeNode ()
		{
		}

		public BTreeNode (T val)
		{
			value = val;
		}

		public BTreeNode (BTreeNode<T> node)
		{
			this.value = node.Value;
			this.lnode = node.Lnode;
			this.rnode = node.Rnode;
		}

		public BTreeNode (BTreeNode<T> lnode,BTreeNode<T> rnode)
		{
			this.lnode = lnode;
			this.rnode = rnode;
		}

		public BTreeNode (T val,BTreeNode<T> lnode,BTreeNode<T> rnode)
		{
			this.value = val;
			this.lnode = lnode;
			this.rnode = rnode;
		}
			
		public T[] getPrefixValues(){
			List<T> val = new List<T> ();

			val.Add (value);

			if (lnode != null)
				val.AddRange(lnode.getPrefixValues ());
			if (rnode != null)
				val.AddRange (rnode.getPrefixValues ());

			return val.ToArray ();
		}

		public T[] getInfixValues(){
			List<T> val = new List<T> ();

			if (lnode != null)
				val.AddRange(lnode.getInfixValues ());

			val.Add (value);

			if (rnode != null)
				val.AddRange (rnode.getInfixValues ());

			return val.ToArray ();
		}

		public T[] getPostfixValues(){
			List<T> val = new List<T> ();

			if (lnode != null)
				val.AddRange(lnode.getPostfixValues ());
			
			if (rnode != null)
				val.AddRange (rnode.getPostfixValues ());
			val.Add (value);

			return val.ToArray ();
		}


		public void addRNode(){
			rnode = new BTreeNode<T>();
		}

		public void addRNode(T val){
			rnode = new BTreeNode<T>(val);
		}

		public void addLNode(){
			lnode = new BTreeNode<T>();
		}

		public void addLNode(T val){
			lnode = new BTreeNode<T>(val);
		}

		public bool isLeaf(){
			if (lnode == null && rnode == null)
				return true;
			return false;
		}

		//Getters & Setters
		public T Value {
			get {
				return this.value;
			}
			set {
				this.value = value;
			}
		}

		public BTreeNode<T> Lnode {
			get {
				return this.lnode;
			}
			set {
				lnode = value;
			}
		}

		public BTreeNode<T> Rnode {
			get {
				return this.rnode;
			}
			set {
				rnode = value;
			}
		}
	}
}