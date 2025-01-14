namespace CLIChatApp;

class Program
{
    static async Task Main(string[] args)
    {
        //ChatServer chatServer = new ChatServer();
        Console.WriteLine("CLI Chat App v.1");
        Console.WriteLine("Run server Y/n?");

        string userInput = Console.ReadLine() ?? "n";
        if (userInput.ToLower() == "y")
        {
            RunServer();
            Console.WriteLine("Server running...");
        }
        else
        {
            Console.WriteLine("Server is not running..");
        }
    }

    static void RunServer()
    {
        ChatServer chatServer = new ChatServer();
        chatServer.StartServer(4444);
    }
}
