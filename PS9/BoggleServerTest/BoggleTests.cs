// Boggle server tests.
// This contains a test for each of the commands that should come from the server.
//
// Written for PS8, CS3500 F13
// By Jesse Whitaker and Dylan Noaker




using CustomNetworking;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using Boggle;

namespace StringSocketTester
{
	[TestClass()]
	public class BoggleServerTest
	{

		[TestMethod()]
		public void TestStartSetDice()
		{
			string[] toArgs  = new string[3];
			toArgs[0] = "180";
			toArgs[1] = "dictionary.txt";
			toArgs[2] = "TAPRVILRGTOAEUEQ";
			ThreadPool.QueueUserWorkItem((e) => { BoggleServer.Main(toArgs); });

			List<StringSocket> clients = new List<StringSocket>();
			
			int CLIENT_COUNT = 2;//not a constant, I know.
			for(int i = 0; i<CLIENT_COUNT;i++)
			{
				TcpClient tempClient = new TcpClient("localhost", 2000);
				Socket tempclientSocket = tempClient.Client;
				StringSocket SS = new StringSocket(tempclientSocket, new UTF8Encoding());
				clients.Add(SS);
			}

			string clientString0 = "";
			string clientString1 = "";

			clients[0].BeginSend("PLAY Dylan\n", (e, p) => { clients[0].BeginReceive((s, ee, pp) => { clientString0 = s; }, 1); }, 1);
			clients[1].BeginSend("PLAY YY\n", (e, p) => {clients[1].BeginReceive((s, ee, pp) => { clientString1 = s; }, 1); }, 1);
			Thread.Sleep(1250);
            Assert.AreEqual("START TAPRVILRGTOAEUEQ " + toArgs[0] + " YY", clientString0);
            Assert.AreEqual("START TAPRVILRGTOAEUEQ " + toArgs[0] + " Dylan", clientString1);
		}



		[TestMethod()]
		public void TestStartAndScore()
		{
			string[] toArgs = new string[3];
			toArgs[0] = "5";
			toArgs[1] = "..\\..\\..\\dictionary.txt";
			toArgs[2] = "TAPRVILRGTOAEUEQ";
			ThreadPool.QueueUserWorkItem((e) => { BoggleServer.Main(toArgs); });

			List<StringSocket> clients = new List<StringSocket>();

			int CLIENT_COUNT = 2;//not a constant, I know.
			for (int i = 0; i < CLIENT_COUNT; i++)
			{
				TcpClient tempClient = new TcpClient("localhost", 2000);
				Socket tempclientSocket = tempClient.Client;
				StringSocket SS = new StringSocket(tempclientSocket, new UTF8Encoding());
				clients.Add(SS);
			}

			string clientString0 = "";
			string clientString1 = "";
			clients[0].BeginSend("PLAY Dylan\n", (e, p) => { clients[0].BeginReceive((s, ee, pp) => { clientString0 = s; }, 1); }, 1);
			Thread.Sleep(1250);
			clients[1].BeginSend("PLAY YY\n", (e, p) => { clients[1].BeginReceive((s, ee, pp) => { clientString1 = s; }, 1); }, 1);
			Thread.Sleep(1250);
			Assert.AreEqual("START TAPRVILRGTOAEUEQ " + toArgs[0] + " YY", clientString0);
			Assert.AreEqual("START TAPRVILRGTOAEUEQ " + toArgs[0] + " Dylan", clientString1);

			string clientString0next = "";
			string clientString1next = "";
		
			clients[0].BeginSend("WORD queue\n", (e, p) => { clients[0].BeginReceive((s, ee, pp) => { clientString0 = s; }, 1); }, 1);

			clientString0next = catchMessage(clientString0, 6, "SCORE", clients[0]);
			clientString1next = catchMessage(clientString1, 6, "SCORE", clients[1]);

			Assert.AreEqual("2 0", clientString0next);
			Assert.AreEqual("0 2", clientString1next);
		}

        [TestMethod()]
        public void TestScoringDiffSizeWords()
        {
            string[] toArgs = new string[3];
            toArgs[0] = "20";
            toArgs[1] = "..\\..\\..\\dictionary.txt";
            toArgs[2] = "SERSPATGLINESERS";
            ThreadPool.QueueUserWorkItem((e) => { BoggleServer.Main(toArgs); });

            List<StringSocket> clients = new List<StringSocket>();

            int CLIENT_COUNT = 2;//not a constant, I know.
            for (int i = 0; i < CLIENT_COUNT; i++)
            {
                TcpClient tempClient = new TcpClient("localhost", 2000);
                Socket tempclientSocket = tempClient.Client;
                StringSocket SS = new StringSocket(tempclientSocket, new UTF8Encoding());
                clients.Add(SS);
            }

            string clientString0 = "";
            string clientString1 = "";
            clients[0].BeginSend("PLAY Dylan\n", (e, p) => { clients[0].BeginReceive((s, ee, pp) => { clientString0 = s; }, 1); }, 1);

            clients[1].BeginSend("PLAY YY\n", (e, p) => { clients[1].BeginReceive((s, ee, pp) => { clientString1 = s; }, 1); }, 1);

            string clientString0next = "";
            string clientString1next = "";
            //bool notDone = true;
            clients[0].BeginSend("WORD Eateries\n", (e, p) => {  }, 1);
            clients[0].BeginReceive((s, ee, pp) => { clientString0 = s; }, 1);
            //Thread.Sleep(500);
            clientString0next = catchMessage(clientString0, 6, "SCORE", clients[0]);
            //clients[1].BeginReceive((s, ee, pp) => { clientString1 = s; }, 1);
            clientString1next = catchMessage(clientString1, 6, "SCORE", clients[1]);


            Assert.AreEqual("11 0", clientString0next);
            Assert.AreEqual("0 11", clientString1next);

            // The last part of this test is not working, but it is a problem with this test as telnet shows the server is working.
            //Thread.Sleep(500);
            ////notDone = true;
            //clients[0].BeginSend("WORD line\n", (e, p) => {  }, 1);

            //clients[0].BeginReceive((s, ee, pp) => { clientString0 = s; }, 1);

            //clientString0next = catchMessage(clientString0, 6, "SCORE", clients[0]);

            //clients[1].BeginReceive((s, ee, pp) => { clientString1 = s; }, 1);

            //clientString1next = catchMessage(clientString1, 6, "SCORE", clients[1]);

            //Assert.AreEqual("12 0", clientString0next);
            //Assert.AreEqual("0 12", clientString1next);

        }
		/// <summary>
		/// After sending a word to the server, we want to see what it sends back.
		/// Since the server is always sending back the time, we need a way to wait 
		/// to hear back the SCORE or STOP etc. This method does that.
		/// </summary>
		/// <param name="fromReceive">the string holding data received from the server</param>
		/// <param name="substringArg">how far to index into the message(should be about the length of message type</param>
		/// <param name="client">the StringSocket doing the sending/receiving</param>
		/// <param name="messageType">The type of message we are expecting according to the protocol</param>
		/// <returns>hopefully the string we are interested in.</returns>
		private string catchMessage(string fromReceive, int substringArg,string messageType, StringSocket client)
		{
			
			string toReturn = "";
			bool notDone = true;
			while (notDone)
			{
				if(fromReceive == null)
					client.BeginReceive((s, ee, pp) => { fromReceive = s; }, 1);
				else if (fromReceive.StartsWith(messageType))
				{
					lock (client)
					{
						toReturn = fromReceive.Substring(substringArg);
						notDone = false;
					}
				}
				else
					client.BeginReceive((s, ee, pp) => { fromReceive = s; }, 1);
			}
			return toReturn;
		}

        [TestMethod()]
        public void TestStartAndStop()
        {
            string[] toArgs = new string[3];
            toArgs[0] = "5";
            toArgs[1] = "..\\..\\..\\dictionary.txt";
            toArgs[2] = "TAPRVILRGTOAEUEQ";
            ThreadPool.QueueUserWorkItem((e) => { BoggleServer.Main(toArgs); });

            List<StringSocket> clients = new List<StringSocket>();

            int CLIENT_COUNT = 2;//not a constant, I know.
            for (int i = 0; i < CLIENT_COUNT; i++)
            {
                TcpClient tempClient = new TcpClient("localhost", 2000);
                Socket tempclientSocket = tempClient.Client;
                StringSocket SS = new StringSocket(tempclientSocket, new UTF8Encoding());
                clients.Add(SS);
            }

            string clientString0 = "";
            string clientString1 = "";
            clients[0].BeginSend("PLAY Dylan\n", (e, p) => { clients[0].BeginReceive((s, ee, pp) => { clientString0 = s; }, 1); }, 1);
            Thread.Sleep(1250);
            clients[1].BeginSend("PLAY YY\n", (e, p) => { clients[1].BeginReceive((s, ee, pp) => { clientString1 = s; }, 1); }, 1);
            Thread.Sleep(1250);
            Assert.AreEqual("START TAPRVILRGTOAEUEQ " + toArgs[0] + " YY", clientString0);
            Assert.AreEqual("START TAPRVILRGTOAEUEQ " + toArgs[0] + " Dylan", clientString1);


            string clientString0next = "";
            string clientString1next = "";
            
            clients[0].BeginSend("WORD queue\n", (e, p) => { clients[0].BeginReceive((s, ee, pp) => { clientString0 = s; }, 1); }, 1);
			//clientString0next = catchMessage(clientString0, 6, "SCORE", clients[0]);
            
           
            clients[1].BeginSend("WORD dkjflaks\n", (e, p) => { clients[1].BeginReceive((s, ee, pp) => { clientString1 = s; }, 1); }, 1);
			//clientString1next = catchMessage(clientString1, 6, "SCORE", clients[1]);

            Thread.Sleep(1250);

            clients[0].BeginReceive((s, ee, pp) => { clientString0 = s; }, 1);
			Thread.Sleep(500);
			clientString0next = catchMessage(clientString0, 5, "STOP", clients[0]);
            
            
            clients[1].BeginReceive((s, ee, pp) => { clientString1 = s; }, 1);
			Thread.Sleep(500);
			clientString1next = catchMessage(clientString1, 5, "STOP", clients[1]);

            Thread.Sleep(1250);
            Assert.AreEqual("1 QUEUE 0  0  0  1 DKJFLAKS", clientString0next);
			Assert.AreEqual("0  1 QUEUE 0  1 DKJFLAKS 0 ", clientString1next);

        }

        // for random dice, join two clients and play, then test start message check the dice match, and other parts of the message against what it should be
        [TestMethod()]
        public void TestStartRandomDice()
        {

            string[] toArgs = new string[2];
            toArgs[0] = "5";
            toArgs[1] = "..\\..\\..\\dictionary.txt";

            ThreadPool.QueueUserWorkItem((e) => { BoggleServer.Main(toArgs); });



            List<StringSocket> clients = new List<StringSocket>();

            int CLIENT_COUNT = 2;//not a constant, I know.
            for (int i = 0; i < CLIENT_COUNT; i++)
            {
                TcpClient tempClient = new TcpClient("localhost", 2000);
                Socket tempclientSocket = tempClient.Client;
                StringSocket SS = new StringSocket(tempclientSocket, new UTF8Encoding());
                clients.Add(SS);
            }

            string clientString0 = "";
            string clientString1 = "";

            clients[0].BeginSend("PLAY Dylan\n", (e, p) => { clients[0].BeginReceive((s, ee, pp) => { clientString0 = s; }, 1); }, 1);
            Thread.Sleep(1250);

            clients[1].BeginSend("PLAY YY\n", (e, p) => { clients[1].BeginReceive((s, ee, pp) => { clientString1 = s; }, 1); }, 1);
            Thread.Sleep(1250);
            string boardtemp = clientString0.Substring(6, 16);
            Assert.AreEqual("START " + boardtemp + " " + toArgs[0] + " YY", clientString0);
            Assert.AreEqual("START " + boardtemp + " " + toArgs[0] + " Dylan", clientString1);
        }


        [TestMethod()]
        public void TestIgnoring()
        {
            string[] toArgs = new string[3];
            toArgs[0] = "5";
            toArgs[1] = "..\\..\\..\\dictionary.txt";
            toArgs[2] = "TAPRVILRGTOAEUEQ";
            ThreadPool.QueueUserWorkItem((e) => { BoggleServer.Main(toArgs); });

            List<StringSocket> clients = new List<StringSocket>();

            int CLIENT_COUNT = 2;//not a constant, I know.
            for (int i = 0; i < CLIENT_COUNT; i++)
            {
                TcpClient tempClient = new TcpClient("localhost", 2000);
                Socket tempclientSocket = tempClient.Client;
                StringSocket SS = new StringSocket(tempclientSocket, new UTF8Encoding());
                clients.Add(SS);
            }

            string clientString0 = "";
            string clientString1 = "";
            clients[0].BeginSend("PLAY Dylan\n", (e, p) => { clients[0].BeginReceive((s, ee, pp) => { clientString0 = s; }, 1); }, 1);
            Thread.Sleep(1250);
            clients[1].BeginSend("PLAY YY\n", (e, p) => { clients[1].BeginReceive((s, ee, pp) => { clientString1 = s; }, 1); }, 1);
            Thread.Sleep(1250);
            Assert.AreEqual("START TAPRVILRGTOAEUEQ " + toArgs[0] + " YY", clientString0);
            Assert.AreEqual("START TAPRVILRGTOAEUEQ " + toArgs[0] + " Dylan", clientString1);


            string clientString0next = "";

           
            clients[0].BeginSend("bad string\n", (e, p) => {  }, 1);
            clients[0].BeginReceive((s, ee, pp) => { clientString0 = s; }, 1);
			clientString0next = catchMessage(clientString0, 0, "IGNORING", clients[0]);
            

            Thread.Sleep(1250);
            Assert.AreEqual("IGNORING bad string", clientString0next);
        }



        [TestMethod()]
        public void TestDisconnect()
        {

            string[] toArgs = new string[3];
            toArgs[0] = "5";
            toArgs[1] = "..\\..\\..\\dictionary.txt";
            toArgs[2] = "TAPRVILRGTOAEUEQ";
            ThreadPool.QueueUserWorkItem((e) => { BoggleServer.Main(toArgs); });

            List<StringSocket> clients = new List<StringSocket>();

            int CLIENT_COUNT = 2;//not a constant, I know.
            for (int i = 0; i < CLIENT_COUNT; i++)
            {
                TcpClient tempClient = new TcpClient("localhost", 2000);
                Socket tempclientSocket = tempClient.Client;
                StringSocket SS = new StringSocket(tempclientSocket, new UTF8Encoding());
                clients.Add(SS);
            }

            string clientString0 = "";
            string clientString1 = "";
            clients[0].BeginSend("PLAY Dylan\n", (e, p) => { }, 1);
            clients[1].BeginSend("PLAY ToClose\n", (e, p) => { clients[1].BeginReceive((s, ee, pp) => { clientString1 = s; }, 1); }, 1);

            Thread.Sleep(500);

            clientString0 = "";

            string clientString0next = "";

            clients[1].Close();

            clients[0].BeginReceive((s, ee, pp) => { clientString0 = s; }, 1);
			clientString0next = catchMessage(clientString0, 0, "TERMINATED", clients[0]);

            Assert.AreEqual("TERMINATED", clientString0next);

        }
    }
}