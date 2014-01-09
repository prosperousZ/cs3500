using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SS;
using SpreadsheetUtilities;
using System.Text.RegularExpressions;
using System.Globalization;
using System.IO;
using System.Xml;



namespace SS
{
	///<summary>
	/// spreadsheet consists of an infinite number of named cells.
	/// (we can't hold infinite cells, but we may be able to make it look infinite...)
	/// A string is a valid cell name if and only if:
	///   (1) its first character is an underscore or a letter
	///   (2) its remaining characters (if any) are underscores and/or letters and/or digits
	/// Note that this is the same as the definition of valid variable from the PS3 Formula class.
	/// 
	/// For example, "x", "_", "x2", "y_15", and "___" are all valid cell  names, but
	/// "25", "2x", and "[andpersand]" are not.  Cell names are case sensitive, so "x" and "X" are
	/// different cell names.
	/// 
	/// A spreadsheet contains a cell corresponding to every possible cell name.  (This
	/// means that a spreadsheet contains an infinite number of cells.)  In addition to 
	/// a name, each cell has a contents and a value.  The distinction is important.
	/// 
	/// The contents of a cell can be (1) a string, (2) a double, or (3) a Formula.  If the
	/// contents is an empty string, we say that the cell is empty.  (By analogy, the contents
	/// of a cell in Excel is what is displayed on the editing line when the cell is selected.)
	/// 
	/// In a new spreadsheet, the contents of every cell is the empty string.
	///  
	/// The value of a cell can be (1) a string, (2) a double, or (3) a FormulaError.  
	/// (By analogy, the value of an Excel cell is what is displayed in that cell's position
	/// in the grid.)
	/// 
	/// If a cell's contents is a string, its value is that string.
	/// 
	/// If a cell's contents is a double, its value is that double.
	/// 
	/// If a cell's contents is a Formula, its value is either a double or a FormulaError,
	/// as reported by the Evaluate method of the Formula class.  The value of a Formula,
	/// of course, can depend on the values of variables.  The value of a variable is the 
	/// value of the spreadsheet cell it names (if that cell's value is a double) or 
	/// is undefined (otherwise).
	/// 
	/// Spreadsheets are never allowed to contain a combination of Formulas that establish
	/// a circular dependency.  A circular dependency exists when a cell depends on itself.
	/// For example, suppose that A1 contains B1*2, B1 contains C1*2, and C1 contains A1*2.
	/// A1 depends on B1, which depends on C1, which depends on A1.  That's a circular
	/// dependency.
	/// </summary>
	public class Spreadsheet : AbstractSpreadsheet
	{
	
		/// <summary>
		/// Representation :
		/// function:
		/// DG contains all dependency data for the spreadsheet.
		/// if cell A1 depends on B1 and C1, DG contains the pairs (B1,A1) and (C1,A1)
		/// Invariant: 
		/// no cells are allowed to have a circular dependency
		/// 
		/// </summary>
		private DependencyGraph DG;
		/// <summary>
		/// Representation:
		/// function: 
		/// keys are the cells' names. values are the cells themselves.
		/// Invariant:
		/// There are no empty cells in Sheet. An empty cell is one that
		/// has its contents set to "". Cells set to contain "" should be removed
		/// from the keys of sheet and DG
		/// </summary>
		private Dictionary<string, Cell> Sheet;
		/// <summary>
		/// True if this spreadsheet has been modified since it was created or saved                  
		/// (whichever happened most recently); false otherwise.
		/// </summary>
		public override bool Changed
		{
			get;
			protected set;
		}
		/// <summary>
		///Your zero-argument constructor should create an empty spreadsheet that imposes no extra validity conditions,
		///normalizes every cell name to itself, and has version "default".zdfgdf
		/// </summary>
		public Spreadsheet()
			: base(s => true, s => s, "default")
		{
			Changed = false;
			Sheet = new Dictionary<string, Cell>();
			DG = new DependencyGraph();
		}
		/// <summary>
		/// three argument constructor
		/// </summary>
		/// <param name="_isValid">validity delegate</param>
		/// <param name="_normalize">normalization delegate</param>
		/// <param name="_version">version information</param>
		public Spreadsheet(Func<string, bool> _isValid, Func<string, string> _normalize, string _version)
			:base(_isValid,_normalize,_version)
		{
			Changed = false;
			Sheet = new Dictionary<string, Cell>();
			DG = new DependencyGraph();
			
		}
		/// <summary>
		/// reads a saved spreadsheet from a file (see the Save method) and uses it to construct a new spreadsheet.
		/// </summary>
		/// <param name="_filePath"></param>
		/// <param name="_isValid"></param>
		/// <param name="_normalize"></param>
		/// <param name="_version"></param>
		public Spreadsheet(string _filePath,Func<string, bool> _isValid, Func<string, string> _normalize, string _version)
			: base(_isValid, _normalize, _version)
		{
			
			Sheet = new Dictionary<string, Cell>();
			DG = new DependencyGraph();
			//load up specified sheet
			if(_version!=GetSavedVersion(_filePath))
				throw new SpreadsheetReadWriteException("Error: mismatched version");
			load(_filePath);
			Changed = false;
		}
		/// <summary>
		/// Returns the version information of the spreadsheet saved in the named file.
		/// If there are any problems opening, reading, or closing the file, the method
		/// should throw a SpreadsheetReadWriteException with an explanatory message.
		/// </summary>
		public override string GetSavedVersion(string filename)
		{
			//try to create a new file reader tied to the specified file
			try
			{
				using (XmlReader reader = XmlReader.Create(filename))
				{
					try
					{
						while (reader.Read())
						{
							if (reader.IsStartElement())
							{
								switch (reader.Name)
								{
									//the goal is to get the attribut of the first tag
									case "spreadsheet":
										return reader["version"];
									default:
										throw new SpreadsheetReadWriteException("Error: encountered malformed Xml while loading spreadsheet");
								}
							}
						}
					}
					catch( XmlException ex)
					{
						throw new SpreadsheetReadWriteException("error: cannot parse xml" + ex.Message);
					}
					//the only way to avoid returns and such in the while is to go around it
					//this happens when the .xml is empty/ read() is immediately false.
					throw new SpreadsheetReadWriteException("Error: empty xml document");
				}
			}
				//catch some of the invalid 
			catch (Exception ex)
			{
				if (ex is DirectoryNotFoundException)
					throw new SpreadsheetReadWriteException("error: invalid directory" + ex.Message);
				if (ex is FileNotFoundException)
					throw new SpreadsheetReadWriteException("error: specified file does not exist");
				throw new SpreadsheetReadWriteException("Congrats! You found a Bug!" +ex.Message); 
			}
		}
		private void load(string filename)
		{
			//using statements neatly close readers/writers
			using (XmlReader reader = XmlReader.Create(filename))
			{
				string name = "";
				string contents = "";
				while (reader.Read())
				{
					if (reader.IsStartElement())
					{
						
						switch (reader.Name)
						{
							case "spreadsheet":
								Version = reader["version"];
								break;
							case "cell":
								reader.Read();
								name = reader.ReadElementContentAsString();
								contents = reader.ReadElementContentAsString();
								SetContentsOfCell(name, contents);
								break;
							default:
								throw new SpreadsheetReadWriteException("error: malformed XML");
						}
						
					}
				}
			}

			
		}
		/// <summary>
		/// Writes the contents of this spreadsheet to the named file using an XML format.
		/// <remarks>
		/// The XML elements should be structured as follows:
		/// 
		/// <spreadsheet version="version information goes here">
		/// 
		/// <cell>
		/// <name>
		/// cell name goes here
		/// </name>
		/// <contents>
		/// cell contents goes here
		/// </contents>    
		/// </cell>
		/// 
		/// </spreadsheet>
		/// 
		/// There should be one cell element for each non-empty cell in the spreadsheet.  
		/// If the cell contains a string, it should be written as the contents.  
		/// If the cell contains a double d, d.ToString() should be written as the contents.  
		/// If the cell contains a Formula f, f.ToString() with "=" prepended should be written as the contents.
		/// 
		/// </remarks>
		/// </summary>
		/// <exception cref="SpreadsheetReadWriteException">
		/// If there are any problems opening, writing, or closing the file, the method should throw a
		/// SpreadsheetReadWriteException with an explanatory message.
		/// </exception>
		public override void Save(string filename)
		{
			try
			{
				using (XmlWriter writer = XmlWriter.Create(filename))
				{
					writer.WriteStartDocument();
					writer.WriteStartElement("spreadsheet");
					writer.WriteAttributeString("version", Version);
					foreach (Cell cell in Sheet.Values)
					{
						writer.WriteStartElement("cell");
						writer.WriteElementString("name", cell.Name);
						writer.WriteElementString("contents", writeContents(cell.Contents));
						writer.WriteEndElement();
					}
					writer.WriteEndElement();
					writer.WriteEndDocument();
					
				}
			}
			catch { throw new SpreadsheetReadWriteException("error writing to spreadsheet file"); }
			
		}
		/// <summary>
		/// converts doubles into their equivalent strings (roughly)
		/// doesn't do anything to strings
		/// converts a formula into the form "=[formula]"
		/// </summary>
		/// <param name="cellContents">Object to be converted to an appropriate string</param>
		/// <exception cref="SpreadsheetReadWriteException">thrown if cellcontents can't be written (isn't correct type)</exception>
		/// <returns>acceptable string version of the object cellContents</returns>
		private string writeContents(object cellContents)
		{
			if (cellContents is Formula)
				return "=" + cellContents.ToString();
			if (cellContents is double)
				return cellContents.ToString();
			if (cellContents is string)
				return cellContents as string;
			throw new SpreadsheetReadWriteException("error writing Cell Contents");
		}
		private object readContents(string objInfo)
		{
			double retDouble;
			if (Double.TryParse(objInfo,out retDouble))
				return retDouble;
			if (objInfo[0] == '=')
				return new Formula(objInfo.Substring(1));
			return objInfo;
		}
		/// <summary>
		/// If name is null or invalid, throws an InvalidNameException.
		/// 
		/// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
		/// value should be either a string, a double, or a SpreadsheetUtilities.FormulaError.
		/// </summary>
		public override object GetCellValue(string name)
		{
			
			if(name==null||!Regex.IsMatch(name,@"[a-zA-Z]+\d+")||!IsValid(Normalize(name)))
				throw new InvalidNameException();
			if (!Sheet.ContainsKey(name))
				return string.Empty;
			return Sheet[name].Value;
		
		}
		/// <summary>
		/// looks up the dependency info of the cell with the given name
		/// </summary>
		/// <param name="name"> name of a cell already in the sheet</param>
		/// <returns> double: the value of the cell</returns>
		public double lookerupper(string name)
		{
			if (Sheet.ContainsKey(name))
			{
				object concern  = Sheet[name].Value;
				if (concern is double)
					return (double)concern;
			}
			throw new ArgumentException("Error: cell value was not a double");
		}
		/// <summary>
		/// If content is null, throws an ArgumentNullException.
		/// 
		/// Otherwise, if name is null or invalid, throws an InvalidNameException.
		/// 
		/// Otherwise, if content parses as a double, the contents of the named
		/// cell becomes that double.
		/// 
		/// Otherwise, if content begins with the character '=', an attempt is made
		/// to parse the remainder of content into a Formula f using the Formula
		/// constructor.  There are then three possibilities:
		/// 
		///   (1) If the remainder of content cannot be parsed into a Formula, a 
		///       SpreadsheetUtilities.FormulaFormatException is thrown.
		///       
		///   (2) Otherwise, if changing the contents of the named cell to be f
		///       would cause a circular dependency, a CircularException is thrown.
		///       
		///   (3) Otherwise, the contents of the named cell becomes f.
		/// 
		/// Otherwise, the contents of the named cell becomes content.
		/// 
		/// If an exception is not thrown, the method returns a set consisting of
		/// name plus the names of all other cells whose value depends, directly
		/// or indirectly, on the named cell.
		/// 
		/// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
		/// set {A1, B1, C1} is returned.
		/// </summary>
		public override ISet<string> SetContentsOfCell(string name, string content)
		{
			//prelim content check
			if (content == null)
				throw new ArgumentNullException();
			//check name arg
			if (name == null || !Regex.IsMatch(name = Normalize(name), @"[a-zA-Z]+\d+"))
				throw new InvalidNameException();
			//is the name good according to user??
			if (IsValid(name))
			{
				if (content == "")
				{
					Sheet.Remove(name);
					DG.ReplaceDependents(name, new HashSet<string>());
					return new HashSet<string>(GetCellsToRecalculate(name));
				}
				if (content[0] == '=')
					return SetCellContents(name, new Formula(content.Substring(1),Normalize,IsValid));
				double toAdd;
				try
				{
					NumberStyles styles = NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
					toAdd = Double.Parse(content, styles);
					
					return SetCellContents(name, toAdd);
				}
					//unsuccessful parse jumps here
				catch
				{
					return SetCellContents(name,content);
				}
				

			}
			throw new InvalidNameException();
		}
		/// <summary>
		/// Enumerates the names of all the non-empty cells in the spreadsheet.
		/// </summary>
		public override IEnumerable<string> GetNamesOfAllNonemptyCells()
		{
			foreach (KeyValuePair<string, Cell> entry in Sheet)
			{
				if (entry.Value.Contents is string)
				{
					if (String.Compare(entry.Value.Contents as string, string.Empty) != 0)
					{
						yield return entry.Key;
					}
				}
				else
				{
					yield return entry.Key;
				}
			}
		}
		/// <summary>
		/// returns contents of cell
		///  The returnvalue should be either a string, a double, or a Formula.
		/// </summary>
		/// <exception cref="InvalidNameException">If name is null or invalid, throws an InvalidNameException.</exception>
		/// <returns>returns the contents (as opposed to the value) of the named cell.</returns>
		public override object GetCellContents(string name)
		{
			if (String.Compare(name,null)==0 || !Regex.IsMatch(name=Normalize(name), @"[a-zA-Z_](?: [a-zA-Z_]|\d)*"))
			{
				throw new InvalidNameException();
			}
			if (!Sheet.ContainsKey(name))
				return string.Empty;
			return Sheet[name].Contents;
		}
		/// <summary>
		/// If name is null or invalid, throws an InvalidNameException.
		/// 
		/// Otherwise, the contents of the named cell becomes number.  The method returns a
		/// set consisting of name plus the names of all other cells whose value depends, 
		/// directly or indirectly, on the named cell.
		/// 
		/// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
		/// set {A1, B1, C1} is returned.
		/// </summary>
		/// <remarks>
		/// Note:structure is backed by an underlying DG
		/// </remarks>
		/// <exception cref="InvalidNameException">If name is null or invalid, throws an InvalidNameException.</exception>
		/// <returns>
		/// The method returns a
		/// set consisting of name plus the names of all other cells whose value depends, 
		/// directly or indirectly, on the named cell.
		/// </returns>
		protected override ISet<string> SetCellContents(string name, double number)
		{
			//If name is null or invalid, throws an InvalidNameException
			if (name == null||!Regex.IsMatch(name, @"[a-zA-Z]+\d+"))
			{
				throw new InvalidNameException();
			}
			//we are going to "fill" a cell. therefore, we can actually add it to Sheet
			if (Sheet.ContainsKey(name))
				Sheet[name].Contents = number;
			else
				Sheet.Add(name, new Cell(name, number,lookerupper));
			Changed = true;
			
			DG.ReplaceDependents(name, new HashSet<string>());
			foreach (string gor in GetCellsToRecalculate(name))
			{
				Sheet[gor].Contents = Sheet[gor].Contents;
			}
			HashSet<string> dents = new HashSet<string>(GetCellsToRecalculate(name));
			Changed = true;
			return dents;
		}
		/// <summary>
		/// If text is null, throws an ArgumentNullException.
		/// 
		/// Otherwise, if name is null or invalid, throws an InvalidNameException.
		/// 
		/// Otherwise, the contents of the named cell becomes text.  The method returns a
		/// set consisting of name plus the names of all other cells whose value depends, 
		/// directly or indirectly, on the named cell.
		/// 
		/// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
		/// set {A1, B1, C1} is returned.
		/// </summary>
		/// <exception cref="ArgumentNullException">If text is null, throws an ArgumentNullException.</exception>
		/// <exception cref="InvalidNameException">Otherwise, if name is null or invalid, throws an InvalidNameException.</exception>
		protected override ISet<string> SetCellContents(string name, string text)
		{
			if(text == null){
				throw new ArgumentNullException();
			}
			if (name == null || !Regex.IsMatch(name, @"[a-zA-Z]+\d+"))
			{
				throw new InvalidNameException();
			}

			if (Sheet.ContainsKey(name))
				Sheet[name].Contents = text;
			else
			{
				Sheet.Add(name, new Cell(name, text, lookerupper));
			}
			//clean dents, then it will be ready to add new dents later if it changes
 			//to a formula or vice versa.
			DG.ReplaceDependents(name, new HashSet<string>());
			foreach (string nombre in GetCellsToRecalculate(name))
			{
				//contents setter recalculates value.
				Sheet[nombre].Contents = Sheet[nombre].Contents;
			}
			Changed = true;
			HashSet<string> dents = new HashSet<string>(GetCellsToRecalculate(name));
			dents.Add(name);
			return dents;
		}
		/// <summary>
		/// sets the cell, name, to contain a specified formula
		/// </summary>
		/// <remarks>
		///  If the formula parameter is null, throws an ArgumentNullException.
		/// 
		/// Otherwise, if name is null or invalid, throws an InvalidNameException.
		/// 
		/// Otherwise, if changing the contents of the named cell to be the formula would cause a 
		/// circular dependency, throws a CircularException.  (No change is made to the spreadsheet.)
		/// 
		/// Otherwise, the contents of the named cell becomes formula.  The method returns a
		/// Set consisting of name plus the names of all other cells whose value depends,
		/// directly or indirectly, on the named cell.
		/// 
		/// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
		/// set {A1, B1, C1} is returned.
		/// circ dependency example:
		/// when setting b1 to a1
		/// a1 = b1		DG has pair (b1,a1)
		/// b1 = a1		DG can't be allowed to add(a1,b1)
		/// </remarks>
		/// <exception cref="ArgumentNullException"> If the formula parameter is null, throws an ArgumentNullException.</exception>
		/// <exception cref="InvalidNameException"> Otherwise, if name is null or invalid, throws an InvalidNameException.</exception>
		/// <exception cref="CircularException">
		/// Otherwise, if changing the contents of the named cell to be the formula would cause a 
		/// circular dependency, throws a CircularException.  (No change is made to the spreadsheet.) 
		/// </exception>
		/// <returns>
		/// The method returns a
		/// Set consisting of name plus the names of all other cells whose value depends,
		/// directly or indirectly, on the named cell.
		/// </returns>
		protected override ISet<string> SetCellContents(string name, Formula formula)
		{
			//If the formula parameter is null, throws an ArgumentNullExceptio
			if (formula == null)
			{
				throw new ArgumentNullException();
			}
			//If name is null or invalid, throws an InvalidNameException
			if (name == null || !Regex.IsMatch(name, @"[a-zA-Z]+\d+"))
			{
				throw new InvalidNameException();
			}
			IEnumerable<string> storedDents = DG.GetDependents(name);
			DG.ReplaceDependents(name, new HashSet<string>());
			foreach (string var in formula.GetVariables())
			{
				try
				{
					DG.AddDependency(name, var);
				}
				catch (InvalidOperationException)
				{
					DG.ReplaceDependents(name, storedDents);
					throw new CircularException();
				}
			}
			
			if (Sheet.ContainsKey(name))
				Sheet[name].Contents = formula;
			else
				Sheet.Add(name, new Cell(name, formula,lookerupper));
			foreach (string nombre in GetCellsToRecalculate(name))
			{
				//contents setter recalculates value.
				Sheet[nombre].Contents = Sheet[nombre].Contents;
			}
			Changed = true;
			HashSet<string> toreturn = new HashSet<string>(GetCellsToRecalculate(name));
			toreturn.Add(name);
			return toreturn;
		}
		
		/// <summary>
		/// If name is null, throws an ArgumentNullException.
		/// 
		/// Otherwise, if name isn't a valid cell name, throws an InvalidNameException.
		/// 
		/// Otherwise, returns an enumeration, without duplicates, of the names of all cells whose
		/// values depend directly on the value of the named cell.  In other words, returns
		/// an enumeration, without duplicates, of the names of all cells that contain
		/// formulas containing name.
		/// 
		/// For example, suppose that
		/// A1 contains 3
		/// B1 contains the formula A1 * A1
		/// C1 contains the formula B1 + A1
		/// D1 contains the formula B1 - C1
		/// The direct dependents of A1 are B1 and C1
		/// </summary>
		///------------------protected-----------------------------------/// 
		protected override IEnumerable<string> GetDirectDependents(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException();
			}
			else if (!Regex.IsMatch(name, @"[a-zA-Z]+\d+"))
			{
				throw new InvalidNameException();
			}
			return DG.GetDependees(name);
		}

		
		/// <summary>
		/// a class for associating cell organs
		/// </summary>
		private class Cell
		{
			/// <summary>
			/// the name of the cell
			/// </summary>
			public String Name { get; private set; }
			/// <summary>
			/// the contents of the cell (string, double, or formula)
			/// </summary>
			private object _contents;
			public object Contents
			{
				get { return _contents; }
				set
				{
					_value = value;
					if (value is Formula)
					{
						_value = (_value as Formula).Evaluate(MyLookup);
					}
					_contents = value;
				}
			}
			/// <summary>
			/// holds the double, string, or formulaerrror representing the Cell Value
			/// </summary>
			private object _value;
			public object Value{
				get{return _value;}
				private set { _value = value; }
			}
			
			public Func<string,double> MyLookup{get;private set;}
			/// <summary>
			/// basic constructor: fills the properties name and contents
			/// </summary>
			/// <param name="_name"> new name</param>
			/// <param name="_contents">new contents</param>
			/// <param name="_lookup">lookup function to evaluate Formula for value</param>
			public Cell(string _name, object _contents, Func<string,double> _lookup)
			{
				Name = _name;
				MyLookup = _lookup;
			//this sets value
				Contents = _contents;
			
			}
		
		}
		
		
	}
	
}
