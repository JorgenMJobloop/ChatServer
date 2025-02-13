using System.Collections;
using System.Diagnostics;

namespace CLIChatApp;

class Program
{
    static async Task Main(string[] args)
    {
        // @@@@@@@@@@@@@@@@@
        // Test suite below
        // Tests();
        // @@@@@@@@@@@@@@@@@


        //ChatServer chatServer = new ChatServer();
        Console.WriteLine("Welcome to the Decentralized CLI Chat App v.1");
        Console.WriteLine("There are three modes to run the program in.\n");
        Console.WriteLine("[1]: Asyncronous Decentralized Server (not encrypted, not TSL, insecure)\n[2]: Decentralized Server with client authentication (not encrypted, not TSL, less secure)\n[3]: P2P (TSL Encrypted)");

        string userInput = Console.ReadLine() ?? "";

        switch (userInput)
        {
            case "1":
                await RunServerAsync();
                break;
            case "2":
                RunServerWithClientValidation();
                break;
            case "3":
                RunP2P();
                break;
            default:
                Console.WriteLine("Invalid option selected!\n Exiting program...");
                Environment.Exit(1);
                break;
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
    /// <summary>
    /// Run the CreateKeyPair.py script to create a new keypair.pem file.
    /// </summary>
    static void RunP2P()
    {
        using (Peer peer = new Peer())
        {
            peer.StartListening(30303); // start a new P2P connection
            peer.ConnectToPeer("127.0.0.1", 30303, "Hello P2P!"); // connect to peer(s)
        }
        Console.WriteLine("P2P connection terminated...");
    }

    /*
    /// <summary>
    /// Tests, uncomment if you wish to run/write them
    /// </summary>
    static void Tests()
    {
        TestSSLAuthentication testSSLAuthentication = new TestSSLAuthentication();
        testSSLAuthentication.CheckOutput();
        testSSLAuthentication.CaptureSSLInformation();
    }
    */
}
