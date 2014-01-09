using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SS;
using System.Collections.Generic;
using SpreadsheetUtilities;
using System.IO;


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
		/// test constructors. successfully run each
		/// </summary>
		[TestMethod]
		public void TestConstructor()
		{
			
			//just some stuff with filewriting
			Assert.IsTrue(sheet1.IsValid("any old string"));
			Assert.IsTrue(sheet1.Normalize("dead") == "dead");
			Assert.IsTrue(sheet1.Version == "default");
			
			//test 3 arg constructor
			sheet1 = new Spreadsheet(s => (s.Length >= 2) ? true : false, 
				s => s.Replace(" ", ""),
				"version1");
			Assert.IsTrue(sheet1.IsValid("A1"));
			Assert.IsFalse(sheet1.IsValid("A"));
			Assert.IsTrue(sheet1.Normalize("d e a d") == "dead");
			Assert.IsTrue(sheet1.Version == "version1");
			sheet1.SetContentsOfCell("A     1","loaded!");

			string savePath = "save 1.xml";
			sheet1.Save(savePath);
			sheet1 = new Spreadsheet(
				savePath,
				s => (s.Length >= 2) ? true : false, 
				s => s.Replace(" ", ""),
				"version1");
			Assert.AreEqual("loaded!",(string)sheet1.GetCellContents("A1"));
		}
		/// <summary>
		/// try setting a cell to each type
		/// </summary>
		[TestMethod]
		public void testGetCellContents()
		{
			sheet1.SetContentsOfCell("A1", "78.5");
			Assert.AreEqual(78.5,sheet1.GetCellContents("A1"));
			sheet1.SetContentsOfCell("A1", "Imaginative string");
			Assert.AreEqual("Imaginative string", sheet1.GetCellContents("A1"));
			sheet1.SetContentsOfCell("A1","=B1+B1+(c1+235)");
			Assert.AreEqual("B1+B1+(c1+235)", sheet1.GetCellContents("A1").ToString());
			Assert.AreEqual(1, new List<string>(sheet1.GetNamesOfAllNonemptyCells()).Count);
		}
		/// <summary>
		/// tests to see if we can set cell contents correctly.
		/// unfortunately the only way to do this test is with getcontents()
		/// </summary>
		[TestMethod]
		public void testSetCellContents()
		{
			//string
			Assert.IsFalse(sheet1.Changed);
			sheet1.SetContentsOfCell("A1", "bear hug a grizzly");
			Assert.AreEqual("bear hug a grizzly", sheet1.GetCellContents("A1"));
			Assert.IsTrue(sheet1.Changed);

			//double
			sheet1 = new Spreadsheet();
			Assert.IsFalse(sheet1.Changed);
			sheet1.SetContentsOfCell("A1", "45.9e+4");
			Assert.AreEqual((double)45.9e+4,sheet1.GetCellContents("A1"));
			Assert.IsTrue(sheet1.Changed);

			//set up a good test for the formula version
			sheet1 = new Spreadsheet();
			Assert.IsFalse(sheet1.Changed);
			sheet1.SetContentsOfCell("A1", "5.5");
			sheet1.SetContentsOfCell("C1", "4.5");
			sheet1.SetContentsOfCell("D1", "=A1+5+C1");
			//correct contents??
			Assert.AreEqual("A1+5+C1",sheet1.GetCellContents("D1").ToString());
			Assert.IsTrue(sheet1.Changed);
			//test indirect dependency craziness.
			//also test resetting cells.
			sheet1.SetContentsOfCell("A1","=B1");
			sheet1.SetContentsOfCell("B1","=C1");
			sheet1.SetContentsOfCell("D1", "");
			sheet1.SetContentsOfCell("C1","=D1");
			sheet1.SetContentsOfCell("D1","=E1");
			HashSet<string> didReturn = new HashSet<string>(sheet1.SetContentsOfCell("E1","5"));
			HashSet<string> shouldReturn = new HashSet<string>{"E1","D1","C1","B1","A1"};
			//don't stack up dependencies when resetting.
			sheet1 = new Spreadsheet();
			sheet1.SetContentsOfCell("A1", "=B1+C1");
			sheet1.SetContentsOfCell("A1", "=B1+G1");
			sheet1.SetContentsOfCell("A1", "=B1+Z1");
			sheet1.SetContentsOfCell("A1", "=B1+Y1");
			sheet1.SetContentsOfCell("B1","5");
			sheet1.SetContentsOfCell("Y1","6");

			Assert.AreEqual(11.0, sheet1.GetCellValue("A1"));
			sheet1.SetContentsOfCell("Z1", "=A1");
			Assert.AreEqual(11.0, sheet1.GetCellValue("Z1"));
			Assert.IsTrue(shouldReturn.SetEquals(didReturn));
			
			//checking name validation
			
		}

		/// <summary>
		/// test get value
		/// relies on set
		/// </summary>
		[TestMethod]
		public void testGetCellValue()
		{
			//string
			sheet1.SetContentsOfCell("A1", "bear hug a grizzly");
			Assert.AreEqual("bear hug a grizzly",sheet1.GetCellValue("A1"));

			//double
			sheet1.SetContentsOfCell("A1", "45.9e+4");
			Assert.AreEqual(459000.0, (double)sheet1.GetCellValue("A1"),1e-9);

			//set up a good test for the formula version
			sheet1.SetContentsOfCell("A1", "5.5");
			sheet1.SetContentsOfCell("C1", "4.5");
			sheet1.SetContentsOfCell("D1", "=A1+C1+3");
			//correct value??
			Assert.AreEqual(13.0,(double)sheet1.GetCellValue("D1"));
			sheet1.SetContentsOfCell("C1", "=E1+F1");
			sheet1.SetContentsOfCell("E1", "3");
			sheet1.SetContentsOfCell("F1", "4");
			Assert.AreEqual(15.5, (double)sheet1.GetCellValue("D1"));

		}
		/// <summary>
		/// No matter what the contents of a cell are, as long as there is something in it,
		/// it should be in the list returned by this method.
		/// </summary>
		[TestMethod]
		public void testgetNamesOfNonEmptyCells()
		{
			sheet1.SetContentsOfCell("A1", "Guns Are Drawn");
			List<string> listForTest = new List<string>(sheet1.GetNamesOfAllNonemptyCells());
			Assert.AreEqual("A1", listForTest[0]);
			sheet1.SetContentsOfCell("B1", "123.346");
			listForTest = new List<string>(sheet1.GetNamesOfAllNonemptyCells());
			Assert.AreEqual("B1",listForTest[1]);
			sheet1.SetContentsOfCell("C1", "=A1+B1");
			listForTest = new List<string>(sheet1.GetNamesOfAllNonemptyCells());
			Assert.AreEqual("C1", listForTest[2]);
		}
		/// <summary>
		/// test save. kind of already did this...
		/// </summary>
		[TestMethod]
		public void testSave()
		{
			string thing = AppDomain.CurrentDomain.BaseDirectory + "\\save 1";
			sheet1 = new Spreadsheet(
				thing,
				s => (s.Length >= 2) ? true : false,
				s => s.Replace(" ", ""),
				"version1");
			sheet1.SetContentsOfCell("A1","=c3+c4");
			
			sheet1.Save(thing);
			sheet1 = new Spreadsheet(
				thing,
				s => (s.Length >= 2) ? true : false, 
				s => s.Replace(" ", ""),
				"version1");
			Assert.AreEqual(new Formula("c3+c4"),sheet1.GetCellContents("A1"));
		}
		[TestMethod]
		public void testNesting() 
		{
			sheet1.SetContentsOfCell("A1", "=1+(1+(1+(1+(1+2+3+B1))))");
			sheet1.SetContentsOfCell("B1", "4");
			Assert.AreEqual(14.0, sheet1.GetCellValue("A1"));
		}

	}
}
