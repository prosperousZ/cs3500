// Game class written for the BoggleServer.
// This handles a game after two clients have joined.
//
// Written for PS8, CS3500 F13
// By Jesse Whitaker and Dylan Noaker


using BB;
using CustomNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using HashSetExtensions;

namespace Boggle
{
	public class game
	{

		private player player1;
		private player player2;
		public BoggleBoard board;
		public BoggleServer parent;
		public int time;
		public Timer secondsTimer;

		private readonly object processingKey;
		private HashSet<string> common;

		/// <summary>
		/// normal constructor
		/// </summary>
		/// <param name="_p1Sock"> player one's socket (ss)</param>
		/// <param name="_p2Sock">player two's socket (ss)</param>
		/// <param name="_time">the game time</param>
		public game(BoggleServer _parent,Tuple<StringSocket, string> _p1Sock, Tuple<StringSocket, string> _p2Sock, int _time)
		{
		parent = _parent;
			player1 = new player(_p1Sock.Item1, _p1Sock.Item2);
			player2 = new player(_p2Sock.Item1, _p2Sock.Item2);
			
			time = _time;
		
			board = new BoggleBoard();
			secondsTimer = new Timer(1000);
			common = new HashSet<string>();
			processingKey = new object();
			StartGame();
		}

		/// <summary>
		/// overload for user specified board with preset dice results.
		/// </summary>
		/// <param name="_p1Sock"> player one's socket (ss)</param>
		/// <param name="_p2Sock">player two's socket (ss)</param>
		/// <param name="_time">the game time</param>
		/// <param name="boardLetters">the optional string params for the console running emacs stuff</param>
		public game(BoggleServer _parent,Tuple<StringSocket, string> _p1Sock, Tuple<StringSocket, string> _p2Sock, int _time, string boardLetters)	
		{
			parent = _parent;
			player1 = new player(_p1Sock.Item1, _p1Sock.Item2);
			player2 = new player(_p2Sock.Item1, _p2Sock.Item2);

			time = _time;

			board = new BoggleBoard(boardLetters);
			secondsTimer = new Timer(1000);
			common = new HashSet<string>();
			processingKey = new object();
			StartGame();
		}

		/// <summary>
		/// sends start message to a pair. 
		/// </summary>
		public void StartGame()
		{
			player1.PlayerSocket.BeginSend("START " + board.ToString() + " " + (time) + " " + player2.PlayerName+"\n", (ee, pp) => { }, null);
			player2.PlayerSocket.BeginSend("START " + board.ToString() + " " + (time) + " " + player1.PlayerName + "\n", (ee, pp) => { }, null);
			secondsTimer.Elapsed += new ElapsedEventHandler(Tick);
			secondsTimer.Start();
			player1.PlayerSocket.BeginReceive(ProcessReceive, player1);
			player2.PlayerSocket.BeginReceive(ProcessReceive, player2);
		}

		/// <summary>
		/// call back for receiving a word
		/// </summary>
		/// <remarks>
		/// 1. gets actual word off of received stuff
		/// 2. checks against dictionary and board
		/// 3. checks against other player (removes and adds to common if matched)
		/// 4.in case of failure (npot in dictionary or board) adds to player's penalty list
		/// </remarks>
		/// <param name="s">string received</param>
		/// <param name="e"></param>
		/// <param name="payload"></param>
		private void ProcessReceive(string s, Exception e, object payload)
		{
			//if someone disconnects, tell their opponent
			if (object.ReferenceEquals(s, null))// && object.ReferenceEquals(e, null))
			{
				player offender = (payload as player);
				offender.PlayerSocket.Close();
				if (ReferenceEquals(offender, player1))
					player2.PlayerSocket.BeginSend("TERMINATED\n", (ee, pp) => { player2.PlayerSocket.Close(); }, null);
				else
					player1.PlayerSocket.BeginSend("TERMINATED\n", (ee, pp) => { player1.PlayerSocket.Close(); }, null);
				return;
			}
			else if (!object.ReferenceEquals(e, null))
                throw e; // Maybe we need to do something different here that doens't stop the server. Like just remove this game from games.
                           // Or maybe it is just showing another issue.

			if (s.StartsWith("WORD"))
			{
				lock (processingKey)
				{
					string tempWord = s.Substring(5).TrimEnd('\r').ToUpper();
					player tempPlayer = (payload as player);

					if (BoggleServer.WordDictionary.Contains(tempWord) && board.CanBeFormed(tempWord))
					{
						tempPlayer.Valids.Add(tempWord);
						if(player1.Valids.Contains(tempWord) && player2.Valids.Contains(tempWord))
						{
							player1.Valids.Remove(tempWord);
							player2.Valids.Remove(tempWord);
							common.Add(tempWord);
						}
						updateScore();
                        //(payload as player).PlayerSocket.BeginReceive(ProcessReceive, (payload as player));
					}
					else
					{
						tempPlayer.Penalties.Add(tempWord);
						updateScore();
                        //(payload as player).PlayerSocket.BeginReceive(ProcessReceive, (payload as player));
					}
				}
			}
			else
			{
				StringSocket temp = (payload as player).PlayerSocket;
				temp.BeginSend("IGNORING " + s+"\n", (ee, pp) => { }, null);
                
			}
            (payload as player).PlayerSocket.BeginReceive(ProcessReceive, (payload as player));
		}
		/// <summary>
		/// updates the score based on the player's lists.
		/// THIS IS BLOCKING
		/// </summary>
		private void updateScore()
		{
			int player1Score = 0;
			int player2Score = 0;
			foreach (string s in player1.Valids)
			{
				switch (s.Length)
				{
					case 3:
						player1Score++;
						break;
					case 4:
						player1Score++;
						break;
					case 5:
						player1Score += 2;
						break;
					case 6:
						player1Score += 3;
						break;
					case 7:
						player1Score += 5;
						break;
					default:
						player1Score += 11;
						break;
				}
			}
			foreach (string s in player2.Valids)
			{
				switch (s.Length)
				{
					case 3:
						player2Score++;
						break;
					case 4:
						player2Score++;
						break;
					case 5:
						player2Score += 2;
						break;
					case 6:
						player2Score += 3;
						break;
					case 7:
						player2Score += 5;
						break;
					default:
						player2Score += 11;
						break;
				}
			}
			player1Score -= player1.Penalties.Count;
			player2Score -= player2.Penalties.Count;

			player1.PlayerSocket.BeginSend("SCORE " + player1Score + " " + player2Score + "\n", (e, p) => { }, null);
			player2.PlayerSocket.BeginSend("SCORE " + player2Score + " " + player1Score + "\n", (e, p) => { }, null);

		}
		/// <summary>
		/// every second, sends remaining seconds to clients and ends game when timer gets to zero
		/// </summary>
		public void Tick(object source, ElapsedEventArgs e)
		{
			time--;

			if (time == 0)
			{
				secondsTimer.Stop();
				EndGame();
			}
			else
			{
				player1.PlayerSocket.BeginSend("TIME "+time.ToString()+"\n", (ee, pp) => { }, null);
				player2.PlayerSocket.BeginSend("TIME " + time.ToString() + "\n", (ee, pp) => { }, null);
			}
		}

		/// <summary>
		/// Finishes the game by sending the results to the players.
		/// First, it transmits the final score to both clients
		/// </summary>
		private void EndGame()
		{

			player1.PlayerSocket.BeginSend("STOP " + player1.Valids.Count + " " + player1.Valids.ToReadable() + " " +
				player2.Valids.Count + " " + player2.Valids.ToReadable() + " " +
				common.Count + " " + common.ToReadable() + " " +
				player1.Penalties.Count + " " + player1.Penalties.ToReadable() + " " +
				player2.Penalties.Count + " " + player2.Penalties.ToReadable() + "\n", (ee, pp) => { }, null);

			player2.PlayerSocket.BeginSend("STOP " + player2.Valids.Count + " " + player2.Valids.ToReadable() + " " +
				player1.Valids.Count + " " + player1.Valids.ToReadable() + " " +
				common.Count + " " + common.ToReadable() + " " +
				player2.Penalties.Count + " " + player2.Penalties.ToReadable() + " " +
				player1.Penalties.Count + " " + player1.Penalties.ToReadable() + "\n", (ee, pp) => { }, null);

			player1.PlayerSocket.Close();
			player2.PlayerSocket.Close();
			parent.removegame(this);
		}

        

		/// <summary>
		/// construct the five part summary
		/// </summary>
		/// <returns>
		/// count+client scoring words,count +opponents scoring words, count + common words,count + client illegal,count + opponent illegal 
		/// </returns>
		/// <param name="whichPlayer">the player receiving a summary</param>
		//private string GameSummary(player whichPlayer)
		//{
		//	return
		//		whichPlayer.
		//}

		//------------------------------------------------------
		//------------------------player------------------------
		//------------------------------------------------------
		private class player
		{
			public StringSocket PlayerSocket { get; private set; }
			public string PlayerName { get; private set; }
			public HashSet<string> Valids { get; private set; }
			public HashSet<string> Penalties { get; private set; }
			
			public player(StringSocket _playerSocket, string _playerName)
			{
				PlayerSocket = _playerSocket;
				PlayerName = _playerName;
				Valids = new HashSet<string>();
				Penalties = new HashSet<string>();
			}
			
		}
	}
}
namespace HashSetExtensions
{
	public static class MyExtensions
	{
		/// <summary>
		/// returns a space separated string of words from the instance of hashset
		/// </summary>
		/// <param name="mySet">instance of hashset.</param>
		/// <returns>a space separated string of words from the instance of hashset</returns>
		public static string ToReadable(this HashSet<string> mySet)
		{
			string toReturn ="";
			foreach(string item in mySet)
			{
				toReturn += item + " " ;
			}
			
			return toReturn.TrimEnd(' ');
		}
	}   
}
