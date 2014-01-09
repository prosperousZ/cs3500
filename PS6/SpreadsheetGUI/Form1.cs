using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SS;
using System.Text.RegularExpressions;
using SpreadsheetUtilities;

namespace SpreadsheetGUI
{
	/// <summary>
	/// A form with a spreadsheet.
	/// By Dylan Noaker
	/// </summary>
	public partial class SpreadSheetForm : Form
	{
		/// <summary>
		/// Representation Invariant:
		/// Only one cell named [column][row] may be selected
		/// function:
		/// these ints name that column (see ConvertRowCol)
		/// </summary>
		private int col;
		/// <summary>
		/// Representation Invariant:
		/// Only one cell named [column][row] may be selected
		/// function:
		/// these ints name that column (see ConvertRowCol)
		/// </summary>
		private int row;
		/// <summary>
		///Representation Invariant:
		///The cells in the GUI are connected by formulae
		///Function:
		///for 9 weeks, I have been making a class that logically unites named cells
		///hopefully, It works well enough!
		/// </summary>
		Spreadsheet sheetModel;
		/// <summary>
		/// Representation Invariant
		/// A user can either move in the spreadsheet XOR edit a cell
		/// NOT BOTH.
		/// Function: this boolean governs what the keyboard does in the form.
		/// </summary>
		private bool PanelFocus;
		
		/// <summary>
		/// the actual name of the doc
		/// defaults to sheet(count).
		/// </summary>
		private  string docName = "sheet"+SSContextSingleton.getContext().formCount;
		

		//The following are globaly declared components for building background spreadsheet instances
		
		/// <summary> a string representing the currently specified filepath for saving this spreadsheet instance </summary>
		private string filePath;
		/// <summary> validation function for background spreadsheet</summary>
		private readonly Func<string, bool> Validator;
		/// <summary> normalization function for background spreadsheet</summary>
		private readonly Func<string, string> Normalizer;
		/// <summary> version info for background spreadsheet</summary>
		private const string VERSION = "PS6";


		/// <summary>
		/// #ctor
		/// </summary>
		public SpreadSheetForm()
		{
			Validator = s => Regex.IsMatch(s, @"[A-Z][1-9][0-9]?");
			Normalizer = s => s.ToUpper();
			sheetModel = new Spreadsheet(Validator,Normalizer,VERSION);
			InitializeComponent();
			this.Text = "SpreadSheet" + "- "+docName;
		}

		public SpreadSheetForm(string p)
		{
			filePath = p;
			Validator = s => Regex.IsMatch(s, @"[A-Z][1-9][0-9]?");
			Normalizer = s => s.ToUpper();
			sheetModel = new Spreadsheet(filePath,Validator, Normalizer, VERSION);
			InitializeComponent();
			this.Text = "SpreadSheet" + "- " + docName;
			open_and_paint();
		}
		
		
		/// <summary>
		/// occurs when I click save or hit Ctrl+s
		/// should spawn a file dialog
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void saveButton_Click(object sender, EventArgs e)
		{
			//If a filepath is not yet specified, prompt user to do so
			if (filePath == null)
			{
				saveAsToolStripMenuItem_Click(sender, e);
			}
			else
			{
				try
				{
					backgroundWorker1.RunWorkerAsync();
					Console.WriteLine("saved");
				}
				catch (SpreadsheetReadWriteException ex)
				{
					MessageBox.Show("Error: your file could not be saved:\n" + ex.Message);
				}
			}
		}
		
		
		/// <summary>
		/// what happens when the spreadsheet loads the panel
		/// initially, I want users to navigate cells.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Panel_Load(object sender, EventArgs e)
		{
			PanelFocus = true;
		}
		/// <summary>
		/// I'm using this event to handle arrow keys. probably bad practice...
		/// oh well
		/// </summary>
		/// <param name="msg">I don't know what this does</param>
		/// <param name="keyData">Data about the keypress</param>
		/// <returns>a boolean...for obvious reasons...</returns>
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			
			if (PanelFocus)
			{
				switch (keyData)
				{
					case Keys.Down:
						Panel.SetSelection(col, ++row);
						this.Panel_SelectionChanged(Panel);
						break;
					case Keys.Up:
						Panel.SetSelection(col, --row);
						this.Panel_SelectionChanged(Panel);
						break;
					case Keys.Left:
						Panel.SetSelection(--col, row);
						this.Panel_SelectionChanged(Panel);
						break;
					case Keys.Right:
						Panel.SetSelection(++col, row);
						this.Panel_SelectionChanged(Panel);
						break;
					default:
						PanelFocus = false;
						CellContents.Text += new string((char)keyData, 1);
						CellContents.Select(CellContents.Text.Length, 0);
						//Panel.SetValue(col, row, CellContents.Text);
						break;
				}
				this.CellContents.Select();
				return true;
				
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}
		/// <summary>
		/// gives open file dialog
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void openButton_Click(object sender, EventArgs e)
		{
			openFileDialog1.ShowDialog();
		}
		/// <summary>
		/// when closing, I should ask the user if they want to keep changes
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void closeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Close();
		}
		/// <summary>
		/// what happens when I press enter/return and the edit box is filled
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CellContents_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)Keys.Return || e.KeyChar == (char)Keys.Enter)
			{
				try
				{
					sheetModel.SetContentsOfCell(ColRow_To_string(col, row), CellContents.Text);
					CellVal.Text = PrintableValue(sheetModel.GetCellValue(ColRow_To_string(col, row)));

				}
				catch { CellVal.Text = "Eval Error"; return; }
					Panel.SetValue(col, row, CellContents.Text);
					//CellContents.Clear();
					this.Panel.Select();
					PanelFocus = true;
			}
			
		}
		/// <summary>
		/// helper to turn col/row into something the spreadsheet model can
		/// handle
		/// </summary>
		/// <param name="colNumber"></param>
		/// <param name="rowNumber"></param>
		/// <returns></returns>
		private string ColRow_To_string(int colNumber, int rowNumber)
		{
			Console.WriteLine("" + (char)(colNumber + 65) + "" + rowNumber.ToString());
			return new string((char)(colNumber + 65), 1) + (rowNumber + 1).ToString();
		}
		/// <summary>
		/// clicking the edit box should give it focus
		/// or at take focus from Panel
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CellContents_MouseClick(object sender, MouseEventArgs e)
		{
			PanelFocus = false;
		}
		/// <summary>
		/// makes a printable value out of a cellvalue object
		/// </summary>
		/// <param name="CellValue"></param>
		/// <returns></returns>
		private string PrintableValue(object CellValue)
		{
			if (CellValue is string){
				return CellValue as string;
			}
			else if (CellValue is double)
			{
				return "" + CellValue;
			}
			else 
			{
				//~~~~~~~~~~~~~~~~~~~~stinky
				FormulaError toreturn = (FormulaError)CellValue;
				return toreturn.Reason;
			}
			
		}
		/// <summary>
		/// makes a printable version of cell contents
		/// SOOOOOOOO not the same as tostring
		/// </summary>
		/// <param name="CellContents"></param>
		/// <returns></returns>
		private string PrintableContents(object CellContents)
		{
			if (CellContents is string)
			{
				return CellContents as string;
			}
			else if (CellContents is double)
			{
				return "" + CellContents;
			}
			else
				return "=" + CellContents.ToString();

		}
		
		/// <summary>
		/// What happens when user clicks New
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void newToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SSContextSingleton.getContext().RunForm(new SpreadSheetForm());
		}
		/// <summary>
		/// what happens when user clicks save as
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			saveFileDialog1.ShowDialog();
			//wait for user
		}
		/// <summary>
		/// tied to async work. Saving, in worst case, could be sort of time consuming.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
		{
			lock (sendlock)
			{
				try { sheetModel.Save(filePath); }
				catch (SpreadsheetReadWriteException ex)
				{
					MessageBox.Show("Could not save " + docName + "\n" + ex.Message);
				}
			}
		}
		private readonly object sendlock = new object();
		

		private void Panel_Click(object sender, EventArgs e)
		{
			Console.WriteLine("clicked");
		}
		/// <summary>
		/// users can still click-select cells
		/// </summary>
		/// <remarks>
		/// This is actually trickier than it seems. We need to update the 
		/// cell value and cell content txt boxes, update col,row,
		/// and ensure that focus is passed to the panel. 
		/// Deselecting should release(write) the edit box text to the cell
		/// </remarks>
		/// <param name="sender"></param>
		private void Panel_SelectionChanged(SpreadsheetPanel sender)
		{
			
			Panel.GetSelection(out col, out row);
			
			
			CellNameBox.Text = this.ColRow_To_string(col, row);

			CellVal.Text = this.PrintableValue(sheetModel.GetCellValue(CellNameBox.Text));
			
			string To_cell_content_box;
			Panel.GetValue(col, row, out To_cell_content_box);
			CellContents.Text = To_cell_content_box;
			CellContents.Select(0, CellContents.Text.Length);


			

			this.Panel.Select();
			PanelFocus = true;
		}
		/// <summary>
		/// when user hits save in the dialog
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
		{
			try
			{
				filePath = saveFileDialog1.FileName;
				backgroundWorker1.RunWorkerAsync();
				Console.WriteLine("saved");
				
				getDocName();
				this.Text = docName;
				Console.Beep();
			}
			catch(SpreadsheetReadWriteException ex)
			{ MessageBox.Show("Encountered error while saving\n" + ex.Message); }
		}
		/// <summary>
		/// reads the document name off of the end of the fileanme
		/// </summary>
		private void getDocName()
		{
			char[] pathArray= filePath.ToCharArray();
			int i = pathArray.Length - 1;
			char currLetter = pathArray[i];
			while (currLetter != '/' && currLetter != '\\')
			{
				docName =docName+ currLetter;
				currLetter = pathArray[--i]; 
			}
			docName.Reverse();
		}
		/// <summary>
		/// When the user clicks ok we need to
		/// 1. load in the new SSmodel
		/// 2. display the contents/values/ what have you contained within the model.
		/// 3. handle any exceptions this may cause
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
		{
			try
			{
				SpreadSheetForm leform = new SpreadSheetForm(openFileDialog1.FileName);
				SpreadsheetGUI.SSContextSingleton.getContext().RunForm(leform);
				open_and_paint();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error opening file\n" + ex.Message);
			}


		}
		
		/// <summary>
		/// A helper method for opening and drawing a spreadsheet
		/// use invoke??
		/// </summary>
		private void open_and_paint()
		{
			int RowToWrite;
			int ColToWrite;
			foreach (string cellName in sheetModel.GetNamesOfAllNonemptyCells())
			{
				Cell_to_ColRow(out ColToWrite, out RowToWrite, cellName);
				//need another version of printable value that does contents.
				Panel.SetValue(ColToWrite, RowToWrite, PrintableContents(sheetModel.GetCellContents(cellName)));
			}
		}
		/// <summary>
		/// turns cellname into (col,row) coordinates
		/// </summary>
		/// <param name="_col"></param>
		/// <param name="_row"></param>
		/// <param name="cellname"></param>
		private void Cell_to_ColRow(out int _col, out int _row,string cellname) 
		{
			_col = (int)cellname[0]-65;
			int.TryParse(cellname.Substring(1), out _row);
			_row--;
		}
		/// <summary>
		/// updates cells inline with edit box
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CellContents_TextChanged(object sender, EventArgs e)
		{
			Panel.SetValue(col, row, CellContents.Text);	
		}
		/// <summary>
		/// prompts user to save changes if they changed beforc
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SpreadSheetForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (sheetModel.Changed == true)
			{
				var BoxResult = MessageBox.Show("Save Changes?", "", MessageBoxButtons.YesNo);
				if (BoxResult == DialogResult.Yes)
					this.saveButton_Click(sender, e);
			}
			//continue closing
		}
		/// <summary>
		/// This is the worst function I have ever written
		/// shows a messagebox with all of the help info in it.
		/// technically there is a class that does this with html docs but
		/// I don't want to deal with that.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Halpbutton_Click(object sender, EventArgs e)
		{
			//
			MessageBox.Show(
				"SpreadSheet Help\n"+
				"Navigating the Spreadsheet:\n"+
				"You can navigate the spreadsheet by clicking on a cell or using the arrowkeys\n" +
				"Don't let the selection box go off screen. I don't know How to deal with it.\n" +
				"\n"+
				"Editing cell contents:\n" +
				"Start typing! it should work. If it doesn't, try clicking on the box labeled CellContents\n" +
				"to create a formula, prefix the formula with an \"=\".\n" +
				"i.e. =A1+b1\n"+
				"\n" +
				"CellValues:"+
				"Cellvalues are displayed in the box labeled \"Cell=\"\n" +
				"\n" +
				"CellNames:\n"+
				"the name of the currently selected cell is displayed in the box labeled \"CellName\"\n" +
				"in the top left.\n"+
				"\n" +
				"File menu:\n"+
				"The file menu allows you to create new sheets, open saved sheets, and save the currently active\n "+
				"spreadsheet. Closing all windows will close the application.\n"
			);
		}
		
		
	}
}
