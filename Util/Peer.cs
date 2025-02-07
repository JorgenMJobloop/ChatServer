using System.IO;
using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public class Peer : IDisposable
{
    private TcpListener? listener;

    // Todo: add TLS or SSL instead of RSA to encrypt the TCP transmissions
    private bool isRunning = false;
    private readonly ConcurrentDictionary<TcpClient, SslStream> currentlyConnectedPeers = new ConcurrentDictionary<TcpClient, SslStream>();
    // TSL support
    private readonly X509Certificate2 serverCert;

    public Peer(string certificatePath, string certificatePassword)
    {
        serverCert = new X509Certificate2(certificatePath, certificatePassword);
    }

    public void StartListening(int port)
    {
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start(); // start the connection
        Console.WriteLine("Listening for peers...");
        isRunning = true;
        while (isRunning)
        {
            var client = listener.AcceptTcpClient();
            var sslStream = new SslStream(client.GetStream(), false);
            try
            {
                sslStream.AuthenticateAsServer(serverCert, true, System.Security.Authentication.SslProtocols.Tls12, false);
                Console.WriteLine($"TLS Handshake completed with: {client.Client.RemoteEndPoint}");

                currentlyConnectedPeers.TryAdd(client, sslStream);
                Console.WriteLine($"New peer connected: {client.Client.RemoteEndPoint}");
                HandleClient(client, sslStream);
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occured when attempting to iniate the TLS Handskake\n ERROR:{e.Message}");
                client.Close();
            }
        }
    }

    public void StopListening()
    {
        isRunning = false;
        listener?.Stop(); // stop the connection and close the port
        Console.WriteLine("Connection terminated...");
        foreach (var connectedClients in currentlyConnectedPeers.Keys)
        {
            connectedClients.Close();
        }
        currentlyConnectedPeers.Clear();
    }

    private void HandleClient(TcpClient client, SslStream sslStream)
    {
        Task.Run(async () =>
        {
            try
            {
                using var reader = new StreamReader(sslStream);
                using var writer = new StreamWriter(sslStream) { AutoFlush = true };
                while (isRunning)
                {
                    string? message = await reader.ReadLineAsync();
                    if (message == null)
                    {
                        break;
                    }
                    Console.WriteLine($"Peer: {client.Client.RemoteEndPoint}\n message: {message}");
                    BroadcastMessages(message, client);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occured: {e.Message}");
            }
            finally
            {
                currentlyConnectedPeers.TryRemove(client, out _);
                client.Close();
            }
        });
    }

    private void BroadcastMessages(string? message, TcpClient sender)
    {
        foreach (var client in currentlyConnectedPeers.Keys)
        {
            if (client == sender)
            {
                continue;
            }
            try
            {
                var stream = client.GetStream();
                using var writer = new StreamWriter(stream) { AutoFlush = true };
                writer.WriteLine(message);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to send a message to: {client.Client.RemoteEndPoint}! Error message: {e.Message}");
            }
        }
    }

    public void ConnectToPeer(string address, int port, string message)
    {
        var client = new TcpClient(address, port);
        using var stream = client.GetStream();
        using var reader = new StreamReader(stream);
        using var writer = new StreamWriter(stream) { AutoFlush = true };
        // send the public key
        string? peerPublicKey = reader.ReadLine();
        Console.WriteLine("Message sent");
    }

    public void Dispose()
    {
        StopListening();
    }
}