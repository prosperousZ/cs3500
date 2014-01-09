// BoggleServer class
// This handles client communication and connects clients in a game of Boggle.
//
// Written for PS8, CS3500 F13
// By Jesse Whitaker and Dylan Noaker

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using BB;
using System.Net;
using CustomNetworking;
using System.Text.RegularExpressions;
using System.IO;

namespace Boggle
{

	public class BoggleServer
	{
		
		// the server what listens for sockets.
		private TcpListener server;
		// list of clients and their names
		private Queue<Tuple<StringSocket, string>> clients;
		//list of all the words parsed from a dictionary file
		//--------------bad--------------------
		public  static HashSet<string> WordDictionary{get; private set;}
		private static int timePerRound;
        private HashSet<game> currentGames;
        private static string specificDice = "";
		
		
		/// <summary>
		/// start a new boggle server ready to accept clients
		/// </summary>
		/// <param name="args">0:timer time 1: dictionary file 3: specific dice</param>
		public static void Main(string[] args)
		{
            if (args.Length >= 2)
            {
				if (args.Length == 3 && Regex.IsMatch(args[2], @"[a-zA-Z]{16}"))
					specificDice = args[2];

                int tempTime;
                if (int.TryParse(args[0], out tempTime))
                    timePerRound = tempTime;

				WordDictionary = ParseDictionary(args[1]);

				BoggleServer myServer = new BoggleServer(2000);
                Console.ReadLine();
            }

			else
				throw new Exception("Error: Invalid args input");
		}
		/// <summary>
		/// #ctor
		/// makes server with new client lis
		/// </summary>
		/// <param name="port"></param>
		public BoggleServer(int port)
		{
			server = new TcpListener(IPAddress.Any,port);
            clients = new Queue<Tuple<StringSocket, string>>();
            currentGames = new HashSet<game>();
			server.Start();
			server.BeginAcceptSocket(ConnectionAccept, null);
           
		}

        /// <summary>
        /// Callback for accepting socket connections.
        /// </summary>
        /// <param name="ar"></param>
		private void ConnectionAccept(IAsyncResult ar)
		{
			Socket socket = server.EndAcceptSocket(ar);
			StringSocket StrungSocket = new StringSocket(socket, UTF8Encoding.Default);
			StrungSocket.BeginReceive(AddPlayer, StrungSocket);

            //Console.WriteLine("Connected");
			//if an even number of clients
			//send new pair a Start^&%&*^& message
			server.BeginAcceptSocket(ConnectionAccept, null);
		}

		/// <summary>
		/// adds a player with the name PLAY @ where @ is the intended name
		/// </summary>
		/// <remarks>
		/// 1.check validity of name (PLAY @)
		/// 2. enqueue player
        /// 3. if there are two players, stick them in a game
		/// </remarks>
		/// <param name="s">incoming string of format PLAY @ for name</param>
		/// <param name="e">some exception</param>
		/// <param name="payload">the socket should be used as the payload</param>
		/// 
		private void AddPlayer(string s, Exception e, object payload)
		{
            //Console.WriteLine("player is adding");
			if (object.ReferenceEquals(s, null))
			{
			
				(payload as StringSocket).Close();
				return;
			}
			else if (!object.ReferenceEquals(e, null))
				throw e;
			if (s.StartsWith("PLAY"))
			{
				Tuple<StringSocket, string> tempPlayer = new Tuple<StringSocket, string>((StringSocket)payload, s.Substring(4).Replace(" ", ""));
				lock (clients)
				{
					if (clients.Count == 1)
					{
                        Tuple<StringSocket, string> tempPlayer2 = clients.Dequeue();
						if (!tempPlayer2.Item1.Connected())      // This doesn't seem to be working as expected.
						{
							clients.Enqueue(tempPlayer);
							return;
						}
						if (specificDice.Equals(""))
							currentGames.Add(new game(this, tempPlayer, tempPlayer2, timePerRound));
						else
						{
							currentGames.Add(new game(this, tempPlayer, tempPlayer2, timePerRound, specificDice));
						}
					}
					else
					{
						clients.Enqueue(tempPlayer);
					}
				}
			}
			else
			{
				StringSocket temp = (StringSocket)payload;
				temp.BeginSend("IGNORING " + s+"\n", (ee, pp) => { }, null);
				temp.BeginReceive(AddPlayer, temp);
			}
			
		}

		/// <summary>
		/// parses the words out of the file into the dictionaryWords
		/// </summary>
		/// <remarks>makes a list out of the file, then assigns it to the READONLY word list</remarks>
		/// <param name="filePath">path to the file (absolute)</param>
		private static HashSet<string> ParseDictionary(string filePath)
		{
            HashSet<string> tempSet = new HashSet<string>();
            try
            {
                string line;
                using (StreamReader sr = new StreamReader(filePath))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        tempSet.Add(line);
                    }
                }
               return tempSet;
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
                return tempSet;
            }
		}

		/// <summary>
		/// checks a word against the dictionary of words
		/// </summary>
		/// <param name="testWord"></param>
		/// <returns>bool determining wheter its a valid word</returns>
		public static bool isword(string testWord)
		{
			if (WordDictionary == null)
				throw new Exception("dictionary is null");
			return WordDictionary.Contains(testWord);
		}
		/// <summary>
		/// remove terminated game
		/// </summary>
		/// <param name="gameToGo"></param>
		public void removegame(game gameToGo)
		{
			this.currentGames.Remove(gameToGo);
		}

		
	}
}
