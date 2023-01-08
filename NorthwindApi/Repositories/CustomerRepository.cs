using Microsoft.EntityFrameworkCore.ChangeTracking; // EntityEntry<T>
using Packt.Shared; // Customer
using System.Collections.Concurrent; // ConcurrentDictionary

namespace NorthwindApi.Repositories;

public class CustomerRepository : ICustomerRepository
{
    // использование статического потокобезопасного словарного
    // поля для кэширования клиентов.
    private static ConcurrentDictionary
    <string, Customer>? customersCache;

    // использование поля экземпляра класса для контекста,
    // поскольку он не должен кэшироваться из-за своего
    // внутреннего кэширования
    private Northwind db;

    public CustomerRepository(Northwind injectedContext)
  {
    db = injectedContext;

    // предварительная загрузка клиентов из базы данных в обычный
    // словарь с CustomerID в качестве ключа, а затем
    // преобразование в потокобезопасный ConcurrentDictionary
    if (customersCache is null)
    {
      customersCache = new ConcurrentDictionary<string, Customer>(
        db.Customers.ToDictionary(c => c.CustomerId));
    }
  }

  public async Task<Customer?> CreateAsync(Customer c)
  {
    // нормализация CustomerID в верхнем регистре
    c.CustomerId = c.CustomerId.ToUpper();
    // добавление в базу данных с помощью EF Core
    EntityEntry<Customer> added = await db.Customers.AddAsync(c);
    int affected = await db.SaveChangesAsync();
    if (affected == 1)
    {
      if (customersCache is null) return c;
      // если клиент новый, то добавление его в кэш, иначе
        // произойдет вызов метода UpdateCache
      return customersCache.AddOrUpdate(c.CustomerId, c, UpdateCache);
    }
    else
    {
      return null;
    }
  }

  public Task<IEnumerable<Customer>> RetrieveAllAsync()
  {
    // For performance, get from cache.
    return Task.FromResult(
        customersCache is null
        ? Enumerable.Empty<Customer>() 
        : customersCache.Values);
  }

  public Task<Customer?> RetrieveAsync(string id)
  {
    // For performance, get from cache.
    id = id.ToUpper();
    if (customersCache is null) return null!;
    customersCache.TryGetValue(id, out Customer? c);
    return Task.FromResult(c);
  }

  private Customer UpdateCache(string id, Customer c)
  {
    Customer? old;
    if (customersCache is not null)
    {
      if (customersCache.TryGetValue(id, out old))
      {
        if (customersCache.TryUpdate(id, c, old))
        {
          return c;
        }
      }
    }
    return null!;
  }

  public async Task<Customer?> UpdateAsync(string id, Customer c)
  {
    // Normalize customer Id.
    id = id.ToUpper();
    c.CustomerId = c.CustomerId.ToUpper();
    // Update in database.
    db.Customers.Update(c);
    int affected = await db.SaveChangesAsync();
    if (affected == 1)
    {
      // Update in cache.
      return UpdateCache(id, c);
    }
    return null;
  }

  public async Task<bool?> DeleteAsync(string id)
  {
    id = id.ToUpper();
    // Remove from database.
    Customer? c = db.Customers.Find(id);
    if (c is null) return null;
    db.Customers.Remove(c);
    int affected = await db.SaveChangesAsync();
    if (affected == 1)
    {
      if (customersCache is null) return null;
      // Remove from cache.
      return customersCache.TryRemove(id, out c);
    }
    else
    {
      return null;
    }
  }
}