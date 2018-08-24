using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SimpleMultiThreadedMllpHl7Server
{
    class OurSimpleMultiThreadedMllpHl7Server
    {
        private TcpListener _tcpListener;
        private static char START_OF_BLOCK = (char)0x0B;
        private static char END_OF_BLOCK = (char)0x1C;
        private static char CARRIAGE_RETURN = (char)13;
        private static int MESSAGE_CONTROL_ID_LOCATION = 9;
        private static char FIELD_DELIMITER = '|';

        public void StartServer(int portToListenOn)
        {
            _tcpListener = new TcpListener(IPAddress.Any, portToListenOn);

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
            var tcpClientConnection = _tcpListener.EndAcceptTcpClient(result);

            Console.WriteLine("A client connection was initiated from " + tcpClientConnection.Client.RemoteEndPoint);

            var receivedByteBuffer = new byte[200];
            var netStream = tcpClientConnection.GetStream();

            try
            {
                // Keep receiving data from the client closes connection
                int bytesReceived; // Received byte count
                var hl7Data = "";

                //keeping reading until there is data available from the client and echo it back
                while ((bytesReceived = netStream.Read(receivedByteBuffer, 0, receivedByteBuffer.Length)) > 0)
                {
                    hl7Data += Encoding.UTF8.GetString(receivedByteBuffer, 0, bytesReceived);

                    // Find start of MLLP frame, a VT character ...
                    var startOfMllpEnvelope = hl7Data.IndexOf(START_OF_BLOCK);
                    if (startOfMllpEnvelope >= 0)
                    {
                        // Now look for the end of the frame, a FS character
                        var end = hl7Data.IndexOf(END_OF_BLOCK);
                        if (end >= startOfMllpEnvelope) //end of block received
                        {
                            //if both start and end of block are recognized in the data transmitted, then extract the entire message
                            var hl7MessageData = hl7Data.Substring(startOfMllpEnvelope + 1, end - startOfMllpEnvelope);

                            //create a HL7 acknowledgement message
                            var ackMessage = GetSimpleAcknowledgementMessage(hl7MessageData);

                            Console.WriteLine(ackMessage);

                            //echo the received data back to the client 
                            var buffer = Encoding.UTF8.GetBytes(ackMessage);

                            if (netStream.CanWrite)
                            {
                                netStream.Write(buffer, 0, buffer.Length);

                                Console.WriteLine("Ack message was sent back to the client...");
                            }
                        }
                    }

                }

                
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
                tcpClientConnection.Close();
            }

        }

        private string GetSimpleAcknowledgementMessage(string incomingHl7Message)
        {
            if (string.IsNullOrEmpty(incomingHl7Message))
                throw new ApplicationException("Invalid HL7 message for parsing operation. Please check your inputs");

            //retrieve the message control ID of the incoming HL7 message 
            var messageControlId = GetMessageControlID(incomingHl7Message);

            //build an acknowledgement message and include the control ID with it
            var ackMessage = new StringBuilder();
            ackMessage = ackMessage.Append(START_OF_BLOCK)
                .Append("MSH|^~\\&|||||||ACK||P|2.2")
                .Append(CARRIAGE_RETURN)
                .Append("MSA|AA|")
                .Append(messageControlId)
                .Append(CARRIAGE_RETURN)
                .Append(END_OF_BLOCK)
                .Append(CARRIAGE_RETURN);

            return ackMessage.ToString();
        }

        private string GetMessageControlID(string incomingHl7Message)
        {
            
            var fieldCount = 0;
            //parse the message into segments using the end of segment separter
            var hl7MessageSegments = incomingHl7Message.Split(CARRIAGE_RETURN);

            //tokenize the MSH segment into fields using the field separator
            var hl7FieldsInMshSegment = hl7MessageSegments[0].Split(FIELD_DELIMITER);

            //retrieve the message control ID in order to reply back with the message ack
            foreach (var field in hl7FieldsInMshSegment)
            {
                if (fieldCount == MESSAGE_CONTROL_ID_LOCATION)
                {
                    return field;
                }
                fieldCount++;
            }

            return string.Empty;
        }
    }
}