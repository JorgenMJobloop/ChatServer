using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

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
            tcpClients.Add(client);
            Console.WriteLine("A new client has connected to the server!");

            // todo: create a client handler
            var clientThread = new Thread(ClientHandler);
        }
    }

    private void ClientHandler(object clientObject)
    {
        var client = (TcpClient)clientObject;
        var stream = client.GetStream();
        var reader = new StreamReader(stream);

        try
        {
            while (true)
            {
                var message = reader.ReadLine();
                if (message == null)
                {
                    break;
                }
                Console.WriteLine($"Client: {message}");
                // todo: create a message method
                WriteMessage(message);
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

    private void WriteMessage(string message)
    {
        foreach (var client in tcpClients)
        {
            try
            {
                // new writer, use autoflush
                var writer = new StreamWriter(client.GetStream()) { AutoFlush = true };
                writer.WriteLine(message);
            }
            catch
            {
                // client no longer active!
            }
        }
    }
}