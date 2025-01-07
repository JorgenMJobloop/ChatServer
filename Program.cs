﻿namespace CLIChatApp;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("CLI Chat App v.1");
        Console.WriteLine("Run server Y/n?");

        string userInput = Console.ReadLine() ?? "n";
        if (userInput.ToLower() == "y")
        {
            RunServer();
        }
        else
        {
            Console.WriteLine("Server is not running..");
        }
        Console.WriteLine("Server running..");

    }

    static void RunServer()
    {
        ChatServer chatServer = new ChatServer();
        chatServer.StartServer(4444);
    }

}