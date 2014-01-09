using CustomNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientModel
{
	public enum State {iP, name, wait, game };


    /// <summary>
    /// The underlying model for the client view for the boggle game
    /// </summary>
	/// <author>Jesse and Dylan</author>

	public class BoggleClientModel
    {
		public event Action<string> LineComplete;
		public State state;
		private StringSocket socket;
		/// <summary>
		/// #ctor
		/// starts the socket as null
		/// </summary>
		public BoggleClientModel()
		{
			state = State.iP;
			socket = null;
		}
		/// <summary>
		/// connects on the given port and ip
		/// </summary>
		/// <param name="port"></param>
		/// <param name="ip"></param>
		public void Connect(int port, string ip)
		{
            if (socket == null)
            {
                TcpClient client = new TcpClient(ip, port);
                socket = new StringSocket(client.Client, UTF8Encoding.Default);
                state = State.name;
				socket.BeginReceive(LineReceived, null);
            }
		}
		/// <summary>
		/// sends any message to the server. be sure to append protocol prefixes based on state
		/// </summary>
		/// <param name="command">the message to be sent. be sure to append protocol message format</param>
		public void sendMessage(string command)
		{
            socket.BeginSend(command + "\n", (ee, pp) => { }, null);
		}

		/// <summary>
		/// called when we receive stuff back from the server
		/// uses the lineComplete event to update the view
		/// also used to begin receive on the socket
		/// </summary>
		/// <param name="s"></param>
		/// <param name="e"></param>
		/// <param name="p"></param>
		public void LineReceived(string s, Exception e, object p)
		{
            if (s == null) 
            {
                ServerDisconnected();
				return;
            }

            if (LineComplete != null)
            {
                LineComplete(s);
            }
            socket.BeginReceive(LineReceived, null);
		}
		/// <summary>
		/// when the server goes down
		/// </summary>
        public void ServerDisconnected()
        {
           // Not used at the moment.
			state = State.iP;
			socket = null;
        }
		/// <summary>
		/// close the underlying socket
		/// </summary>
		public void closeSocket()
		{
			socket.Close();
		}
	}
}
