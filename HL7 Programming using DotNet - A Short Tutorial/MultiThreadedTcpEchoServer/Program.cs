using System;
using System.Net;
using System.Net.Sockets;

namespace MultiThreadedTcpEchoServer
{
    public class Program
    {
        static void Main(string[] args)
        {
            var main = new OurSimpleMultiThreadedTcpServer();
            main.StartOurTcpServer();  //starting the server

            Console.WriteLine("Press any key to exit program...");
            Console.ReadLine();
        }
    }

    class OurSimpleMultiThreadedTcpServer
    {
        private readonly TcpListener _tcpListener = new TcpListener(IPAddress.Any, 1080);

        public void StartOurTcpServer()
        {
            //start the TCP listener that we have instantiated
            _tcpListener.Start();

            Console.WriteLine("Started server successfully...");

            //start processing client connections to this server
            StartProcessingClientConnections();  
        }

        private void StartProcessingClientConnections()
        {
            //connections are handled through an async callback delegate
            //Read the .NET documentation on this and more powerful methods available for use
            _tcpListener.BeginAcceptTcpClient(HandleIncomingConnections, _tcpListener);  
        }

        private void HandleIncomingConnections(IAsyncResult result)  
        {
            //call this recursively until the server is shutdown 
            //all connections are handled concurrently as as result of the async call back mechanism
            StartProcessingClientConnections();  
            var client = _tcpListener.EndAcceptTcpClient(result);  

            var receivedByteBuffer = new byte[200];
            var netStream = client.GetStream();

            try
            {
                // Keep receiving data from the client closes connection
                var totalBytesReceivedFromClient = 0;
                int bytesReceived; // Received byte count

                //keeping reading until there is data available from the client and echo it back
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
                netStream.Close();
                netStream.Dispose();
                client.Close();
            }

        }
    }
}