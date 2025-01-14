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
using System.Diagnostics;
using System.Threading.Tasks;

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
    private readonly List<TcpClient> clients = new List<TcpClient>();
    private TcpClient? newClient { get; set; }
    /// <summary>
    /// Store a hashed password in-memory when server is running
    /// </summary>
    private string? Password { get; set; }
    //public List<Thread?> ClientThreads { get; set; } = [];

    /// <summary>
    /// Our main listening method
    /// </summary>
    /// <param name="port">the port our server will listen on</param>
    public async Task StartServerAsync(int port)
    {
        bool isRunning = true;
        tcpListener = new TcpListener(IPAddress.Any, port);
        // start up the connection
        tcpListener.Start();
        Console.WriteLine($"Server started on port {port}...");

        while (isRunning)
        {
            // client listener
            var client = await tcpListener.AcceptTcpClientAsync();
            // lock threads
            lock (clients) clients.Add(client);
            _ = Task.Run(() => HandleClientAsync(client));
            Console.WriteLine("A new client has connected to the server!");
            if (client.Connected == false)
            {
                isRunning = false;
                break;
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        using var stream = client.GetStream();
        using var streamReader = new StreamReader(stream);
        using var streamWriter = new StreamWriter(stream) { AutoFlush = true };

        try
        {
            while (true)
            {
                string? message = await streamReader.ReadLineAsync();
                if (message == null)
                {
                    break;
                }
                Console.WriteLine($"");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occured {e.Message}");
        }
        finally
        {
            lock (clients) clients.Remove(client);
            client.Close(); // client disconnected
        }
    }

    private void WriteMessage(string message, TcpClient sender)
    {
        lock (clients)
            //Luker vekk client med handler lik den som spawned WriteMessage.
            foreach (var client in clients)
            {
                if (client == sender)
                {
                    continue;
                }
                try
                {
                    // new writer, use autoflush
                    using var writer = new StreamWriter(client.GetStream()) { AutoFlush = true };
                    Console.WriteLine("Connected Clients: " + clients.Count); // count number of connected clients
                    writer.WriteLine(sender.GetHashCode() + ": " + message); // print out the message stdout
                }
                catch
                {
                    // client no longer active!
                }
            }
    }
}