using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClientModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Summary;

namespace BoggleClient
{
	
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// Dylan and Jesse
	/// </summary>
	public partial class MainWindow : Window
	{
		private BackgroundWorker hermes = new BackgroundWorker();
		private BoggleClientModel model;
        private HashSet<string> wordset;
        private string saveIP;
		public MainWindow()
		{
			InitializeComponent();
			model = new BoggleClientModel();
			model.LineComplete += MessageReceived;
            wordset = new HashSet<string>();
            Status.Text = "Welcome to Boggle. Please input the IP address for a Boggle server.";
			hermes.DoWork += sender_DoWork;
			hermes.RunWorkerCompleted += sender_RunWorkerCompleted;
            saveIP = "localhost";
            Input.Focus();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void sender_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			//Cursor = Cursors.Arrow;
		}
		/// <summary>
		/// sends stuff to the server through the model's socket
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void sender_DoWork(object sender, DoWorkEventArgs e)
		{
			
			//Cursor = Cursors.AppStarting;
			string toAppend;
			switch (model.state)
			{
				case State.name:
					toAppend = "PLAY ";
                    model.state = State.wait;
					break;
				case State.game:
					toAppend = "WORD ";
                    Status.Dispatcher.Invoke(() => { if (wordset.Add(Input.Text) && !(Input.Text.Length < 3)) { Wordlist.Text += Input.Text + "\r\n"; Input.Clear(); Input.Focus(); } });
					break;
				default:
					throw new Exception("WHAT ARE YOU DOING HUMAN???");

			}
			model.sendMessage(toAppend + e.Argument);
			
		}
		/// <summary>
		/// on enter press
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void HandleKeyUp(object sender, KeyEventArgs e)
		{
            if (e.Key != System.Windows.Input.Key.Enter) 
                return;
            SubmitClick(null, null);
		}
		/// <summary>
		/// on clicking the submit button
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SubmitClick(object sender, RoutedEventArgs e)
		{
			if (Input.Text == "" && model.state != State.wait)
				return;
            Input.Text = Input.Text.ToUpper();
            switch (model.state)
            {
                case State.iP:
                    try
                    {
                        model.Connect(2000, Input.Text);
                        Status.Dispatcher.Invoke(() => { Status.Text = "Connected. Please input your name."; saveIP = Input.Text;  Input.Text = ""; Input.Focus(); });
                    }
                    catch (Exception ee)
                    {
                        Status.Dispatcher.Invoke(() => { Status.Text = "Not able to connect. Please enter an IP address for an active Boggle server."; Input.Text = saveIP; Input.Focus(); });
                        model.state = State.iP;
                        //throw ee;
                    }
                    
                        break;
                case State.name:
					//sends whatever is in input
                        Input.Text = Input.Text.Replace(" ", "");
						hermes.RunWorkerAsync(Input.Text);
                        name1.Text = Input.Text;
                        Status.Dispatcher.Invoke(() => { Status.Text = "Please wait for another player."; SubmitButton.Content = "Cancel"; Input.Text = ""; Input.IsEnabled = false; });
                        break;
				case State.wait:
                        Status.Dispatcher.Invoke(() =>
                        {
                            TimeLeft.Text = "0";
                            name1.Text = "name1";
                            name2.Text = "name2";
                            Score1.Text = "score1";
                            Score2.Text = "score2";
                            Input.Text = saveIP;
                            wordset.Clear();
                            Status.Text = "Welcome to Boggle. Please input the IP address for a Boggle server.";
                            Wordlist.Text = "";
                            foreach (TextBlock tile in BoardView.Children)
                                tile.Text = "";
                            model.state = State.iP;
                            SubmitButton.Content = "Submit";
                            Input.IsEnabled = true;
                            model.closeSocket();
                        });
						break;
                case State.game:
                        // Use to add a word to the Wordlist displayed, it adds e and a new line
						hermes.RunWorkerAsync(Input.Text);
                        break;
                default:

                        break;
            }


		}
		/// <summary>
		/// every time a line is received from the server...
		/// </summary>
		private void MessageReceived(string command)
		{
            if (object.ReferenceEquals(command, null)) // This can happen after the game is over and it is okay to ignore.
				return;

			List<string> commandParts = new List<string>();
			
			//separates command into parts
			foreach (string s in Regex.Split(command, @" "))
			{
				if (!s.Equals(""))
					commandParts.Add(s);
			}
            switch (commandParts[0])
            {
                case "TIME":

                    Status.Dispatcher.Invoke(() => { TimeLeft.Text = commandParts[1]; }); // update time display
                    return;

                case "TERMINATED": //termination

                    Status.Dispatcher.Invoke(() =>
                    {
                        TimeLeft.Text = "0";
                        name1.Text = "name1";
                        name2.Text = "name2";
                        Score1.Text = "score1";
                        Score2.Text = "score2";
                        Input.Text = saveIP;
                        wordset.Clear();
                        Status.Text = "Welcome to Boggle. Please input the IP address for a Boggle server.";
                        Wordlist.Text = "";
                        foreach (TextBlock tile in BoardView.Children)
                            tile.Text = "";
                        model.state = State.iP;
                    });
                    return;

                case "IGONORING":
                    //should murder client, This should never be seen if server and client work correctly.
                    Status.Dispatcher.Invoke(() => { Wordlist.Text += command + "\r\n"; });
                    return;

                    // START the game.
                case "START":
                    Status.Dispatcher.Invoke(() =>
                    {
                        SubmitButton.Content = "Submit";
                        Status.Text = "Play game. See rules at \r\nhttp://en.wikipedia.org/wiki/Boggle"; // Some info for player
                        int i = 0;
                        foreach (TextBlock tile in BoardView.Children) // Populate board view
                        {
                            if (commandParts[1][i] + "" == "Q")
                                tile.Text = "QU";
                            else
                                tile.Text = "" + commandParts[1][i];
                            i++;
                        }
                        // Set the remaining display for the start of game
                        Input.IsEnabled = true; 
                        Input.Focus();
                        SubmitButton.IsEnabled = true;
                        TimeLeft.Text = commandParts[2];
                        name2.Text = commandParts[3];
                        Score1.Text = "0";
                        Score2.Text = "0";
                    });
                    model.state = State.game;
                    return;

                    // Update score display
                case "SCORE":
                    Status.Dispatcher.Invoke(() =>
                    {
                        Score1.Text = commandParts[1];
                        Score2.Text = commandParts[2];
                    });
                    return;

                    // Game is over. Send summary and reset client.
                case "STOP":
                    Status.Dispatcher.Invoke(() =>
                    {
                        new SummaryWindow(commandParts, name1.Text, Score1.Text, name2.Text, Score2.Text).Show();
          
                    });
                    Status.Dispatcher.Invoke(() =>
                    {
                        TimeLeft.Text = "0";
                        name1.Text = "name1";
                        name2.Text = "name2";
                        Score1.Text = "score1";
                        Score2.Text = "score2";
                        Input.Text = saveIP;
                        wordset.Clear();
                        Status.Text = "Welcome to Boggle. Please input the IP address for a Boggle server.";
                        Wordlist.Text = "";
                        foreach (TextBlock tile in BoardView.Children)
                            tile.Text = "";
                        model.state = State.iP;
                    });
                    return;

                    // Reset client in case of odd error.
                default:
                    // It should not hit this... Maybe add code to disconnect from server and restart client.
                    Status.Dispatcher.Invoke(() =>
                    {
                        TimeLeft.Text = "0";
                        name1.Text = "name1";
                        name2.Text = "name2";
                        Score1.Text = "score1";
                        Score2.Text = "score2";
                        Input.Text = saveIP;
                        wordset.Clear();
                        Status.Text = "Welcome to Boggle. Please input the IP address for a Boggle server.";
                        Wordlist.Text = "";
                        foreach (TextBlock tile in BoardView.Children)
                            tile.Text = "";
                        model.state = State.iP;
                    });
                    return;
            }
		}

        /// <summary>
        /// Closing client's last action.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void CloseAndDispose(object sender, EventArgs e)
		{
			model.closeSocket();
		}
	}
}
