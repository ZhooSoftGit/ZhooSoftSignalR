﻿using System.Collections.Concurrent;

namespace ZhooSoft.Tracker.Services
{
    public static class ConnectionMapping
    {
        private static readonly ConcurrentDictionary<int, string> _map = new();

        public static void Add(int userId, string connectionId) => _map[userId] = connectionId;

        public static void Remove(int userId) => _map.TryRemove(userId, out _);

        public static string? GetConnection(int userId) =>
            _map.TryGetValue(userId, out var connId) ? connId : null;
    }
}
