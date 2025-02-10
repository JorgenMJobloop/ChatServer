using System.Net.Sockets;

public class ClientValidationAndHandling
{
    public TcpClient tcpClient = new TcpClient();
    /// <summary>
    /// Validate whether a client is connected to our server or not, if no client is connected, we call a helper method that shuts down the server.
    /// if the TcpClient.Connected state is true, we return a valid connection state
    /// else, default to false
    /// </summary>
    /// <returns>false</returns>
    public bool Validate(TcpClient client) => client.Connected;
}