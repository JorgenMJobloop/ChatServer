using System.IO;
using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Peer
{
    private TcpListener? listener;
    private readonly RSA rsa;

    public Peer()
    {
        rsa = RSA.Create();
    }

    public void StartListening(int port)
    {
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start(); // start the connection
        Console.WriteLine("Listening for peers...");

        while (true)
        {
            var client = listener.AcceptTcpClient();
            HandleClient(client);
        }
    }

    private void HandleClient(TcpClient client)
    {
        using var stream = client.GetStream();
        using var reader = new StreamReader(stream);
        using var writer = new StreamWriter(stream) { AutoFlush = true };

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

    }

    public void ConnectToPeer(string address, int port, string message)
    {
        var client = new TcpClient(address, port);
        using var stream = client.GetStream();
        using var reader = new StreamReader(stream);
        using var writer = new StreamWriter(stream) { AutoFlush = true };

        // send the public key
        string? ownPublicKey = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());
        writer.Write(ownPublicKey);

        string? peerPublicKey = reader.ReadLine();
        var peerRsa = RSA.Create();
        peerRsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(peerPublicKey), out _);

        // encrypt messages
        byte[] encryptedMessage = peerRsa.Encrypt(Encoding.UTF8.GetBytes(message), RSAEncryptionPadding.Pkcs1);
        writer.WriteLine(Convert.ToBase64String(encryptedMessage));
        Console.WriteLine("Message sent");
    }
}