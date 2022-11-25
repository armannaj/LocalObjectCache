using LiteDB;

namespace LocalObjectCache;

internal static class CacheExtensions
{
    internal static T? EnsureDocumentValidity<T>(this ILiteCollection<T> collection, BsonDocument? document, TimeSpan evictionTime)
    {
        if (document == null)
        {
            return default;
        }

        var documentId = document["_id"];
        var timespan = DateTime.Now - documentId.AsObjectId.CreationTime;
        if (timespan > evictionTime)
        {
            collection.Delete(documentId);
            return default;
        }

        return BsonMapper.Global.ToObject<T>(document);
    }
}