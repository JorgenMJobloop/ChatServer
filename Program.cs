namespace CLIChatApp;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("CLI Chat App v.1");
        Console.WriteLine("Run server Y/n?");

        string userInput = Console.ReadLine() ?? "n";
        if (userInput.ToLower() == "y")
        {
            await RunServer();
        }
        else
        {
            Console.WriteLine("Server is not running..");
        }
        Console.WriteLine("Server running..");

    }

    static async Task RunServer()
    {
        ChatServer chatServer = new ChatServer();
        await chatServer.StartServer(4444);
    }

}
