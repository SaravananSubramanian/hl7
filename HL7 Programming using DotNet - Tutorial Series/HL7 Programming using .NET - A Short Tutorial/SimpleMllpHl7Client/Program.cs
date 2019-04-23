using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SimpleMllpHl7Client
{
    public class Program
    {
        private static char START_OF_BLOCK = (char)0x0B;
        private static char END_OF_BLOCK = (char)0x1C;
        private static char CARRIAGE_RETURN = (char)13;

        static void Main(string[] args)
        {
            TcpClient ourTcpClient = null;
            NetworkStream networkStream = null;

            var testHl7MessageToTransmit = new StringBuilder();

            //a HL7 test message that is enveloped with MLLP as described in my article
            testHl7MessageToTransmit.Append(START_OF_BLOCK)
                .Append("MSH|^~\\&|AcmeHIS|StJohn|CATH|StJohn|20061019172719||ORM^O01|MSGID12349876|P|2.3")
                .Append(CARRIAGE_RETURN)
                .Append("PID|||20301||Durden^Tyler^^^Mr.||19700312|M|||88 Punchward Dr.^^Los Angeles^CA^11221^USA|||||||")
                .Append(CARRIAGE_RETURN)
                .Append("PV1||O|OP^^||||4652^Paulson^Robert|||OP|||||||||9|||||||||||||||||||||||||20061019172717|20061019172718")
                .Append(CARRIAGE_RETURN)
                .Append("ORC|NW|20061019172719")
                .Append(CARRIAGE_RETURN)
                .Append("OBR|1|20061019172719||76770^Ultrasound: retroperitoneal^C4|||12349876")
                .Append(CARRIAGE_RETURN)
                .Append(END_OF_BLOCK)
                .Append(CARRIAGE_RETURN);

            try
            {
                //initiate a TCP client connection to local loopback address at port 1080
                ourTcpClient = new TcpClient();

                ourTcpClient.Connect(new IPEndPoint(IPAddress.Loopback, 1080));

                Console.WriteLine("Connected to server....");

                //get the IO stream on this connection to write to
                networkStream = ourTcpClient.GetStream();

                //use UTF-8 and either 8-bit encoding due to MLLP-related recommendations
                var byteBuffer = Encoding.UTF8.GetBytes(testHl7MessageToTransmit.ToString());

                //send a message through this connection using the IO stream
                networkStream.Write(byteBuffer, 0, byteBuffer.Length);

                Console.WriteLine("Data was sent data to server successfully....");

                Console.WriteLine("This client is not technically complete as it does not receive MLLP message acknowledgement back....");

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
                networkStream?.Dispose();
                ourTcpClient?.Close();
            }
        }
    }
}
