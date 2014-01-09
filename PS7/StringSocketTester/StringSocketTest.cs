using CustomNetworking;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

namespace StringSocketTester
{


    /// <summary>
    ///This is a test class for StringSocketTest and is intended
    ///to contain all StringSocketTest Unit Tests
    ///</summary>
    [TestClass()]
    public class StringSocketTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A simple test for BeginSend and BeginReceive
        ///</summary>
        [TestMethod()]
        public void Test1()
        {
            new Test1Class().run(4001);
        }
		[TestMethod()]
		public void TW_AH_Test()
		{
			new TW_AH_TestClass().run(4001);
		}
		[TestMethod]
		public void SendingAfterReceivingTest()
		{
			new Test2Class().run(4001);
		}

        public class Test1Class
        {
            // Data that is shared across threads
            private ManualResetEvent mre1;
            private ManualResetEvent mre2;
            private String s1;
            private object p1;
            private String s2;
            private object p2;

            // Timeout used in test case
            private static int timeout = 200000;

            public void run(int port)
            {
                // Create and start a server and client.
                TcpListener server = null;
                TcpClient client = null;

                try
                {
                    server = new TcpListener(IPAddress.Any, port);
                    server.Start();
                    client = new TcpClient("localhost", port);

                    // Obtain the sockets from the two ends of the connection.  We are using the blocking AcceptSocket()
                    // method here, which is OK for a test case.
                    Socket serverSocket = server.AcceptSocket();
                    Socket clientSocket = client.Client;

                    // Wrap the two ends of the connection into StringSockets
                    StringSocket sendSocket = new StringSocket(serverSocket, new UTF8Encoding());
                    StringSocket receiveSocket = new StringSocket(clientSocket, new UTF8Encoding());

                    // This will coordinate communication between the threads of the test cases
                    mre1 = new ManualResetEvent(false);
                    mre2 = new ManualResetEvent(false);

                    // Make two receive requests
                    receiveSocket.BeginReceive(CompletedReceive1, 1);
                    receiveSocket.BeginReceive(CompletedReceive2, 2);

                    // Now send the data.  Hope those receive requests didn't block!
                    String msg = "Hello world\nThis is a test\n";
                    foreach (char c in msg)
                    {
                        sendSocket.BeginSend(c.ToString(), (e, o) => { }, null);
                    }

                    // Make sure the lines were received properly.
                    Assert.AreEqual(true, mre1.WaitOne(timeout), "Timed out waiting 1");
                    Assert.AreEqual("Hello world", s1);
                    Assert.AreEqual(1, p1);

                    Assert.AreEqual(true, mre2.WaitOne(timeout), "Timed out waiting 2");
                    Assert.AreEqual("This is a test", s2);
                    Assert.AreEqual(2, p2);
                }
                finally
                {
                    server.Stop();
                    client.Close();
                }
            }

            // This is the callback for the first receive request.  We can't make assertions anywhere
            // but the main thread, so we write the values to member variables so they can be tested
            // on the main thread.
            private void CompletedReceive1(String s, Exception o, object payload)
            {
                s1 = s;
                p1 = payload;
                mre1.Set();
            }

            // This is the callback for the second receive request.
            private void CompletedReceive2(String s, Exception o, object payload)
            {
                s2 = s;
                p2 = payload;
                mre2.Set();
            }
			/// <summary>
			/// Tim Winchester and Aundrea Hargroder
			///A test sending 100 strings "0\n" through "100\n" and asserting that the strings are received in the same order.
			///</summary>
		

			
        }
		public class TW_AH_TestClass
		{
			// Data that is shared across threads
			List<String> ReceiveList;
			List<String> CompareList;
			List<String> SendList;

			public void run(int port)
			{
				// Create and start a server and client.
				TcpListener server = null;
				TcpClient client = null;

				ReceiveList = new List<string>();
				CompareList = new List<string>();
				SendList = new List<string>();

				try
				{
					server = new TcpListener(IPAddress.Any, port);
					server.Start();
					client = new TcpClient("localhost", port);

					// Obtain the sockets from the two ends of the connection.  We are using the blocking AcceptSocket()
					// method here, which is OK for a test case.
					Socket serverSocket = server.AcceptSocket();
					Socket clientSocket = client.Client;

					// Wrap the two ends of the connection into StringSockets
					StringSocket sendSocket = new StringSocket(serverSocket, new UTF8Encoding());
					StringSocket receiveSocket = new StringSocket(clientSocket, new UTF8Encoding());

					// Make receive requests
					for (int n = 0; n <= 99; n++)
					{
						BeginReceieve(receiveSocket, n);
					}

					// Now send the data.  Hope those receive requests didn't block!
					for (int n = 0; n <= 99; n++)
					{
						BeginSend(sendSocket, n);
					}

					// Give the messages 3 seconds to arrive. If you have long sleeps in your code, you should increase this
					// increased to four seconds.
					Thread.Sleep(4000);

					// Make sure the lines were received properly.
					for (int n = 0; n <= 99; n++)
					{
						Assert.AreEqual(CompareList[n], ReceiveList[n]);
					}
				}
				finally
				{
					server.Stop();
					client.Close();
				}

			}

			/// <summary>
			/// Calls BeginSend.  Adds strings to list in order that they are sent.
			/// </summary>
			/// <param name="sendSocket"></param>
			/// <param name="n"></param>
			private void BeginSend(StringSocket sendSocket, int n)
			{
				sendSocket.BeginSend(n.ToString() + "\n", (e, o) => { }, null);
				SendList.Add(n.ToString());
			}

			/// <summary>
			/// Calls BeginReceive and adds strings in order they were received.
			/// </summary>
			/// <param name="receiveSocket"></param>
			/// <param name="n"></param>
			private void BeginReceieve(StringSocket receiveSocket, int n)
			{
				receiveSocket.BeginReceive(ReceiveCallback, null);
				CompareList.Add(n.ToString());
			}

			/// <summary>
			/// Callback created for test. Adds strings to list in order received.
			/// </summary>
			/// <param name="s"></param>
			/// <param name="e"></param>
			/// <param name="payload"></param>
			private void ReceiveCallback(String s, Exception e, Object payload)
			{
				ReceiveList.Add(s);
			}
		}

		/// <summary>
		/// BeginSend and BeginReceive in various ordering
		/// </summary>
		public class Test2Class
		{
			// Data that is shared across threads
			private ManualResetEvent mre1;
			private String s1;
			private object p1;
			private ManualResetEvent mre2;
			private String s2;
			private object p2;
			private ManualResetEvent mre3;
			private String s3;
			private object p3;
			private ManualResetEvent mre4;
			private String s4;
			private object p4;

			// Timeout used in test case
			private static int timeout = 2000;

			public void run(int port)
			{
				// Create and start a server and client.
				TcpListener server = null;
				TcpClient client = null;

				try
				{
					server = new TcpListener(IPAddress.Any, port);
					server.Start();
					client = new TcpClient("localhost", port);

					// Obtain the sockets from the two ends of the connection.  We are using the blocking AcceptSocket()
					// method here, which is OK for a test case.
					Socket serverSocket = server.AcceptSocket();
					Socket clientSocket = client.Client;

					// Wrap the two ends of the connection into StringSockets
					StringSocket sendSocket = new StringSocket(serverSocket, new UTF8Encoding());
					StringSocket receiveSocket = new StringSocket(clientSocket, new UTF8Encoding());

					// This will coordinate communication between the threads of the test cases
					mre1 = new ManualResetEvent(false);
					mre2 = new ManualResetEvent(false);
					mre3 = new ManualResetEvent(false);
					mre4 = new ManualResetEvent(false);

					// Make two receive request to begin with
					receiveSocket.BeginReceive(CompletedReceive1, 1);
					receiveSocket.BeginReceive(CompletedReceive2, 2);

					// Now send the data.  Hope the receive request didn't block!
					String msg = "Hello world.\nThis should be fine\neven though there are five messages here."
						+ "\n";
					sendSocket.BeginSend(msg, (e, o) => { }, null);

					// Make another receive request
					receiveSocket.BeginReceive(CompletedReceive3, 3);

					// Send another line
					sendSocket.BeginSend("It should send them back\n", (e, o) => { }, null);

					// Make final receive requests
					receiveSocket.BeginReceive(CompletedReceive4, 4);

					// Make sure the lines were received properly.
					Assert.AreEqual(true, mre1.WaitOne(timeout), "Timed out waiting 1");
					Assert.AreEqual("Hello world.", s1);
					Assert.AreEqual(1, p1);

					Assert.AreEqual(true, mre2.WaitOne(timeout), "Timed out waiting 2");
					Assert.AreEqual("This should be fine", s2);
					Assert.AreEqual(2, p2);

					Assert.AreEqual(true, mre3.WaitOne(timeout), "Timed out waiting 3");
					Assert.AreEqual("even though there are five messages here.", s3);
					Assert.AreEqual(3, p3);

					Assert.AreEqual(true, mre4.WaitOne(timeout), "Timed out waiting 4");
					Assert.AreEqual("It should send them back", s4);
					Assert.AreEqual(4, p4);
				}
				finally
				{
					server.Stop();
					client.Close();
				}
			}

			// These are the callbacks for the receive request.  We can't make assertions anywhere
			// but the main thread, so we write the values to member variables so they can be tested
			// on the main thread.
			private void CompletedReceive1(String s, Exception o, object payload)
			{
				s1 = s;
				p1 = payload;
				mre1.Set();
			}

			private void CompletedReceive2(string s, Exception e, object payload)
			{
				s2 = s;
				p2 = payload;
				mre2.Set();
			}

			private void CompletedReceive3(string s, Exception e, object payload)
			{
				s3 = s;
				p3 = payload;
				mre3.Set();
			}

			private void CompletedReceive4(string s, Exception e, object payload)
			{
				s4 = s;
				p4 = payload;
				mre4.Set();
			}
		}

    }
}
//sorry