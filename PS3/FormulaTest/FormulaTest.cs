using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;
using System.Text.RegularExpressions;

namespace FormulaTest
{
	/// <summary>
	/// Arrange, Act, Assert. AAA insurance
	/// </summary>
	[TestClass]
	public class FormulaTest
	{
		//[TestInitialize]
		
		/// <summary>
		/// straightforward test. should be able to make new formula with factory.
		/// </summary>
		/// <remarks>My philosophy on this situation is that you can debug toString to see if
		/// it is the problem or you may hit a problem in the constructor code first.
		/// I know it is bad practice to test functions with functions, but we are limited
		/// by specifications</remarks>
        [TestMethod]
        public void ConstructorTest1() {
            Formula f1 = new Formula("2.0*3.0");
            Assert.AreEqual("2.0*3.0", f1.ToString());
        }
	    /// <summary>
	    /// can we do it with variables
	    /// </summary>
        [TestMethod]
        public void ConstructorTest2() {
            Formula f1 = new Formula("x2*y3");
            Assert.AreEqual("x2*y3", f1.ToString());
        }
	    /// <summary>
	    /// try parens
	    /// </summary>
        [TestMethod]
        public void ConstructorTest3() {
            Formula f1 = new Formula("x2*(y3)");
            Assert.AreEqual("x2*(y3)", f1.ToString());
        }
		/// <summary>
		/// uppercases variables
		/// </summary>
		[TestMethod]
		public void Constructor2Test()
		{
			Formula f1 = new Formula("x", s => s.ToUpper(), s => (s == "X") ? true : false);
			Assert.AreEqual("X", f1.ToString());
		}
		/// <summary>
		/// 
		/// </summary>
		[TestMethod]
		public void Constructor2Test1()
		{
			Formula f1 = new Formula("x+1", s => s.ToUpper(), s => (s == "X") ? true : false);
			Assert.AreEqual("X+1", f1.ToString());
		}
        /// <summary>
        /// There must be at least one token.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FormulaFormatException))]
        public void ConstructorFailTest1() {
            Formula f1 = new Formula("");
        }
	   /// <summary>
	   /// can't start with ops
	   /// </summary>
	   [TestMethod]
	   [ExpectedException(typeof(FormulaFormatException))]
	   public void ConstructorFailTest2()
	   {
		   Formula f1 = new Formula("+2");
	   }
	   /// <summary>
	   /// can't start with close paren
	   /// </summary>
	   [TestMethod]
	   [ExpectedException(typeof(FormulaFormatException))]
	   public void ConstructorFailTest3()
	   {
		   Formula f1 = new Formula(")2");
	   }
	   /// <summary>
	   /// open and close must be equal
	   /// </summary>
	   [TestMethod]
	   [ExpectedException(typeof(FormulaFormatException))]
	   public void ConstructorFailTest4()
	   {
		   Formula f1 = new Formula("((2)");
	   }
	   /// <summary>
	   /// open and close must be equal
	   /// </summary>
	   [TestMethod]
	   [ExpectedException(typeof(FormulaFormatException))]
	   public void ConstructorFailTest5()
	   {
		   Formula f1 = new Formula("(2))");
	   }
	   /// <summary>
	   /// open and close must be equal
	   /// </summary>
	   [TestMethod]
	   [ExpectedException(typeof(FormulaFormatException))]
	   public void ConstructorFailTest6()
	   {
		   Formula f1 = new Formula("(2))");
	   }
	   /// <summary>
	   /// just an op should fail
	   /// </summary>
	   [TestMethod]
	   [ExpectedException(typeof(FormulaFormatException))]
	   public void ConstructorFailTest7()
	   {
		   Formula f1 = new Formula("*");
	   }
	 
        [TestMethod]
        public void EqualsTest() {
            Formula f1 = new Formula("2+3");
            Formula f2 = new Formula("2+3");
            Assert.IsTrue(f1.Equals(f2));
        }
        [TestMethod]
        public void EqualsTest2() {
            Formula f1 = new Formula("2+3");
            Formula f2 = new Formula("2+4");
            Assert.IsFalse(f1.Equals(f2));
        }
        [TestMethod]
        public void EvaluateTest1() {
		Func<string,double> lookerupper = s=>0;
			Assert.AreEqual(5.5, new Formula("2.5+3").Evaluate(lookerupper));

		}
		/// <summary>
		/// SIMPLE VAR TEST
		/// </summary>
		[TestMethod]
		public void GetVariablesTest()
		{
			Formula f1 = new Formula("X+y+z");
			foreach (string var in f1.GetVariables())
			{
				Assert.IsTrue(Regex.IsMatch(var, @"[Xyz]"));
			}

		}
		/// <summary>
		/// some vars and nums
		/// </summary>
		[TestMethod]
		public void GetVariablesTest2()
		{
			Formula f1 = new Formula("X+y+z+2");
			foreach (string var in f1.GetVariables())
			{
				Assert.IsTrue(Regex.IsMatch(var, @"[Xyz]"));
			}

		}
		/// <summary>
		/// specified normalize and validate
		/// </summary>
		[TestMethod]
		public void GetVariablesTest3()
		{
			Formula f1 = new Formula("X+y+z+2",s=>s.ToUpper(),s=>(Regex.IsMatch(s,@"[A-Z]")));
			foreach (string var in f1.GetVariables())
			{
				Assert.IsTrue(Regex.IsMatch(var, @"[XYZ]"));
			}

		}
		/// <summary>
		/// dupe vars
		/// </summary>
		[TestMethod]
		public void GetVariablesTest4()
		{
			Formula f1 = new Formula("X+y+z+X+X");
			int i=0;
			foreach (string var in f1.GetVariables())
			{
				i++;
				Assert.IsTrue(Regex.IsMatch(var, @"[Xyz]"));
			}
			Assert.AreEqual(3, i);
		}
		/// <summary>
		/// basic test for to string
		/// </summary>
		[TestMethod]
		public void tostringTest()
		{
			Formula f1 = new Formula("2+3");
			Assert.IsTrue(f1.ToString() == "2+3");

		}
		/// <summary>
		/// tostring should omit spaces
		/// </summary>
		[TestMethod]
		public void tostringTest2()
		{
			Formula f1 = new Formula("2 + 3");
			Assert.IsTrue(f1.ToString() == "2+3");

		}
		/// <summary>
		/// tostring should 
		/// </summary>
		[TestMethod]
		public void tostringTest3()
		{
			Formula f1 = new Formula("2 + 3");
			Assert.IsTrue(f1.ToString() == "2+3");

		}
		[TestMethod]
		public void GetHashcodeTest()
		{
			Formula f1 = new Formula("x+2");
			Formula f2 = new Formula("x+2");
			Assert.IsTrue(f1.GetHashCode() == f2.GetHashCode());
		}
		/// <summary>
		/// two equal formulas are ==
		/// </summary>
		[TestMethod]
		public void TestOperatorEq()
		{
			Formula f1 = new Formula("2+3");
			Formula f2 = new Formula("2+3");
			Assert.IsTrue(f1 == f2);
		}
		/// <summary>
		/// one null and one formula are not ==
		/// </summary>
		[TestMethod]
		public void TestOperatorEq1()
		{
			Formula f1 = new Formula("2+3");
			Formula f2 = null;
			Assert.IsFalse(f1 == f2);
		}
		/// <summary>
		/// one null and one formula are not ==
		/// </summary>
		[TestMethod]
		public void TestOperatorEq2()
		{
			Formula f1 = null;
			Formula f2 = new Formula("2+3");
			Assert.IsFalse(f1 == f2);
		}
		/// <summary>
		/// two null formulae are ==
		/// </summary>
		[TestMethod]
		public void TestOperatorEq3()
		{
			Formula f1 = null;
			Formula f2 = null;
			Assert.IsTrue(f1 == f2);
		}
		/// <summary>
		/// two same formula are...the same
		/// </summary>
		[TestMethod]
		public void TestOperatorEq4()
		{
			Formula f1 = new Formula("2+3");
			Formula f2 = new Formula("2+3");
			Assert.IsTrue(f1 == f2);
		}
		/// <summary>
		/// two null formulae are ==, but not !=
		/// </summary>
		[TestMethod]
		public void TestOperatorNEq()
		{
			Formula f1 = null;
			Formula f2 = null;
			Assert.IsFalse(f1 != f2);
		}
		/// <summary>
		/// one null and one formula are !=
		/// </summary>
		[TestMethod]
		public void TestOperatorNEq2()
		{
			Formula f1 = null;
			Formula f2 = new Formula("2+3");
			Assert.IsTrue(f1 != f2);
		}
		/// <summary>
		/// one null and one formula are !=
		/// </summary>
		[TestMethod]
		public void TestOperatorNEq4()
		{
			Formula f1 = new Formula("2+3");
			Formula f2 = null;
			Assert.IsTrue(f1 != f2);
		}
		/// <summary>
		/// two different formula strings
		/// </summary>
		[TestMethod]
		public void TestOperatorNEq3()
		{
			Formula f1 = new Formula("2+3");
			Formula f2 = new Formula("2+4");
			Assert.IsTrue(f1 != f2);
		}
		/// <summary>
		/// simply testing addition
		/// </summary>
		[TestMethod]
		public void testEval()
		{
			Formula f1 = new Formula("2 + 3.5");
			Assert.AreEqual(5.5, f1.Evaluate(lookerupper));
		}
		/// <summary>
		/// vars
		/// </summary>
		[TestMethod]
		public void testEval2()
		{
			Formula f1 = new Formula("2 + 3.5 + x");
			Assert.AreEqual(7.5, f1.Evaluate(lookerupper));
		}
		/// <summary>
		///stress
		/// </summary>
		[TestMethod]
		public void testEval3stress()
		{
			Formula f1 = new Formula("2*(5+x)+(5-x*(x/x))");
			Assert.AreEqual(17.0, f1.Evaluate(lookerupper));
		}
		private Func<string, double> lookerupper = s => (s == "x") ? 2 : 0;
		
	}
}
