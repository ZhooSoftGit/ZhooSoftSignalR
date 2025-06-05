using System.Collections.Concurrent;

namespace ZhooSoft.Tracker.Services
{
    public static class ConnectionMapping
    {
        private static readonly ConcurrentDictionary<string, string> _map = new();

        public static void Add(string userId, string connectionId) => _map[userId] = connectionId;

        public static void Remove(string userId) => _map.TryRemove(userId, out _);

        public static string? GetConnection(string userId) =>
            _map.TryGetValue(userId, out var connId) ? connId : null;
    }
}
