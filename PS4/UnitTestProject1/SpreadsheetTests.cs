using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SS;
using System.Collections.Generic;
using SpreadsheetUtilities;


namespace SpreadsheetTests
{
	[TestClass]
	public class SpreadsheetTests
	{
		public Spreadsheet sheet1;
		/// <summary>
		/// init sheet 1
		/// </summary>
		[TestInitialize]
		public void setup()
		{
			sheet1 = new Spreadsheet();
		}
		/// <summary>
		/// test construction basic. no args, no elements
		/// </summary>
		[TestMethod]
		public void TestConstructor()
		{
			List<string> stuff = new List<string>( new Spreadsheet().GetNamesOfAllNonemptyCells());
			Assert.AreEqual(0, stuff.Count);
		}
		/// <summary>
		/// gnoanc should return names of filled cells
		/// </summary>
		[TestMethod]
		public void TestGetNamesOfAllNonemptyCells()
		{
			sheet1.SetCellContents("A1", 1.2);
			Assert.AreEqual("A1", new List<string>(sheet1.GetNamesOfAllNonemptyCells())[0]);
			Assert.AreEqual(1, new List<string>(sheet1.GetNamesOfAllNonemptyCells()).Count);
		}
		/// <summary>
		/// if Have a cell with a formula in it
		/// </summary>
		[TestMethod]
		public void TestGetNamesOfAllNonemptyCells2()
		{
			sheet1.SetCellContents("A1", new Formula("x+1"));
			Assert.AreEqual("A1", new List<string>(sheet1.GetNamesOfAllNonemptyCells())[0]);
			Assert.AreEqual(1, new List<string>(sheet1.GetNamesOfAllNonemptyCells()).Count);
		}
		/// <summary>
		/// if Have a cell with a string in it
		/// </summary>
		[TestMethod]
		public void TestGetNamesOfAllNonemptyCells3()
		{
			sheet1.SetCellContents("A1", "x+1");
			Assert.AreEqual("A1", new List<string>(sheet1.GetNamesOfAllNonemptyCells())[0]);
			Assert.AreEqual(1, new List<string>(sheet1.GetNamesOfAllNonemptyCells()).Count);
		}
		/// <summary>
		/// if I add and clear a cell
		/// </summary>
		[TestMethod]
		public void TestGetNamesOfAllNonemptyCells4()
		{
			sheet1.SetCellContents("A1", "x+1");
			sheet1.SetCellContents("A1", string.Empty);
			
			Assert.AreEqual(0, new List<string>(sheet1.GetNamesOfAllNonemptyCells()).Count);
		}
		/// <summary>
		/// basic get cell contents test double
		/// </summary>
		[TestMethod]
		public void TestgetCellContents()
		{
			sheet1.SetCellContents("A1", 1.2);
			Assert.AreEqual(1.2, sheet1.GetCellContents("A1"));

		}
		/// <summary>
		/// If name is null or invalid, throws an InvalidNameException.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(InvalidNameException))]
		public void TestsetCellContentsDUBFail()
		{
			sheet1.SetCellContents("", 1.2);
		}
		/// <summary>
		/// If name is null or invalid, throws an InvalidNameException.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(InvalidNameException))]
		public void TestsetCellContentsDUBFail2()
		{
			sheet1.SetCellContents(null, 1.2);

		}
		/// <summary>
		/// basic get cell contents test string
		/// </summary>
		[TestMethod]
		public void TestsetsellContentsSTR()
		{
			sheet1.SetCellContents("A1", "goblin");
			Assert.AreEqual("goblin", sheet1.GetCellContents("A1"));

		}
		/// <summary>
		/// If name is null or invalid, throws an InvalidNameException.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(InvalidNameException))]
		public void TestsetCellContentsSTRfail()
		{
			sheet1.SetCellContents(null, "goblin");
		}
		/// <summary>
		/// If name is null or invalid, throws an InvalidNameException.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(InvalidNameException))]
		public void TestSetCellContentsSTRfail2()
		{
			//bad regex...
			sheet1.SetCellContents("a039490459sdjfgnjk4i56809", "goblin");
		}
		/// <summary>
		/// going crazy
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestSetCellContentsSTRfail30rfour()
		{
			string pomade = null;
			sheet1.SetCellContents("a039490459sdjfgnjk4i56809",pomade);
		}
		/// <summary>
		/// basic get cell contents test formula
		/// </summary>
		[TestMethod]
		public void TestGetCellContents3()
		{
			sheet1.SetCellContents("A1", new Formula("b1+c1"));
			Assert.AreEqual("b1+c1", sheet1.GetCellContents("A1").ToString());

		}
	
		/// <summary>
		/// set contents to a string
		/// </summary>
		[TestMethod]
		public void TestSetCellContentsString()
		{
			sheet1.SetCellContents("A1", "Pylon");
			Assert.AreEqual("Pylon", sheet1.GetCellContents("A1"));
		}
		/// <summary>
		/// set contents to a double
		/// </summary>
		[TestMethod]
		public void TestSetCellContentsdouble()
		{
			sheet1.SetCellContents("A1", 1.2);
			Assert.AreEqual(1.2, sheet1.GetCellContents("A1"));
		}
		/// <summary>
		/// set cell contents to a formula
		/// </summary>
		[TestMethod]
		public void TestSetCellContentsFormula()
		{
			sheet1.SetCellContents("A1", new Formula("2+B1"));
			Assert.AreEqual("2+B1",sheet1.GetCellContents("A1").ToString());
		}
		/// <summary>
		/// set cell contents to a formula
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TestSetCellContentsFormulaFail()
		{
			Formula f1 = null;
			sheet1.SetCellContents("A1", f1);
		}
		/// <summary>
		/// set cell contents to a formula
		/// </summary>
		[TestMethod]
		
		public void TestSetCellContentsFormula7867y789787()
		{
			sheet1.SetCellContents("A1", new Formula("28629837+3425"));
			sheet1.SetCellContents("A1", new Formula("a1+df2"));
			Assert.AreEqual("a1+df2", sheet1.GetCellContents("A1").ToString());
		}
		/// <summary>
		/// stressful test of return value of set cell contents
		/// </summary>
		[TestMethod]
		public void TestSetCellContentsFormula2()
		{
			sheet1.SetCellContents("B1", new Formula("A1*2"));
			sheet1.SetCellContents("C1", new Formula("B1+4"));
			List<string> dents = new List<string>(sheet1.SetCellContents("A1",4));
			Assert.IsTrue(dents.Contains("A1"));
			Assert.IsTrue(dents.Contains("B1"));
			Assert.IsTrue(dents.Contains("C1"));
		}
		[TestMethod]
		[ExpectedException(typeof(CircularException))]
		public void TestSetCellContentsFormula2Fail3343467()
		{
			sheet1.SetCellContents("B1", new Formula("A1"));
			sheet1.SetCellContents("A1", new Formula("B1"));
			Assert.IsTrue(new List<string>(sheet1.GetNamesOfAllNonemptyCells())[0] == "B1");
			Assert.IsTrue(new List<string>(sheet1.GetNamesOfAllNonemptyCells()).Count == 1);
		}
	}
}
