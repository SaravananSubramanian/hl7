using System;
using System.Net;
using System.Net.Sockets;

namespace SimpleTcpEchoServer
{
    public class Program
    {
        static void Main(string[] args)
        {
            TcpListener ourTcpListener;

            try
            {
                // Create a TCPListener to accept client connections through port 1080
                ourTcpListener = new TcpListener(IPAddress.Any, 1080);

                //start listening
                ourTcpListener.Start();

                Console.Write("Started TCP Listener...");
            }
            catch (Exception ex)
            {
                //if there was an error starting the listener then print the error and quit
                Console.WriteLine(ex.Message);
                return;
            }

            var receivedByteBuffer = new byte[200]; 

            for (;;)
            {
                // Run the listening loop forever
                // this will keep accepting and servicing client connections
                TcpClient acceptTcpClient = null;
                NetworkStream netStream = null;
                try
                {
                    Console.Write("Waiting for incoming client connections...");

                    acceptTcpClient = ourTcpListener.AcceptTcpClient(); // Get client connection
                    netStream = acceptTcpClient.GetStream();

                    Console.Write("Handling incoming client connection...");

                    // Keep receiving data from the client closes connection
                    var totalBytesReceivedFromClient = 0;
                    int bytesReceived; // Received byte count
                    while ((bytesReceived = netStream.Read(receivedByteBuffer, 0, receivedByteBuffer.Length)) > 0)
                    {
                        if (netStream.CanWrite)
                        {
                            //echo the received data back to the client 
                            netStream.Write(receivedByteBuffer, 0, bytesReceived);
                        }

                        totalBytesReceivedFromClient += bytesReceived;
                    }

                    Console.WriteLine("Echoed {0} bytes back to the client.", totalBytesReceivedFromClient);
                }
                catch (Exception e)
                {
                    //print any exceptions during the communications to the console
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    // Close the stream and the connection with the client
                    netStream?.Close();
                    netStream?.Dispose();
                    acceptTcpClient?.Close();
                }
            }
        }
    }
}
