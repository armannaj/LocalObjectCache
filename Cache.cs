using System.Linq.Expressions;
using System.Reflection;
using LiteDB;
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace LocalObjectCache;

public class Cache : IDisposable
{
    private readonly string _fileName;
    private readonly TimeSpan _cacheValidity;
    private readonly LiteDatabase _db;

    public Cache(string fileName, TimeSpan cacheValidity)
    {
        _fileName = fileName;
        _cacheValidity = cacheValidity;
        _db = new LiteDatabase(_fileName);
        _db.Checkpoint();
        _db.Rebuild();
    }

    public void Dispose()
    {
        if (_db != null)
        {
            _db.Dispose();
        }
    }

    #region Default Static

    /// <summary>
    /// local object cache with default settings
    /// </summary>
    public static readonly Cache Default = new("cache.db", TimeSpan.FromDays(1));
   
    #endregion
    
    private ILiteCollection<T> GetCollection<T>()
    {
        var type = typeof(T);
        var collectionName = Pluralize(type.Name);
        var collection = _db.GetCollection<T>(collectionName);
        foreach (var propertyInfo in type.GetProperties())
        {
            var attr = propertyInfo.GetCustomAttribute<IndexAttribute>();

            if (attr != null)
            {
                collection.EnsureIndex(propertyInfo.Name, attr.IsUnique);
            }
        }

        return collection;
    }
    private string Pluralize(string singleName)
    {
        if (singleName.EndsWith('y'))
        {
            return singleName.Substring(0, singleName.Length - 1) + "ies";
        }

        return singleName + "s";
    }

    public T GetOne<T>(Expression<Func<T, bool>> predicate)
    {
        var collection = GetCollection<T>();
        var documents = collection.Query().Where(predicate).ToDocuments();
        var document = documents.FirstOrDefault();
        return collection.EnsureDocumentValidity<T>(document, _cacheValidity);
    }

    public List<T> GetMany<T>(Expression<Func<T, bool>> predicate)
    {
        var collection = GetCollection<T>();
        var documents = collection.Query().Where(predicate).ToDocuments();
        return documents.Select(d => collection.EnsureDocumentValidity<T>(d, _cacheValidity)).ToList();
    }

    public void InsertOne<T>(T item)
    {
        var collection = GetCollection<T>();
        collection.Insert(item);
    }

    public void InsertMany<T>(IEnumerable<T> items)
    {
        var collection = GetCollection<T>();
        collection.InsertBulk(items);
    }
}