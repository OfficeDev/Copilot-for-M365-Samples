using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using NorthwindInventory.Models;

namespace NorthwindInventory.NorthwindDB
{
    public interface IDbEntity : ITableEntity
    {
        string ETag { get; set; }
        string? PartitionKey { get; set; }
        string? RowKey { get; set; }
        DateTimeOffset? Timestamp { get; set; }
    }

    public interface IDbProject : IDbEntity, IProduct { }
    public interface IDbSupplier : IDbEntity, ISupplier { }

    public class DbService<TEntity> where TEntity : class, IDbEntity, new()
    {
        private readonly bool _okToCacheLocally;
        private List<TEntity> _entityCache = new List<TEntity>();
        private IConfiguration _configuration;

        public DbService(bool okToCacheLocally, IConfiguration configuration)
        {
            _configuration = configuration;
            if (string.IsNullOrEmpty(_configuration["StorageConnectionString"]))
            {
                throw new InvalidOperationException("STORAGE_ACCOUNT_CONNECTION_STRING is not set");
            }
            _okToCacheLocally = okToCacheLocally;
        }

        public async Task<TEntity> GetEntityByRowKeyAsync(string tableName, string rowKey)
        {
            if (!_okToCacheLocally)
            {
                var tableClient = new TableClient(_configuration["StorageConnectionString"], tableName);

                // Ensure you provide the partitionKey
                var partitionKey = "PartitionKey"; // Replace with actual partitionKey or logic to determine it.

                var result = await tableClient.GetEntityAsync<TEntity>(partitionKey, rowKey);
                return ExpandPropertyValues(result);
            }
            else
            {
                var result = await GetEntitiesAsync(tableName);
                var entity = result.FirstOrDefault(e => e.RowKey == rowKey);
                if (entity == null)
                {
                    Console.WriteLine("Not found");
                    return null;
                }
                return entity;
            }
        }

        public async Task<List<TEntity>> GetEntitiesAsync(string tableName)
        {
            if (_okToCacheLocally && _entityCache.Any())
            {
                return _entityCache;
            }
            else
            {
                var tableClient = new TableClient(_configuration["StorageConnectionString"], tableName);
                var entities = new List<TEntity>();

                // Use QueryAsync to get entities
                await foreach (var entity in tableClient.QueryAsync<TEntity>())
                {
                    entities.Add(ExpandPropertyValues(entity));
                }

                _entityCache = entities;
                return _entityCache;
            }
        }

        public async Task CreateEntityAsync(string tableName, string rowKey, TEntity newEntity)
        {
            var tableClient = new TableClient(_configuration["StorageConnectionString"], tableName);
            await tableClient.UpsertEntityAsync(newEntity);            
        }

        public async Task UpdateEntityAsync(TEntity updatedEntity)
        {
            var tableClient = new TableClient(_configuration["StorageConnectionString"], updatedEntity.PartitionKey);
            var etag = new ETag(updatedEntity.ETag);
            await tableClient.UpdateEntityAsync(updatedEntity, etag, TableUpdateMode.Replace);
        }

        public async Task<int> GetNextIdAsync(string tableName)
        {
            try
            {
                var entities = await GetEntitiesAsync(tableName);
                return entities.Count + 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching entities: {ex.Message}");
                throw;
            }
        }

        private TEntity ExpandPropertyValues(TEntity entity)
        {
            var result = new TEntity();
            var properties = typeof(TEntity).GetProperties();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(entity);
                prop.SetValue(result, ExpandPropertyValue(value));
            }
            return result;
        }

        private object ExpandPropertyValue(object value)
        {
            if (value is string str && (str.StartsWith('{') || str.StartsWith('[')))
            {
                try
                {
                    return JsonSerializer.Deserialize<object>(str);
                }
                catch
                {
                    return value;
                }
            }
            return value;
        }

        private TEntity CompressPropertyValues(TEntity entity)
        {
            var result = new TEntity();
            var properties = typeof(TEntity).GetProperties();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(entity);
                prop.SetValue(result, CompressPropertyValue(value));
            }
            return result;
        }

        private object CompressPropertyValue(object value)
        {
            if (value is object obj)
            {
                return JsonSerializer.Serialize(obj);
            }
            return value;
        }
    }
}
