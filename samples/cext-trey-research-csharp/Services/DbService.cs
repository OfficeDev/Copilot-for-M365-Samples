using Azure;
using Azure.Data.Tables;
using Trey.Research.Declarative.Agent.Models;
using Trey.Research.Declarative.Agent.Utilities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Trey.Research.Declarative.Agent.Services
{
    public class DbService<DbEntityType> where DbEntityType : class, ITableEntity, new()
    {
        private readonly string _storageAccountConnectionString;
        private readonly bool _okToCacheLocally;
        private List<DbEntity> _entityCache = new List<DbEntity>();

        // Updated constructor to accept the storage connection string
        public DbService(IConfiguration configuration, bool okToCacheLocally)
        {
            // Retrieve the AzureWebJobsStorage from configuration
            _storageAccountConnectionString = configuration["AzureWebJobsStorage"]
                ?? throw new InvalidOperationException("AzureWebJobsStorage is not set in the configuration.");

            _okToCacheLocally = okToCacheLocally;
        }

        public async Task<DbEntity> GetEntityByRowKeyAsync(string tableName, string rowKey)
        {
            if (!_okToCacheLocally)
            {
                var tableClient = new TableClient(_storageAccountConnectionString, tableName);
                var result = await tableClient.GetEntityAsync<DbEntityType>(tableName, rowKey);
                var r = result as DbEntity;
                return r;
            }
            else
            {
                var result = await GetEntitiesAsync(tableName);
                var entity = result.Find(e => e.RowKey == rowKey);
                if (entity == null)
                {
                    throw new Utilities.HttpError(404, $"Entity {rowKey} not found");
                }
                return entity;
            }
        }

        public async Task<List<DbEntity>> GetEntitiesAsync(string tableName)
        {
            if (!_okToCacheLocally || _entityCache.Count == 0)
            {
                // Rebuild cache for this entity
                var tableClient = new TableClient(_storageAccountConnectionString, tableName);
                var entities = tableClient.QueryAsync<DbEntityType>();

                _entityCache.Clear();
                await foreach (var entity in entities)
                {
                    if (_entityCache.Find(e => e.RowKey == entity.RowKey) == null)
                    {
                        _entityCache.Add(entity as DbEntity);
                    }
                }
            }
            return _entityCache;
        }

        public async Task CreateEntityAsync(string tableName, string rowKey, DbEntityType newEntity)
        {
            var entity = newEntity;
            var tableClient = new TableClient(_storageAccountConnectionString, tableName);
            var tableEntity = new TableEntity(tableName, rowKey);

            foreach (var prop in entity.GetType().GetProperties())
            {
                if(prop.Name != "Forecast" && prop.Name != "Delivered" && prop.Name != "ETag")
                tableEntity.Add(prop.Name, prop.GetValue(entity));
            }

            await tableClient.AddEntityAsync(tableEntity);
        }

        public async Task UpdateEntityAsync(string tableName, DbEntityType updatedEntity)
        {
            var tableClient = new TableClient(_storageAccountConnectionString, tableName);
            var tableEntity = new TableEntity(tableName, updatedEntity.RowKey);

            foreach (var prop in updatedEntity.GetType().GetProperties())
            {
                if (prop.Name != "Forecast" && prop.Name != "Delivered" && prop.Name != "ETag")
                    tableEntity.Add(prop.Name, prop.GetValue(updatedEntity));
            }
            await tableClient.UpdateEntityAsync(tableEntity, ETag.All, TableUpdateMode.Replace);
        }
    }
}