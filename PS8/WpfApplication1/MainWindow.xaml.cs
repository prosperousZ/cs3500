using System;
using System.Collections.Generic;
using System.Data;
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
using BB;
using CustomNetworking;
using System.Net.Sockets;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Tuple<string, StringSocket, string, string> player1; //name, SS, oppName, status
        //Tuple<string, StringSocket, string, string> player2; //name, SS, oppName, status
        StringSocket socketString1;
        StringSocket socketString2;
        String boardLetters;
        BoggleBoard theBoard;
        String name;
        String oppName;
        String status;

        // Register for this event to be motified when a line of text arrives.
        public event Action<String> IncomingEvent;


        public MainWindow()
        {
            Boggle.BoggleServer server = new Boggle.BoggleServer(2000);
            InitializeComponent();
            IncomingEvent += CommandReceived;

            // this needs to be added to a button after name has been input
            ConnectToGame1();


            // this isn't right yet, it will just follow the status update to playing after name has been sent with PLAY
            SetBoggleBoardGrid();



        }

        private void ConnectToGame1()
        {
            TcpClient tcpClient = new TcpClient("localhost", 2000);
            socketString1 = new StringSocket(tcpClient.Client, UTF8Encoding.Default);
            socketString1.BeginSend("PLAY " + name, (o, p) => { }, name);
            socketString1.BeginReceive(Received, socketString1);
        }
        private void ConnectToGame2()
        {
            TcpClient tcpClient = new TcpClient("localhost", 2000);
            socketString2 = new StringSocket(tcpClient.Client, UTF8Encoding.Default);
            socketString2.BeginSend("PLAY " + name, (o, p) => { }, name);
            //socketString2.BeginReceive(Received, socketString2);
        }

        private void Received(string s, Exception e, object payload)
        {
            if (IncomingEvent != null)
            {
                IncomingEvent(s);
            }
            socketString1.BeginReceive(Received, null);
        }

        private void CommandReceived(String command)
        {
            if (command.StartsWith("TIME "))
            {
            }
            else if (command.StartsWith("SCORE "))
            {
            }
            else if (command.StartsWith("START "))
            {
            }
            else if (command.StartsWith("STOP "))
            {
            }
            else
            {
            }

            //textBox2.Invoke(new Action(() => { textBox2.Text += line + "\r\n"; }));
        }



        private void SetBoggleBoardGrid()
        {
            DataTable boardTable = new DataTable();
            int numCols = 4;
            int numRows = 4;
            for (int i = 0; i < numCols; i++)
            {
                boardTable.Columns.Add(i.ToString(), typeof(double));
            }

            for (int row = 0; row < numRows; row++)
            {
                DataRow dataRow = boardTable.NewRow();
                for (int col = 0; col < numCols; col++)
                {
                    dataRow[col] = col;
                }
                boardTable.Rows.Add(dataRow);
            }

            BoardGrid.ItemsSource = boardTable.DefaultView;
        }

    }
}
