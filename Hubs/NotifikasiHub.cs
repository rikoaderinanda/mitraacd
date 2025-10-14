using Microsoft.AspNetCore.SignalR;
using mitraacd.Models;
using System.Collections.Concurrent;

namespace mitraacd.Hubs
{
    public class NotifikasiHub : Hub
    {
        public static ConcurrentDictionary<string, HashSet<string>> UserConnections = new();

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                UserConnections.AddOrUpdate(
                    userId,
                    _ => new HashSet<string> { Context.ConnectionId },
                    (_, set) =>
                    {
                        set.Add(Context.ConnectionId);
                        return set;
                    });
            }

            Console.WriteLine($"🔗 Connected: {Context.ConnectionId}, User: {userId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId) && UserConnections.TryGetValue(userId, out var set))
            {
                set.Remove(Context.ConnectionId);
                if (set.Count == 0)
                {
                    UserConnections.TryRemove(userId, out _);
                }
            }

            Console.WriteLine($"❌ Disconnected: {Context.ConnectionId}, User: {userId}");
            await base.OnDisconnectedAsync(exception);
        }

    }
}
