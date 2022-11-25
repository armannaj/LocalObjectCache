# LocalObjectCache
An object cache to store objects temporarily & locally for .NET applications.

## Required
- [dotnet 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) or [dotnet 7](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)

## Usages
- To store the HTTP response back, in order to not hit an API endpoint too much (avoiding HTTP 429 status code)

## How does it work?
Internally, it uses [LiteDB](https://github.com/mbdavid/litedb) to store objects in a local file.

## How to use
For a quick start, you can use the static `LocalObjectCache.Default`. It creates a cache with the following default settings:
* Object validity timespan: _24hours from the time of object insertion_
* DB filename: _cache.db_
* DB file path: _local executing directory_

Alternatively, you can instantiate `LocalObjectCache` and configure it the way you want it and use it.

You can use `Index` attribute on the properties that you want to use as ID or you want to enforce uniqueness in the cache.

The following methods help you to interact with the cache:
### InsertOne\<T\>  
Inserts an object of type T into the cache. The cache validity starts from the time of insertion. 
``` csharp
InsertOne<ExchangeRate>(new ExchangeRate("USD", "EUR", 0.96))
```

### GetOne\<T\>
Gets an object from the cache with the given predicate (in lambda format)
``` csharp
GetOne<ExchangeRate>(x => x.BaseCurrency.Equals("USD")) 
```

### InsertMany(IEnumerable\<T\>)
```csharp
InsertMany<ExchangeRate>(new [] { new ExchangeRate("USD", "EUR", 0.96),  new ExchangeRate("USD", "AUD", 1.50)})
```

### GetMany
```csharp
GetMany<ExchangeRate>(x => x.BaseCurrency.Equals("USD"))
```
