using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.DataAccess
{
    public abstract class UpdatableEntityMapper<E> : EntityMapper<E> where E : Entity
    {
        protected List<E> insertedEntities = new List<E>();
        protected Dictionary<int, E> updatedEntities = new Dictionary<int, E>();
        protected bool updateBatching = false;
        protected int autoIncrement;

        public UpdatableEntityMapper(MySqlConnection connection, SelectQuery baseSelectQuery)
            : base(connection, baseSelectQuery)
        { }

        public void BeginUpdate()
        {
            if (updateBatching) throw new InvalidOperationException("Can't begin a new batch before ending the active one.");
            updateBatching = true;
        }

        public void EndUpdate()
        {
            if (!updateBatching) throw new InvalidOperationException("Can't end a batch before starting it.");
            updateBatch();
            foreach (E entity in updatedEntities.Values)
            {
                if (!entityCache.TryAdd(entity.ID, entity))
                {
                    updateCachedEntry(entityCache[entity.ID], entity);
                }
            }
            updateBatching = false;
        }

        public void UpdateSaveCache()
        {
            foreach (E entity in entityCache.Values)
            {
                Update(entity);
            }
        }

        public void Update(E entity)
        {
            if (updateBatching)
            {
                if (entity.ID == -1) insertedEntities.Add(entity);
                else if (updatedEntities.ContainsKey(entity.ID)) updatedEntities[entity.ID] = entity;
                else updatedEntities.Add(entity.ID, entity);
            }
            else
            {
                updateSingle(entity);
                if (entity.ID != -1 && !entityCache.TryAdd(entity.ID, entity))
                {
                    updateCachedEntry(entityCache[entity.ID], entity);
                }
            }
        }

        protected abstract void updateSingle(E entity);
        protected abstract void updateBatch();
        protected abstract void updateCachedEntry(E cachedEntity, E updatedEntity);
    }
}
