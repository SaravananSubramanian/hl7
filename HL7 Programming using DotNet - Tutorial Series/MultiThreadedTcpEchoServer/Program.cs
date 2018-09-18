using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MultiThreadedTcpEchoServer
{
    public class Program
    {
        static void Main(string[] args)
        {
            var main = new OurSimpleMultiThreadedTcpServer();
            //starting the server
            main.StartOurTcpServer(1080);  

            Console.WriteLine("Press any key to exit program...");
            Console.ReadLine();
        }
    }

    class OurSimpleMultiThreadedTcpServer
    {
        private TcpListener _tcpListener;

        public void StartOurTcpServer(int portNumberToListenOn)
        {
            try
            {
                _tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 1080);

                //start the TCP listener that we have instantiated
                _tcpListener.Start();

                Console.WriteLine("Started server successfully...");

                while (true)
                {
                    //wait for client connections to come in
                    var incomingTcpClientConnection = _tcpListener.AcceptTcpClient();

                    Console.WriteLine("Accepted incoming client connection...");

                    //create a new thread to process this client connection
                    var clientProcessingThread = new Thread(ProcessClientConnection);

                    //start processing client connections to this server
                    clientProcessingThread.Start(incomingTcpClientConnection);
                }
                
            }
            catch (Exception ex)
            {
                //print any exceptions during the communications to the console
                Console.WriteLine(ex.Message);
            }
            finally
            {
                //stop the TCP listener before you dispose of it
                _tcpListener?.Stop();
            }
        }

        private void ProcessClientConnection(object argumentForThreadProcessing)
        {
            var tcpClient = (TcpClient) argumentForThreadProcessing;
            var receivedByteBuffer = new byte[200];
            var netStream = tcpClient.GetStream();

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
                        netStream.Flush();
                    }

                    totalBytesReceivedFromClient += bytesReceived;
                }

                Console.WriteLine("Echoed {0} bytes back to the client.", totalBytesReceivedFromClient);
            }
            catch (Exception e)
            {
                //print any exceptions during the communications to the console
                //in real-life, always do something about exceptions
                Console.WriteLine(e.Message);
            }
            finally
            {
                // Close the stream and the connection with the client
                netStream.Close();
                netStream.Dispose();
                tcpClient.Close();
            }

        }
    }
}