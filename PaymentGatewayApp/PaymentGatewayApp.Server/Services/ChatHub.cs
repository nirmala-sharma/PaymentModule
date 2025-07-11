using Microsoft.AspNetCore.SignalR;
public class ChatHub : Hub
{
    /// <summary>
    /// Called by clients to send a chat message.
    /// The server will broadcast this message to all connected clients.
    /// </summary>
    /// <param name="user">The name of the user sending the message.</param>
    /// <param name="message">The actual chat message.</param>
    public async Task SendMessage(string user, string message)
    {
    }
}
