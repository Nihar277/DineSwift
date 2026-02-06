using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace API.Services
{
    public record RedisOptions
    {
        public string Configuration { get; init; } = "localhost:6379";
        public string InstanceName { get; init; } = "MVC:";
    }

    public interface IChatService
    {
        void EnsureUser(string username);
        void SendToUser(string fromUser, string toUser, string text);
        string? ReceiveOneForUser(string username);
        IReadOnlyList<string> GetHistory(string userA, string userB, int take = 50);
        IReadOnlyList<string> GetAllNotificationHistory(string username);
        bool DeleteNotification(string username, string message);
        IReadOnlyList<string> PeekQueueMessages(string username);
        bool DeleteFromConversation(string userA, string userB, string message);

    }

    public class RedisChatService : IChatService
    {
        private readonly RedisOptions _options;
        private readonly IConnectionMultiplexer _muxer;
        private readonly IDatabase _db;

        public RedisChatService(IOptions<RedisOptions> options, IConnectionMultiplexer muxer)
        {
            _options = options.Value;
            _muxer = muxer;
            _db = _muxer.GetDatabase();
        }

        private string GetQueueKey(string username) => _options.InstanceName + "queue:" + username.ToLowerInvariant();
        private string GetHistoryKey(string username) => _options.InstanceName + "history:" + username.ToLowerInvariant();
        private string GetConversationKey(string userA, string userB)
        {
            var a = userA.ToLowerInvariant();
            var b = userB.ToLowerInvariant();
            if (string.CompareOrdinal(a, b) <= 0)
            {
                return _options.InstanceName + "conv:" + a + ":" + b;
            }
            return _options.InstanceName + "conv:" + b + ":" + a;
        }

        public void EnsureUser(string username)
        {

        }

        public void SendToUser(string fromUser, string toUser, string text)
        {
            var payload = $"{fromUser}|{DateTime.UtcNow:o}|{text}";
            _db.ListRightPush(GetQueueKey(toUser), payload);
            _db.ListRightPush(GetConversationKey(fromUser, toUser), payload);
            // Also store in history for notification history feature
            _db.ListRightPush(GetHistoryKey(toUser), payload);
        }

        public string? ReceiveOneForUser(string username)
        {
            var value = _db.ListLeftPop(GetQueueKey(username));
            return value.IsNull ? null : value.ToString();
        }

        public IReadOnlyList<string> GetHistory(string userA, string userB, int take = 50)
        {
            var key = GetConversationKey(userA, userB);
            var len = _db.ListLength(key);
            if (len == 0) return Array.Empty<string>();
            var start = Math.Max(0, (int)len - take);
            var values = _db.ListRange(key, start, -1);
            return values.Select(v => v.ToString()).Where(s => s != null).Select(s => s!).ToArray();
        }

        public IReadOnlyList<string> GetAllNotificationHistory(string username)
        {
            var key = GetHistoryKey(username);
            var len = _db.ListLength(key);
            if (len == 0) return Array.Empty<string>();
            // Get all notifications from the beginning
            var values = _db.ListRange(key, 0, -1);
            return values.Select(v => v.ToString()).Where(s => s != null).Select(s => s!).Reverse().ToArray();
        }

        public bool DeleteNotification(string username, string message)
        {
            // Remove the specific message from both history and queue (if present)
            var historyKey = GetHistoryKey(username);
            var queueKey = GetQueueKey(username);

            // LREM count = 0 removes all occurrences; keep it to 0 to ensure full cleanup
            var removedFromHistory = _db.ListRemove(historyKey, message, 0) > 0;
            var removedFromQueue = _db.ListRemove(queueKey, message, 0) > 0;

            return removedFromHistory || removedFromQueue;
        }
        // this is resturant delete
        //conque delete from conversation
        public bool DeleteFromConversation(string userA, string userB, string message)
        {
            var key = GetConversationKey(userA, userB);

            // Remove message from conversation
            return _db.ListRemove(key, message, 0) > 0;
        }

        public IReadOnlyList<string> PeekQueueMessages(string username)
        {
            var key = GetQueueKey(username);
            var len = _db.ListLength(key);
            if (len == 0) return Array.Empty<string>();
            // Peek at all queue messages without removing them
            var values = _db.ListRange(key, 0, -1);
            return values.Select(v => v.ToString()).Where(s => s != null).Select(s => s!).ToArray();
        }



    }
}


