using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomNetworking;
using Boggle;
using BoggleClient;
using System.Threading;

namespace BoggleClientUnitTests
{
	[TestClass]
	public class UnitTest1
	{
		private BoggleClient.MainWindow win1;
		private BoggleClient.MainWindow win2;
		
		private void init()
		{
			BoggleServer.Main(new string[] { "30", "..\\..\\..\\dictionary.txt", "TAPRVILRGTOAEUEQ" });
			  win1 = new BoggleClient.MainWindow();
			  win1.Show();
            win2 = new BoggleClient.MainWindow();
			win2.Show();

			//System.Windows.Threading.Dispatcher.Run();
			Console.WriteLine("hi");
		}
		[TestMethod()]
		public void TestMethod1()
		{
			init();
			Console.WriteLine("hi");
		}
	}
}
