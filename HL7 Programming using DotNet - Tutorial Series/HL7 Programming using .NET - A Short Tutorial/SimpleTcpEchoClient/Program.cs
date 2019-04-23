using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SimpleTcpEchoClient
{
    public class Program
    {
        static void Main(string[] args)
        {
            TcpClient ourTcpClient = null;
            NetworkStream networkStream = null;

            try
            {
                //initiate a TCP client connection to local loopback address at port 1080
                ourTcpClient = new TcpClient();

                ourTcpClient.Connect(new IPEndPoint(IPAddress.Loopback, 1080));

                Console.WriteLine("Connected to server....");

                //get the IO stream on this connection to write to
                networkStream = ourTcpClient.GetStream();

                //use UTF-8 and either 8-bit encoding due to MLLP-related recommendations
                var messageToTransmit = "Hello from Client";
                var byteBuffer = Encoding.UTF8.GetBytes(messageToTransmit);

                //send a message through this connection using the IO stream
                networkStream.Write(byteBuffer, 0, byteBuffer.Length);

                Console.WriteLine("Data was sent data to server successfully....");

                var bytesReceivedFromServer = networkStream.Read(byteBuffer, 0, byteBuffer.Length);

                // Our server for this example has been designed to echo back the message
                // keep reading from this stream until the message is echoed back
                while (bytesReceivedFromServer < byteBuffer.Length)
                {
                    bytesReceivedFromServer = networkStream.Read(byteBuffer, 0, byteBuffer.Length);
                    if (bytesReceivedFromServer == 0)
                    {
                        //exit the reading loop since there is no more data
                        break; 
                    }
                }
                var receivedMessage = Encoding.UTF8.GetString(byteBuffer);

                Console.WriteLine("Received message from server: {0}", receivedMessage);
        
                Console.WriteLine("Press any key to exit program...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                //display any exceptions that occur to console
                Console.WriteLine(ex.Message);
            }
            finally
            {
                //close the IO strem and the TCP connection
                networkStream?.Close();
                ourTcpClient?.Close();
            }
        }
    }
}
