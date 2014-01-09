using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using CustomNetworking;
//Dylan Noaker implemented this skeleton
namespace CustomNetworking
{
    /// <summary> 
    /// A StringSocket is a wrapper around a Socket.  It provides methods that
    /// asynchronously read lines of text (strings terminated by newlines) and 
    /// write strings. (As opposed to Sockets, which read and write raw bytes.)  
    ///
    /// StringSockets are thread safe.  This means that two or more threads may
    /// invoke methods on a shared StringSocket without restriction.  The
    /// StringSocket takes care of the synchonization.
    /// 
    /// Each StringSocket contains a Socket object that is provided by the client.  
    /// A StringSocket will work properly only if the client refrains from calling
    /// the contained Socket's read and write methods.(send and receive?)
    /// 
    /// If we have an open Socket s, we can create a StringSocket by doing
    /// 
    ///    StringSocket ss = new StringSocket(s, new UTF8Encoding());
    /// 
    /// We can write a string to the StringSocket by doing
    /// 
    ///    ss.BeginSend("Hello world", callback, payload);
    ///    
    /// where callback is a SendCallback (see below) and payload is an arbitrary object.
    /// This is a non-blocking, asynchronous operation.  When the StringSocket has 
    /// successfully written the string to the underlying Socket, or failed in the 
    /// attempt, it invokes the callback.  The parameters to the callback are a
    /// (possibly null) Exception and the payload.  If the Exception is non-null, it is
    /// the Exception that caused the send attempt to fail.
    /// 
    /// We can read a string from the StringSocket by doing
    /// 
    ///     ss.BeginReceive(callback, payload)
    ///     
    /// where callback is a ReceiveCallback (see below) and payload is an arbitrary object.
    /// This is non-blocking, asynchronous operation.  When the StringSocket has read a
    /// string of text terminated by a newline character from the underlying Socket, or
    /// failed in the attempt, it invokes the callback.  The parameters to the callback are
    /// a (possibly null) string, a (possibly null) Exception, and the payload.  Either the
    /// string or the Exception will be non-null, but not both.  If the string is non-null, 
    /// it is the requested string (with the newline removed).  If the Exception is non-null, 
    /// it is the Exception that caused the send attempt to fail.
    /// </summary>

    public class StringSocket
    {
        // These delegates describe the callbacks that are used for sending and receiving strings.
        public delegate void SendCallback(Exception e, object payload);
        public delegate void ReceiveCallback(String s, Exception e, object payload);
		/// <summary>
		/// a que of outgoing messages
		/// </summary>
		private Queue<SendInteraction> sends;
		private Queue<RecInteraction> receives;
		private Queue<string> messages;

		private Socket ByteSocket;
		private Encoding MyCoded;
		/// <summary>
		/// How many bytes have been sent so far
		/// </summary>
		private int SendByteCount;
		
		private string incoming;
        /// <summary>
        /// Creates a StringSocket from a regular Socket, which should already be connected.  
        /// The read and write methods of the regular Socket must not be called after the
        /// LineSocket is created.  Otherwise, the StringSocket will not behave properly.  
        /// The encoding to use to convert between raw bytes and strings is also provided.
		/// When does this happen? Where is client/server? Where is application? 
        /// </summary>
        public StringSocket(Socket s, Encoding e)
        {
			sends = new Queue<SendInteraction>();
			receives = new Queue<RecInteraction>();
			messages = new Queue<string>();
			incoming = "";
			ByteSocket = s;
			MyCoded = e;
			SendByteCount = 0;
        }

        /// <summary>
        /// We can write a string to a StringSocket ss by doing
        /// 
        ///    ss.BeginSend("Hello world", callback, payload);
        ///    
        /// where callback is a SendCallback (see below) and payload is an arbitrary object.
        /// This is a non-blocking, asynchronous operation.  When the StringSocket has 
        /// successfully written the string to the underlying Socket, or failed in the 
        /// attempt, it invokes the callback.  The parameters to the callback are a
        /// (possibly null) Exception and the payload.  If the Exception is non-null, it is
        /// the Exception that caused the send attempt to fail. 
        /// 
        /// This method is non-blocking.  This means that it does not wait until the string
        /// has been sent before returning.  Instead, it arranges for the string to be sent
        /// and then returns.  When the send is completed (at some time in the future), the
        /// callback is called on another thread.
        /// 
        /// This method is thread safe.  This means that multiple threads can call BeginSend
        /// on a shared socket without worrying about synchronization.  The implementation of
        /// BeginSend must take care of synchronization instead.  On a given StringSocket, each
        /// string arriving via a BeginSend method call must be sent (in its entirety) before
        /// a later arriving string can be sent.
        /// </summary>
        public void BeginSend(String s, SendCallback callback, object payload)
        {
			lock (sendKey)
			{
				sends.Enqueue(new SendInteraction(s, callback, payload));
				if(sends.Count==1)
					loopSendsQueue();
			}
        }
		/// <summary>
		/// lock for send operations
		/// </summary>
		private readonly object sendKey = new object();
		/// <summary>
		/// sends pending messages stored in the sends queue
		/// </summary>
		private void loopSendsQueue()
		{
				byte[] outgoingBytes = MyCoded.GetBytes(sends.Peek().Message);
				this.ByteSocket.BeginSend(outgoingBytes, 0, outgoingBytes.Length, SocketFlags.None, byte_socket_send_callback,outgoingBytes);
		}
		/// <summary>
		/// check if all bytes were sent
		/// if not, send the rest.
		/// </summary>
		/// <param name="result">result is the payload</param>
		private void byte_socket_send_callback(IAsyncResult result)
		{
			//grab the byte buffer out of result
			byte[] outgoingBytes = (byte[])result.AsyncState;
			int bytes = ByteSocket.EndSend(result);
			if (bytes == 0)
				ByteSocket.Close();
			else
				SendByteCount += bytes;
			if (SendByteCount == outgoingBytes.Length)
			{
				SendInteraction goodsend = sends.Dequeue();
				SendByteCount = 0;
				ThreadPool.QueueUserWorkItem(x => goodsend.sendCB(null, goodsend.payload));
				lock (sendKey)
				{
					if (sends.Count > 0)
						loopSendsQueue();
				}
			}
			else
			{
				ByteSocket.BeginSend(outgoingBytes, SendByteCount, outgoingBytes.Length - SendByteCount,SocketFlags.None,byte_socket_send_callback, outgoingBytes);
			}
		}
        /// <summary>
        /// 
        /// <para>
        /// We can read a string from the StringSocket by doing
        /// </para>
        /// 
        /// <para>
        ///     ss.BeginReceive(callback, payload)
        /// </para>
        /// 
        /// <para>
        /// where callback is a ReceiveCallback (see below) and payload is an arbitrary object.
        /// This is non-blocking, asynchronous operation.  When the StringSocket has read a
        /// string of text terminated by a newline character from the underlying Socket, or
        /// failed in the attempt, it invokes the callback.  The parameters to the callback are
        /// a (possibly null) string, a (possibly null) Exception, and the payload.  Either the
        /// string or the Exception will be non-null, but not both.  If the string is non-null, 
        /// it is the requested string (with the newline removed).  If the Exception is non-null, 
        /// it is the Exception that caused the send attempt to fail.
        /// </para>
        /// 
        /// <para>
        /// This method is non-blocking.  This means that it does not wait until a line of text
        /// has been received before returning.  Instead, it arranges for a line to be received
        /// and then returns.  When the line is actually received (at some time in the future), the
        /// callback is called on another thread.
        /// </para>
        /// 
        /// <para>
        /// This method is thread safe.  This means that multiple threads can call BeginReceive
        /// on a shared socket without worrying around synchronization.  The implementation of
        /// BeginReceive must taken care of synchronization instead.  On a given StringSocket, each
        /// arriving line of text must be passed to callbacks in the order in which the corresponding
        /// BeginReceive call arrived.
        /// </para>
        /// 
        /// <para>
        /// Note that it is possible for there to be incoming bytes arriving at the underlying Socket
        /// even when there are no pending callbacks.  StringSocket implementations should refrain
        /// from buffering an unbounded number of incoming bytes beyond what is required to service
        /// the pending callbacks.        
        /// </para>
        /// 
        /// <param name="callback"> The function to call upon receiving the data</param>
        /// <param name="payload"> 
        /// The payload is "remembered" so that when the callback is invoked, it can be associated
        /// with a specific Begin Receiver....
        /// </param>  
        /// 
        /// <example>
        ///   Here is how you might use this code:
        ///   <code>
        ///                    client = new TcpClient("localhost", port);
        ///                    Socket       clientSocket = client.Client;
        ///                    StringSocket receiveSocket = new StringSocket(clientSocket, new UTF8Encoding());
        ///                    receiveSocket.BeginReceive(CompletedReceive1, 1);
        /// 
        ///   </code>
        /// </example>
        /// </summary>
        /// 
        /// 
        public void BeginReceive(ReceiveCallback callback, object payload)
        {
			lock (recKey)
			{
				//enque a new receive interaction
				receives.Enqueue(new RecInteraction(callback, payload));
				//if the que of receives has stuff, we need to check the incoming string
				if (receives.Count == 1)
					CheckIncoming();
			}
			

        }
		/// <summary>
		/// check what got added after previous callback
		/// </summary>
		private void CheckIncoming()
		{
			//need to look for newlines
			int index;
			while ((index = incoming.IndexOf('\n')) >= 0)
			{
				//one message segment
				String Message = incoming.Substring(0, index);
				if (Message.EndsWith("\r"))
				{
					Message = Message.Substring(0, index - 1);
				}
				messages.Enqueue(Message);
				incoming = incoming.Substring(index + 1);
			}
			while (receives.Count > 0 && messages.Count > 0) 
			{
				RecInteraction goodreceive = receives.Dequeue();
				string goodmessage = messages.Dequeue();
				ThreadPool.QueueUserWorkItem(x=>goodreceive.recvCB(goodmessage,null,goodreceive.payload));
			}
			if(receives.Count>0)
			{
				//bounded buffer. no DOSing
				byte[] buffer = new byte[1024];
				ByteSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceivedBytes, buffer);
			}
		}
		/// <summary>
		/// receive callback for the byte socket
		/// </summary>
		/// <param name="ar">the payload is in here somewhere</param>
		private void ReceivedBytes(IAsyncResult ar)
		{
			//the actual bytes received
			byte[] buffer = (byte[])(ar.AsyncState);
			//get number of bytes received so far
			int bytesIn = ByteSocket.EndReceive(ar);
			if (bytesIn == 0)
			{
				Console.WriteLine("goodbye");
				ByteSocket.Close();
			}
			lock (recKey)
			{
				incoming += MyCoded.GetString(buffer, 0, bytesIn);
				CheckIncoming();
			}
		}
		private readonly object recKey = new object();
		/// <summary>
		/// Send interactions hold all of the information for a send. Allows us to enqueue
		/// the sends in our string socket.
		/// (could use a tuple but this helps organization)
		/// </summary>
		private class SendInteraction
		{
			/// <summary>
			/// the outgoing message
			/// </summary>
			public string Message { get; private set; }
			/// <summary>
			/// the callback defined by the client
			/// </summary>
			public SendCallback sendCB { get; private set; }
			/// <summary>
			/// arbitrary object used to identify this interaction
			/// </summary>
			public object payload { get; private set; }
			public SendInteraction(string _message, SendCallback _sendCB, object _Payload)
			{
				Message = _message;
				sendCB = _sendCB;
				payload = _Payload;
			}
		}
		/// <summary>
		/// Holds data for a receive interaction
		/// (could use a tuple but this helps organization)
		/// </summary>
		private class RecInteraction
		{
			/// <summary>
			/// arbitrary object used to identify this receive interaction
			/// </summary>
			public object payload;
			/// <summary>
			/// the callback as defined by the client for this receive interaction
			/// </summary>
			public ReceiveCallback recvCB;
			public RecInteraction( ReceiveCallback _recvCb,object _payload) 
			{
				payload = _payload;
				recvCB = _recvCb;
			}

		}
    }
	
	//sorry
 }