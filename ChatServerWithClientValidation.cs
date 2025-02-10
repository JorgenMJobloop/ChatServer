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

public class ChatServerWithClientValidation
{
    // Fields
    private TcpListener? tcpListener;
    /// <summary>
    /// A list of connected clients
    /// </summary>
    private Dictionary<TcpClient, string> clientTokens = new Dictionary<TcpClient, string>();

    /// <summary>
    /// Our main listening method
    /// </summary>
    /// <param name="port">the port our server will listen on</param>
    public void StartServer(int port)
    {
        bool isRunning = true;
        tcpListener = new TcpListener(IPAddress.Any, port);
        // start up the connection
        tcpListener.Start();
        Console.WriteLine($"Server started on port {port}...");

        while (isRunning)
        {
            // client listener
            var client = tcpListener.AcceptTcpClient();
            // lock threads
            ThreadPool.QueueUserWorkItem(HandleClient, client);
            Console.WriteLine("A new client has connected to the server!");

        }
    }
    /// <summary>
    /// Handle client interaction
    /// </summary>
    /// <param name="obj">any object, preferably a TcpClient class object</param>
    private void HandleClient(object? obj)
    {
        var client = (TcpClient)obj!;
        using var stream = client.GetStream();
        using var streamReader = new StreamReader(stream);
        using var streamWriter = new StreamWriter(stream) { AutoFlush = true };

        try
        {
            string? tokenMessage = streamReader.ReadLine();
            if (tokenMessage == null || tokenMessage.StartsWith("TOKEN:"))
            {
                Console.WriteLine("Invalid client connection: No token provided!\nClosing connection!");
                client.Close();
                return;
            }
            string token = tokenMessage.Substring(6);
            Console.WriteLine($"Token recieved: {token}");

            lock (clientTokens)
            {
                clientTokens[client] = token;
            }

            while (true)
            {
                string? message = streamReader.ReadLine();
                if (message == null)
                {
                    break;
                }
                Console.WriteLine($"Client {token}: {message}");
                WriteMessage($"{token}: {message}", client);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occured {e.Message}");
        }
        finally
        {
            lock (clientTokens)
            {
                clientTokens.Remove(client);
            }
            client.Close(); // client disconnected
        }
    }

    private void WriteMessage(string message, TcpClient sender)
    {
        lock (clientTokens)
            //Luker vekk client med handler lik den som spawned WriteMessage.
            foreach (var client in clientTokens.Keys)
            {
                if (client == sender)
                {
                    continue;
                }
                try
                {
                    // new writer, use autoflush
                    using var writer = new StreamWriter(client.GetStream()) { AutoFlush = true };
                    Console.WriteLine("Connected Clients: " + clientTokens.Count); // count number of connected clients
                    writer.WriteLine(sender.GetHashCode() + ": " + message); // print out the message stdout
                }
                catch
                {
                    // client no longer active!
                    Debug.WriteLine("Client disconnected!");
                }
            }
    }
}