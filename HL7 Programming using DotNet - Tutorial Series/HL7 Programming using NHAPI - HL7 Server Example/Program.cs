using System.Net;
using System.Net.Sockets;
using System.Text;
using NHapi.Base.Model;
using NHapi.Base.Parser;

namespace Com.SaravananSubramanian.Nhapi.Hl7Server;

/// <summary>
/// Main entry point for the HL7 V2 server application.
/// Uses a custom MLLP server for message reception with NHAPI for parsing.
/// </summary>
/// <remarks>
/// <para><b>Usage:</b> dotnet run [port] (default: 2575)</para>
/// <para><b>Testing:</b> Use the Sending HL7 Messages example or any MLLP client.</para>
/// <para><b>Dependencies:</b> NHAPI 3.2.4 (NuGet: nHapi)</para>
///
/// This server demonstrates:
/// - TCP-based MLLP (Minimal Lower Layer Protocol) server
/// - Message routing based on message type
/// - Handling ADT, ORM, ORU, and other message types
/// - Generating appropriate ACK responses using NHAPI
///
/// MLLP framing uses:
/// - Start Block (SB): 0x0B (vertical tab)
/// - End Block (EB): 0x1C (file separator)
/// - Carriage Return (CR): 0x0D
/// </remarks>
public class Program
{
    private const int DefaultPort = 2575; // Standard HL7 port

    public static void Main(string[] args)
    {
        var port = args.Length > 0 && int.TryParse(args[0], out var p) ? p : DefaultPort;

        Console.WriteLine("==============================================");
        Console.WriteLine("  NHAPI HL7 V2 Server");
        Console.WriteLine("==============================================");
        Console.WriteLine();
        Console.WriteLine("This server demonstrates handling of:");
        Console.WriteLine("  - ADT (Admission/Discharge/Transfer) messages");
        Console.WriteLine("  - ORM (Order) messages");
        Console.WriteLine("  - ORU (Observation Result) messages");
        Console.WriteLine("  - Generic HL7 V2 messages");
        Console.WriteLine();

        // Create and start the HL7 server
        var server = new Hl7Server(port);

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        var serverTask = server.StartAsync(cts.Token);

        Console.WriteLine($"Server listening on port {port}");
        Console.WriteLine("Press Ctrl+C or Enter to stop the server...");
        Console.WriteLine(new string('-', 50));

        // Wait for Enter key or Ctrl+C
        Console.ReadLine();
        cts.Cancel();

        Console.WriteLine("\nStopping server...");
        try
        {
            serverTask.Wait(TimeSpan.FromSeconds(5));
        }
        catch (AggregateException)
        {
            // Expected when cancelled
        }
        Console.WriteLine("Server stopped.");
    }
}

/// <summary>
/// HL7 Server that handles MLLP connections and routes messages to handlers.
/// Implements the MLLP (Minimal Lower Layer Protocol) for HL7 message transport.
/// </summary>
public class Hl7Server
{
    private readonly int _port;
    private readonly Dictionary<string, IHl7MessageHandler> _handlers = new();
    private readonly MessageRouter _router = new();
    private readonly PipeParser _parser = new();
    private TcpListener? _listener;

    // MLLP framing characters
    private const char StartBlock = (char)0x0B;  // Vertical Tab
    private const char EndBlock = (char)0x1C;    // File Separator
    private const char CarriageReturn = (char)0x0D;

    public Hl7Server(int port)
    {
        _port = port;
        RegisterDefaultHandlers();
    }

    private void RegisterDefaultHandlers()
    {
        RegisterHandler("ADT", new AdtMessageHandler());
        RegisterHandler("ORM", new OrmMessageHandler());
        RegisterHandler("ORU", new OruMessageHandler());
        RegisterHandler("*", new GenericMessageHandler()); // Fallback handler

        Console.WriteLine("Registered message handlers:");
        foreach (var handler in _handlers)
        {
            Console.WriteLine($"  - {handler.Key}: {handler.Value.GetType().Name}");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Registers a message handler for a specific message type.
    /// </summary>
    /// <param name="messageType">The message type (e.g., "ADT", "ORM", "ORU") or "*" for fallback</param>
    /// <param name="handler">The handler to process messages of this type</param>
    public void RegisterHandler(string messageType, IHl7MessageHandler handler)
    {
        _handlers[messageType.ToUpperInvariant()] = handler;
    }

    /// <summary>
    /// Starts the server and begins listening for connections.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _listener = new TcpListener(IPAddress.Any, _port);
        _listener.Start();

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync(cancellationToken);
                _ = HandleClientAsync(client, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        finally
        {
            _listener.Stop();
        }
    }

    /// <summary>
    /// Handles a single client connection, processing all messages until disconnection.
    /// </summary>
    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        var endpoint = client.Client.RemoteEndPoint?.ToString() ?? "unknown";
        Console.WriteLine($"\n[Connection] Client connected from {endpoint}");

        try
        {
            await using var stream = client.GetStream();
            var buffer = new byte[8192];
            var messageBuffer = new StringBuilder();

            while (!cancellationToken.IsCancellationRequested && client.Connected)
            {
                var bytesRead = await stream.ReadAsync(buffer, cancellationToken);
                if (bytesRead == 0) break; // Client disconnected

                messageBuffer.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                // Process complete MLLP frames
                var data = messageBuffer.ToString();
                var startIndex = data.IndexOf(StartBlock);

                while (startIndex >= 0)
                {
                    var endIndex = data.IndexOf(EndBlock, startIndex);
                    if (endIndex < 0) break; // Incomplete frame

                    // Extract HL7 message (between SB and EB)
                    var hl7Message = data.Substring(startIndex + 1, endIndex - startIndex - 1);

                    // Process and get response
                    var response = ProcessMessage(hl7Message);

                    // Send MLLP-framed response
                    var responseBytes = Encoding.UTF8.GetBytes($"{StartBlock}{response}{EndBlock}{CarriageReturn}");
                    await stream.WriteAsync(responseBytes, cancellationToken);

                    // Remove processed message from buffer
                    data = data[(endIndex + 2)..]; // Skip EB and CR
                    startIndex = data.IndexOf(StartBlock);
                }

                messageBuffer.Clear();
                messageBuffer.Append(data);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Console.WriteLine($"[Connection] Error handling client {endpoint}: {ex.Message}");
        }
        finally
        {
            client.Close();
            Console.WriteLine($"[Connection] Client disconnected: {endpoint}");
        }
    }

    /// <summary>
    /// Processes an HL7 message and returns the response.
    /// Routes messages to appropriate handlers and generates ACK responses.
    /// </summary>
    private string ProcessMessage(string rawMessage)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        Console.WriteLine($"\n[{timestamp}] Message received:");

        try
        {
            // Log incoming message
            LogMessage(rawMessage, "  IN: ");

            // Determine message type and get appropriate handler
            var messageType = _router.GetMessageType(rawMessage);
            var handler = GetHandler(messageType);

            Console.WriteLine($"  Message Type: {_router.GetFullMessageType(rawMessage)}");
            Console.WriteLine($"  Handler: {handler.GetType().Name}");

            // Process the message
            var responseString = handler.HandleMessage(rawMessage);

            Console.WriteLine($"[{timestamp}] Response sent:");
            LogMessage(responseString, "  OUT: ");

            return responseString;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Error processing message: {ex.Message}");

            // Generate error ACK
            try
            {
                var errorAck = AckGenerator.CreateNack(
                    rawMessage,
                    "AE",
                    $"Processing error: {ex.Message}");
                return errorAck;
            }
            catch
            {
                // If we can't even generate an ACK, return a minimal error response
                return CreateMinimalErrorAck();
            }
        }
    }

    private IHl7MessageHandler GetHandler(string messageType)
    {
        if (_handlers.TryGetValue(messageType.ToUpperInvariant(), out var handler))
        {
            return handler;
        }

        // Try wildcard/fallback handler
        if (_handlers.TryGetValue("*", out var fallback))
        {
            return fallback;
        }

        throw new InvalidOperationException($"No handler registered for message type: {messageType}");
    }

    private static void LogMessage(string message, string prefix)
    {
        var lines = message.Split('\r', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines.Take(5)) // Show first 5 segments
        {
            Console.WriteLine($"{prefix}{line}");
        }
        if (lines.Length > 5)
        {
            Console.WriteLine($"{prefix}... ({lines.Length - 5} more segments)");
        }
    }

    private static string CreateMinimalErrorAck()
    {
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        return $"MSH|^~\\&|SERVER|FACILITY|CLIENT|FACILITY|{timestamp}||ACK|{timestamp}|P|2.4\rMSA|AE|ERROR|Server processing error\r";
    }
}
