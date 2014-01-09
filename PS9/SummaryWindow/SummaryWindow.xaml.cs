using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace Summary
{
    /// <summary>
    /// Interaction logic for summary window
	/// jesse
    /// </summary>
    public partial class SummaryWindow : Window
    {
        public SummaryWindow(List<string> commandParts, string name1, string score1, string name2, string score2)
        {
            InitializeComponent();

            // Show final score with names and a winner message.
            Summary.Text = "******Game Summary****** \r\n<<<Score>>>\r\n" + name1 + " > " + score1 + " <\r\n" + name2 + " > " + score2 + " <\r\n";
            int score1int;
            int score2int; 
            if (int.TryParse(score1, out score1int))
            {
                if (int.TryParse(score2, out score2int))
                {
                    if (score1int == score2int)
                        Summary.Text += "The game was a tie.\r\n"; // From here down it is concatenating the string on a new line of the string to display.
                    if (score1int > score2int)
                        Summary.Text += "You win!!!\r\n";
                    if (score1int < score2int)
                        Summary.Text += "You lose... :(\r\n";
                }
            }
            
			//Using the digit parts of the stop message, we can tell how many words to print for that list.
            int i = 1; // Start with word after STOP
            int lasti;
            if (int.TryParse(commandParts[i], out lasti))
                Summary.Text += "You played " + commandParts[i] + " legal words:\r\n";
            else return;
            int stopingi = i + lasti; i++;
            while (i <= stopingi)
            {
                Summary.Text += commandParts[i] + "\r\n";
                i++;
            }
            if (int.TryParse(commandParts[i], out lasti))
                Summary.Text += "Your opponent played " + commandParts[i] + " legal words:\r\n";
            else return;
            stopingi = i + lasti; i++;
            while (i <= stopingi)
            {
                Summary.Text += commandParts[i] + "\r\n";
                i++;
            }
            if (int.TryParse(commandParts[i], out lasti))
                Summary.Text += "You and your opponent played " + commandParts[i] + " legal words:\r\n";
            else return;
            stopingi = i + lasti; i++;
            while (i <= stopingi)
            {
                Summary.Text += commandParts[i] + "\r\n";
                i++;
            }
            if (int.TryParse(commandParts[i], out lasti))
                Summary.Text += "You mis-played " + commandParts[i] + " string of charaters:\r\n";
            else return;
            stopingi = i + lasti; i++;
            while (i <= stopingi)
            {
                Summary.Text += commandParts[i] + "\r\n";
                i++;
            }
            if (int.TryParse(commandParts[i], out lasti))
                Summary.Text += "Your opponent mis-played " + commandParts[i] + " string of charaters:\r\n";
            else return;
            stopingi = i + lasti; i++;
            while (i <= stopingi)
            {
                Summary.Text += commandParts[i] + "\r\n";
                i++;
            }
        }
    }
}
