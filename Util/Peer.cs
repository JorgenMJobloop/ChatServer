using System.IO;
using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;
using System.Net.Security;

public class Peer : IDisposable
{
    private TcpListener? listener;
    private readonly RSA rsa;


    // Todo: add TLS or SSL instead of RSA to encrypt the TCP transmissions
    // private SslClientAuthenticationOptions authenticationOptions = new SslClientAuthenticationOptions();
    private bool isRunning = false;
    private readonly ConcurrentDictionary<TcpClient, string?> currentlyConnectedPeers = new ConcurrentDictionary<TcpClient, string?>();

    public Peer()
    {
        rsa = RSA.Create();
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
            currentlyConnectedPeers.TryAdd(client, client.Client.RemoteEndPoint?.ToString());
            Console.WriteLine($"New peer connected: {client.Client.RemoteEndPoint}");
            HandleClient(client);
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

    private void HandleClient(TcpClient client)
    {
        var endpoint = client.Client.RemoteEndPoint?.ToString() ?? "Unknown endpoint";
        try
        {
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream);
            using var writer = new StreamWriter(stream) { AutoFlush = true };

            while (isRunning)
            {
                string? publicKey = reader.ReadLine();
                Console.WriteLine($"Recieved public key: {publicKey} from peer!");
                // print the public key to stdout
                string? ownPublicKey = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());
                writer.WriteLine(ownPublicKey);
                // recive the message
                string? encryptedMessage = reader.ReadLine();
                byte[] encryptedBytes = Convert.FromBase64String(encryptedMessage);
                // decrypt the message
                string message = Encoding.UTF8.GetString(rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.Pkcs1));
                Console.WriteLine($"Decrypted message: {message}");
                string? streamMessage = reader.ReadLine();
                if (streamMessage == null)
                {
                    isRunning = false;
                    break;
                }
                Console.WriteLine($"Message from {endpoint}: {streamMessage}");
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
        string? ownPublicKey = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());
        writer.WriteLine(ownPublicKey);
        string? peerPublicKey = reader.ReadLine();
        var peerRsa = RSA.Create();
        peerRsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(peerPublicKey), out _);
        // encrypt messages
        byte[] encryptedMessage = peerRsa.Encrypt(Encoding.UTF8.GetBytes(message), RSAEncryptionPadding.Pkcs1);
        writer.WriteLine(Convert.ToBase64String(encryptedMessage));
        Console.WriteLine("Message sent");
    }

    public void Dispose()
    {
        StopListening();
    }

}