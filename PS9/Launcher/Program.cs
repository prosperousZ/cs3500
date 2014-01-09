using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomNetworking;
using Boggle;
using BoggleClient;
using System.Threading;

namespace Launcher
{
	/// Dylan & Jesse
	/// the launcher for multiple clients and a server
	class Launcher
	{
		/// <summary>
		/// launches a few clients and a server with custom board etc.
		/// </summary>
		/// <param name="args"></param>
		[STAThread]
		static void Main(string[] args)
		{
			new Thread(() =>
			//BoggleServer.Main(new string[] { "30", "..\\..\\..\\dictionary.txt", "TAPRVILRGTOAEUEQ" })).Start();
			BoggleServer.Main(new string[] { "30", "..\\..\\..\\dictionary.txt" })).Start();
			new BoggleClient.MainWindow().Show();
			new BoggleClient.MainWindow().Show();
            new BoggleClient.MainWindow().Show();
            new BoggleClient.MainWindow().Show();
            new BoggleClient.MainWindow().Show();
            new BoggleClient.MainWindow().Show(); 

			System.Windows.Threading.Dispatcher.Run();
		}
	}
}
