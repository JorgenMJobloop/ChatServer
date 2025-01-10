using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json;

/// <summary>
/// A TCP Chatserver CLI (MVP)
/// </summary>
public class ChatServer
{
    // Fields
    private TcpListener? tcpListener;
    /// <summary>
    /// A list of connected clients
    /// </summary>
    private List<TcpClient> tcpClients = new List<TcpClient>();

    //Referanse til en ny klient som blir passet til clientHandler on threadstart. 
    private TcpClient _newClient { get; set; }

    private string? Password { get; set; }


    public List<Thread?> ClientThreads { get; set; } = [];
    /// <summary>
    /// Our main listening method
    /// </summary>
    /// <param name="port">the port our server will listen on</param>
    public void StartServer(int port)
    {
        tcpListener = new TcpListener(IPAddress.Any, port);
        // start up the connection
        tcpListener.Start();
        Console.WriteLine($"Server started on port {port}...");


        while (true)
        {
            // client listener
            var client = tcpListener.AcceptTcpClient();
            _newClient = client;
            tcpClients.Add(client);
            Console.WriteLine("A new client has connected to the server!");

            // todo: create a client handler
            var task = ClientHandler();
        }
    }

    private async Task ClientHandler()
    {
        //Lager en egen referanse til _newClient ved threadstart, herfra er den separat fra mainthread.
        var client = _newClient;
        var stream = client.GetStream();
        var reader = new StreamReader(stream);
        try
        {
            while (true)
            {
                var message = await reader.ReadLineAsync();
                if (message == null)
                {
                    break;
                }
                Console.WriteLine($"Client: {message}");
                // todo: create a message method
                //Leverer handle til threaden's client som identifier til writeMessage.
                WriteMessage(message, client.Client.Handle);
            }
        }
        catch (Exception)
        {
            Console.WriteLine("A client has disconnected!");
        }
        finally
        {
            // close the connection
            tcpClients.Remove(client);
            client.Close();
        }

    }

    private void WriteMessage(string message, nint handle)
    {
        //Luker vekk client med handler lik den som spawned WriteMessage.
        foreach (var client in tcpClients)
        {
            try
            {
                // new writer, use autoflush
                var writer = new StreamWriter(client.GetStream()) { AutoFlush = true };
                Console.WriteLine(handle.GetHashCode());
                Console.WriteLine("Connected Clients: " + tcpClients.Count);
                writer.WriteLine(handle + ": " + message);
            }
            catch
            {
                // client no longer active!
            }

        }

    }

    private string GenerateNewSHA256Hash(string rawData)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte bytes in hash)
            {
                stringBuilder.Append(bytes.ToString("x2"));
            }
            return stringBuilder.ToString();
        }
    }
    private void GenerateUserToken()
    {
        if (!File.Exists("user_token.json"))
        {
            File.Create("user_token.json");
        }
        var token = JsonObject.Parse(_newClient.ToString());
        string? serializeObject = JsonSerializer.Serialize(token);
        File.AppendText(serializeObject);
    }
}