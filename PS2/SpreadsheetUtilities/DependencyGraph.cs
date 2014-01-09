// Skeleton implementation written by Joe Zachary for CS 3500, September 2013.
// Version 1.1 (Fixed error in comment for RemoveDependency.)
//full implementation by Dylan Noaker

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace SpreadsheetUtilities
{

    /// <summary>
    ///
    /// </summary>
	/// <remarks> 
	/// A DependencyGraph can be modeled as a set of ordered pairs of strings.  Two ordered pairs
	/// (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1 equals t2.
	/// (Recall that sets never contain duplicates.  If an attempt is made to add an element to a 
	/// set, and the element is already in the set, the set remains unchanged.)
	/// 
	/// Given a DependencyGraph DG:
	/// 
	///    (1) If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
	///        
	///    (2) If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
	/// left is dees. right is dents.
	///For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
	///     dependents("a") = {"b", "c"}
	///     dependents("b") = {"d"}
	///     dependents("c") = {}
	///     dependents("d") = {"d"}
	///     dependees("a") = {}
	///     dependees("b") = {"a"}
	///     dependees("c") = {"a"}
	///     dependees("d") = {"b", "d"}
	///</remarks>
    public class DependencyGraph
    {
	    //Representation:
	    //invariants:	-all keys are dees, as name suggests. As dictionaries are hashtables,
	    //			this allows O(1) access of dees.
	    //			-values are Hashsets of their respective key's dents allowing O(1) access
	    //			-a dee may not depend on one of it's dependents!!! this forms a 
	    //			CIRCULAR DEPENDENCY in which we don't know dees from dents!
	    //abstraction function:
	    //		the dictionary with keys a,b,c will have values which are hashsets. 
		//		I will write V(a) to represent the hashset which maps to key a.
	    //		let V(a)= x,y,z and V(b) = l,m,n and V(c)= l,m,n.
	    //		this represents the set of pairs  (a,x)(a,y)(a,z) (b,l)(b,m)(b,n) (c,l)(c,m)(c,n)
	    //		I'm not very good at ascii drawing, so I can't draw the actual graph here, but 
	    //		I'm sure we'll be using dotty or something to do this later.
	    private Dictionary<String, HashSet<String>> DeesAreKeys;
		private int _size;
        /// <summary>
        /// Creates an empty DependencyGraph.
        /// </summary>
        public DependencyGraph()
        {
		   DeesAreKeys = new Dictionary<string, HashSet<string>>();
		   _size = 0;
        }


        /// <summary>
        /// The number of ordered pairs in the DependencyGraph.
        /// </summary>
	   public int Size {
		   get {return _size;}
	   }


        /// <summary>
        /// The size of dependees(s).
        /// This property is an example of an indexer.  If dg is a DependencyGraph, you would
        /// invoke it like this:
        /// dg["a"]
        /// It should return the size of dependees("a")
        /// </summary>
        public int this[string s]
        {
		   get {
			   int counter = 0;
			   foreach (KeyValuePair<String, HashSet<String>> entry in DeesAreKeys) {
				   if (entry.Value.Contains(s)) {
					   counter++;
					   continue;
				   }
			   }
			   return counter;
		   }
        }


        /// <summary>
        /// Reports whether dependents(s) is non-empty.
        /// </summary>
        public bool HasDependents(string s)
        {
		   try {
			   return DeesAreKeys[s].Count > 0;
		   }
			   //if s isn't a key
		   catch (KeyNotFoundException) {
			   return false;
		   }
        }


        /// <summary>
        /// Reports whether dependees(s) is non-empty.
        /// </summary>
	   public bool HasDependees(string s) {
		   foreach (KeyValuePair<String, HashSet<String>> entry in DeesAreKeys) {
			   if (entry.Value.Contains(s)) {
				   return true;
			   }
		   }
		   return false;
	   }


        /// <summary>
        /// Enumerates dependents(s).
        /// </summary>
	 
        public IEnumerable<string> GetDependents(string s)
        {
		   try {
			   return new HashSet<string>(DeesAreKeys[s]);
		   }
			   //s may not be a valid key
		   catch (KeyNotFoundException) {
			   //this will be empty, but implements IEnumerable
			   return new List<String>();
		   }
		
        }

        /// <summary>
        /// Enumerates dependees(s).
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
		   //List<string> toreturn = new List<string>();
		   foreach (KeyValuePair<String, HashSet<String>> entry in DeesAreKeys) {
			   if (entry.Value.Contains(s)) {
				   //toreturn.Add(entry.Key);
				   yield return entry.Key;
			   }
		   }
            //return toreturn;
        }


        /// <summary>
        /// Adds the ordered pair (s,t), if it doesn't exist
        /// </summary>
        /// <param name="s">s is the dee</param>
        /// <param name="t">t is the dent</param>
        public void AddDependency(string s, string t)
        {
		   //avoid circular dependencies. breakout if (t,s) exists.
			if (DeesAreKeys.ContainsKey(t) && DeesAreKeys[t].Contains(s))
			{
				throw new InvalidOperationException();
			}
		   //if s is a key in dees are keys and doesn't contain t in it's dents, add t to its dents
		   //recall add() returns a bool. we can use it to determine size!
		   if (DeesAreKeys.ContainsKey(s)&&DeesAreKeys[s].Add(t)) {
				   _size++;
		   }
		  
		   // if s is not a dee, add it as a k/v pair to deesarekeys and add t to its dents
		   else if (!DeesAreKeys.ContainsKey(s)) {
			   DeesAreKeys.Add(s, new HashSet<string>());
			   DeesAreKeys[s].Add(t);
			   _size++;
		   }
		   //increment size at some point
		   //sort of convoluted
        }


        /// <summary>
        /// Removes the ordered pair (s,t), if it exists
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        public void RemoveDependency(string s, string t)
        {
		   
		   if (DeesAreKeys.ContainsKey(s)&&DeesAreKeys[s].Remove(t)) {
				   _size--;
			   //do we want to remove S if it has no dents???
			   //if (DeesAreKeys[s].Count == 0) {
			   //	DeesAreKeys.Remove(s);
			   //}
		   }
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (s,r).  Then, for each
        /// t in newDependents, adds the ordered pair (s,t).
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {

		   try {
			   HashSet<String> alteringList = DeesAreKeys[s];
			   alteringList.Clear();
			   //as of now, there are no elements in s's dents
			   _size -= alteringList.Count;
			   alteringList.UnionWith(newDependents);
			   //as of now, there are more elements in s's dents
			   _size += alteringList.Count;
		   }
			   //in the case where s is not already in the DG, we should add it with new dents??
		   catch (KeyNotFoundException) {
			   DeesAreKeys.Add(s, new HashSet<string>(newDependents));
			   _size += newDependents.Count<string>();
		   }
	
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
        /// t in newDependees, adds the ordered pair (t,s).
        /// </summary>
        public void ReplaceDependees(string s, IEnumerable<string> newDependees)
        {
		   foreach (KeyValuePair<String, HashSet<String>> entry in DeesAreKeys) {
			   entry.Value.Remove(s);
			   _size--;
		   }
		   foreach (string neuDee in newDependees) {

			   if (DeesAreKeys.ContainsKey(neuDee)) {
				   DeesAreKeys[neuDee].Add(s);
			   }
			   else {
				   //if we want a totally new dee, we need to add it as a k/v pair
				   //where the value (a list) has s in it. 
				   DeesAreKeys.Add(neuDee, new HashSet<string>());
				   DeesAreKeys[neuDee].Add(s);
			   }
		   }
        }
    
    }

}