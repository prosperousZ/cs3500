using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using CustomNetworking;

namespace TestStringSocket
{
	class Program
	{
		
		static void Main(string[] args)
		{
			//new server
			TcpListener server = new TcpListener(IPAddress.Any, 4010);
		
			server.Start();
			//blocks until user connects
			Socket serverSocket = server.AcceptSocket();
			Console.WriteLine("hey");

			// Wrap the two ends of the connection into StringSockets
			StringSocket sendSocket = new StringSocket(serverSocket, new UTF8Encoding());
			sendSocket.BeginSend("hi", (e, o) => { }, null);
			Console.ReadLine();
		}

		private static void connectionRequested(IAsyncResult ar)
		{
			
		}
	}
	//sorry
}
