using System.IO;
using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Concurrent;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;

public class Peer : IDisposable
{
    private TcpListener? listener;

    // Todo: add TSL or SSL instead of RSA to encrypt the TCP transmissions
    private bool isRunning = false;
    private readonly ConcurrentDictionary<TcpClient, SslStream> currentlyConnectedPeers = new ConcurrentDictionary<TcpClient, SslStream>();
    // TSL support
    private readonly X509Certificate2 serverCert;
    /// <summary>
    /// P2P class constructor
    /// </summary>
    public Peer()
    {
        serverCert = GenerateOrLoadedCertificate();
    }
    /// <summary>
    /// Start a new listener
    /// </summary>
    /// <param name="port">port to listen on</param>
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
                Console.WriteLine($"TSL Handshake completed with: {client.Client.RemoteEndPoint}");

                currentlyConnectedPeers.TryAdd(client, sslStream);
                Console.WriteLine($"New peer connected: {client.Client.RemoteEndPoint}");
                HandleClient(client, sslStream);
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occured when attempting to iniate the TSL Handskake\n ERROR:{e.Message}");
                client.Close();
            }
        }
    }
    /// <summary>
    /// Kill the server and clase the open port(s)
    /// </summary>
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
    /// <summary>
    /// Handle client interaction and assure end-to-end encryption by utilizing SslStream
    /// </summary>
    /// <param name="client">connected client</param>
    /// <param name="sslStream">encrypted stream</param>
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
    // TODO: [x]
    // Refactor this method
    /// <summary>
    /// Broadcast the messages in the P2P stream over the network.
    /// </summary>
    /// <param name="message">message to broadcast</param>
    /// <param name="sender">client(s) sending a(ny) message(s)</param>
    private void BroadcastMessages(string? message, TcpClient sender)
    {
        foreach (var peer in currentlyConnectedPeers.Keys)
        {
            if (peer == sender)
            {
                continue;
            }
            try
            {
                var stream = peer.GetStream();
                using var writer = new StreamWriter(currentlyConnectedPeers[peer]) { AutoFlush = true };
                writer.WriteLine(message);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to send a message to: {peer.Client.RemoteEndPoint}! Error message: {e.Message}");
            }
        }
    }
    // TODO: [x]
    // Refactor this method
    /// <summary>
    /// Connect to a new peer within the P2P network.
    /// </summary>
    /// <param name="address">Server address</param>
    /// <param name="port">Port to open</param>
    /// <param name="message">P2P Messages</param>
    public void ConnectToPeer(string address, int port, string clientCertificatePath)
    {
        using var client = new TcpClient(address, port);
        using var sslStream = new SslStream(client.GetStream(), false, ValidateServerCertificate);

        try
        {
            sslStream.AuthenticateAsClient(address, null, System.Security.Authentication.SslProtocols.Tls12, false);
            using var writer = new StreamWriter(sslStream) { AutoFlush = true };
            using var reader = new StreamReader(sslStream);

            writer.WriteLine("Hello, peer!");
            Console.WriteLine($"Encrypted message sent: {sslStream}\nfrom: {address}\non port: {port}");

            string? response = reader.ReadLine();
            if (!string.IsNullOrWhiteSpace(response))
            {
                Console.WriteLine($"Message recieved: {response}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"TSL Handshake failed: {e.Message}");
        }
    }
    /// <summary>
    /// Validate the server certificate
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="certificate"></param>
    /// <param name="chain"></param>
    /// <param name="sslPolicyErrors"></param>
    /// <returns></returns>
    private static bool ValidateServerCertificate(object? sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        if (sslPolicyErrors == SslPolicyErrors.None)
        {
            return true;
        }
        Console.WriteLine($"Certification error: {sslPolicyErrors}");

        return sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors;
    }
    /// <summary>
    /// Implementation of the IDisposable interface
    /// </summary>
    public void Dispose()
    {
        isRunning = false;
        listener?.Stop();
        foreach (var peer in currentlyConnectedPeers.Keys)
        {
            peer.Close();
        }
        StopListening();
    }
    /// <summary>
    /// Generate a new X509 Certificate2 on each peer.
    /// </summary>
    /// <returns>new X509Certificate2</returns>
    /// <exception cref="NotImplementedException">Generated TODO</exception>
    private X509Certificate2 GenerateOrLoadedCertificate()
    {

        const string certPath = "peer_certificate.pfx";
        const string password = "p2psecure";
        // Look for existing certificates on each peer
        if (File.Exists(certPath))
        {
            Console.WriteLine("Loading the server certificate..");
            return new X509Certificate2(certPath, password);
        }

        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            "CN=Certificate", rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1
        );

        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

        var certificate = request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5));

        byte[] certData = certificate.Export(X509ContentType.Pfx, password);
        File.WriteAllBytes(certPath, certData);

        return new X509Certificate2(certData, password);

        // TODO: Implement this method[x]: 52c2555->151615e
        //throw new NotImplementedException();
    }
}