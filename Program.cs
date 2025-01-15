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
            await RunServerAsync();
            Console.WriteLine("Server running...");
        }
        else
        {
            Console.WriteLine("Server is not running..");
        }
    }
    static async Task RunServerAsync()
    {
        ChatServerAsync chatServer = new ChatServerAsync();
        await chatServer.StartServerAsync(4444);
    }
    static void RunServerWithClientValidation()
    {
        ChatServerWithClientValidation chatServerWithClientValidation = new ChatServerWithClientValidation();
        chatServerWithClientValidation.StartServer(8888); // run the server
    }
    static void RunP2P()
    {
        Peer peer = new Peer();
        peer.StartListening(30303); // start a new P2P connection
        peer.ConnectToPeer("127.0.0.1", 30303, "Hello P2P!"); // connect to peer(s)
    }
}
