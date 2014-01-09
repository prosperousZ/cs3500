namespace SpreadsheetGUI
{
	partial class SpreadSheetForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.Panel = new SS.SpreadsheetPanel();
			this.file_menu = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveButton = new System.Windows.Forms.ToolStripMenuItem();
			this.openButton = new System.Windows.Forms.ToolStripMenuItem();
			this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.Halpbutton = new System.Windows.Forms.ToolStripMenuItem();
			this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.CellContents = new System.Windows.Forms.TextBox();
			this.EditContentsLabel = new System.Windows.Forms.Label();
			this.CellVal = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.CellNameBox = new System.Windows.Forms.TextBox();
			this.CellNameLabel = new System.Windows.Forms.Label();
			this.file_menu.SuspendLayout();
			this.SuspendLayout();
			// 
			// Panel
			// 
			this.Panel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Panel.Location = new System.Drawing.Point(0, 63);
			this.Panel.Name = "Panel";
			this.Panel.Size = new System.Drawing.Size(613, 344);
			this.Panel.TabIndex = 0;
			this.Panel.SelectionChanged += new SS.SelectionChangedHandler(this.Panel_SelectionChanged);
			this.Panel.Load += new System.EventHandler(this.Panel_Load);
			this.Panel.Click += new System.EventHandler(this.Panel_Click);
			// 
			// file_menu
			// 
			this.file_menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.Halpbutton});
			this.file_menu.Location = new System.Drawing.Point(0, 0);
			this.file_menu.Name = "file_menu";
			this.file_menu.Size = new System.Drawing.Size(625, 24);
			this.file_menu.TabIndex = 1;
			this.file_menu.Text = "File";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveButton,
            this.openButton,
            this.newToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.closeToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// saveButton
			// 
			this.saveButton.Name = "saveButton";
			this.saveButton.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.saveButton.Size = new System.Drawing.Size(144, 22);
			this.saveButton.Text = "save";
			this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
			// 
			// openButton
			// 
			this.openButton.Name = "openButton";
			this.openButton.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.openButton.Size = new System.Drawing.Size(144, 22);
			this.openButton.Text = "open";
			this.openButton.Click += new System.EventHandler(this.openButton_Click);
			// 
			// newToolStripMenuItem
			// 
			this.newToolStripMenuItem.Name = "newToolStripMenuItem";
			this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
			this.newToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
			this.newToolStripMenuItem.Text = "New";
			this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
			// 
			// closeToolStripMenuItem
			// 
			this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
			this.closeToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
			this.closeToolStripMenuItem.Text = "Close";
			this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
			// 
			// saveAsToolStripMenuItem
			// 
			this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
			this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
			this.saveAsToolStripMenuItem.Text = "save as";
			this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
			// 
			// Halpbutton
			// 
			this.Halpbutton.Name = "Halpbutton";
			this.Halpbutton.Size = new System.Drawing.Size(52, 20);
			this.Halpbutton.Text = "HALP!";
			this.Halpbutton.Click += new System.EventHandler(this.Halpbutton_Click);
			// 
			// backgroundWorker1
			// 
			this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.DefaultExt = "ss";
			this.openFileDialog1.FileName = "openFileDialog1";
			this.openFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog1_FileOk);
			// 
			// CellContents
			// 
			this.CellContents.Location = new System.Drawing.Point(390, 27);
			this.CellContents.Name = "CellContents";
			this.CellContents.Size = new System.Drawing.Size(223, 20);
			this.CellContents.TabIndex = 2;
			this.CellContents.MouseClick += new System.Windows.Forms.MouseEventHandler(this.CellContents_MouseClick);
			this.CellContents.TextChanged += new System.EventHandler(this.CellContents_TextChanged);
			this.CellContents.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CellContents_KeyPress);
			// 
			// EditContentsLabel
			// 
			this.EditContentsLabel.AutoSize = true;
			this.EditContentsLabel.Location = new System.Drawing.Point(314, 30);
			this.EditContentsLabel.Name = "EditContentsLabel";
			this.EditContentsLabel.Size = new System.Drawing.Size(70, 13);
			this.EditContentsLabel.TabIndex = 3;
			this.EditContentsLabel.Text = "Edit Contents";
			// 
			// CellVal
			// 
			this.CellVal.Location = new System.Drawing.Point(161, 26);
			this.CellVal.Name = "CellVal";
			this.CellVal.ReadOnly = true;
			this.CellVal.Size = new System.Drawing.Size(147, 20);
			this.CellVal.TabIndex = 4;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(119, 30);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(36, 13);
			this.label1.TabIndex = 5;
			this.label1.Text = "Cell = ";
			// 
			// saveFileDialog1
			// 
			this.saveFileDialog1.DefaultExt = "ss";
			this.saveFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.saveFileDialog1_FileOk);
			// 
			// CellNameBox
			// 
			this.CellNameBox.Location = new System.Drawing.Point(69, 26);
			this.CellNameBox.Name = "CellNameBox";
			this.CellNameBox.Size = new System.Drawing.Size(44, 20);
			this.CellNameBox.TabIndex = 6;
			// 
			// CellNameLabel
			// 
			this.CellNameLabel.AutoSize = true;
			this.CellNameLabel.Location = new System.Drawing.Point(11, 29);
			this.CellNameLabel.Name = "CellNameLabel";
			this.CellNameLabel.Size = new System.Drawing.Size(52, 13);
			this.CellNameLabel.TabIndex = 7;
			this.CellNameLabel.Text = "CellName";
			// 
			// SpreadSheetForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(625, 419);
			this.Controls.Add(this.CellNameLabel);
			this.Controls.Add(this.CellNameBox);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.CellVal);
			this.Controls.Add(this.EditContentsLabel);
			this.Controls.Add(this.CellContents);
			this.Controls.Add(this.Panel);
			this.Controls.Add(this.file_menu);
			this.Name = "SpreadSheetForm";
			this.Text = "SpreadsheetWindow";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SpreadSheetForm_FormClosing);
			this.file_menu.ResumeLayout(false);
			this.file_menu.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private SS.SpreadsheetPanel Panel;
		private System.Windows.Forms.MenuStrip file_menu;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveButton;
		private System.Windows.Forms.ToolStripMenuItem openButton;
		private System.ComponentModel.BackgroundWorker backgroundWorker1;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem Halpbutton;
		private System.Windows.Forms.TextBox CellContents;
		private System.Windows.Forms.Label EditContentsLabel;
		private System.Windows.Forms.TextBox CellVal;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.TextBox CellNameBox;
		private System.Windows.Forms.Label CellNameLabel;

	}
}

