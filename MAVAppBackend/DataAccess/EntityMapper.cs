using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.DataAccess
{
    public abstract class EntityMapper<E> where E : Entity
    {
        protected MySqlConnection connection;
        protected SelectQuery baseSelectQuery;
        protected string idColumn = "id";

        public EntityMapper(MySqlConnection connection, SelectQuery baseSelectQuery)
        {
            this.connection = connection;
            this.baseSelectQuery = baseSelectQuery;
        }
        
        protected BatchSelectStrategy<int, E> sbatchStrategy = null;

        public virtual void BeginSelect(BatchSelectStrategy<int, E> sbatchStrategy)
        {
            if (this.sbatchStrategy != null) throw new InvalidOperationException("Can't begin a new batch before ending the active one.");
            this.sbatchStrategy = sbatchStrategy ?? throw new ArgumentNullException("sbatchStrategy");
        }

        protected E createEntityInternal(int id)
        {
            E entity;
            if (entityCache.ContainsKey(id))
            {
                entity = entityCache[id];
            }
            else
            {
                entity = createEntity(id);
                entityCache.Add(id, entity);
            }
            return entity;
        }

        public virtual E GetByID(int id, bool forceUpdate = true)
        {
            bool hasCache = entityCache.ContainsKey(id);
            E entity = createEntityInternal(id);
            if (!hasCache || forceUpdate) Fill(entity);
            return entity;
        }

        public virtual void Fill(E entity)
        {
            if (sbatchStrategy == null)
                fillByIDSingle(entity);
            else
                sbatchStrategy.AddEntity(entity.ID, entity);
        }

        protected Dictionary<int, E> entityCache = new Dictionary<int, E>();

        public IReadOnlyDictionary<int, E> EntityCache { get => new ReadOnlyDictionary<int, E>(entityCache); }

        protected abstract E createEntity(int id);
        protected abstract bool fillEntity(E entity, MySqlDataReader reader);

        public virtual void EndSelect()
        {
            if (sbatchStrategy == null) throw new InvalidOperationException("Can't end a batch before starting it.");
            sbatchStrategy.BatchFill(connection, baseSelectQuery, idColumn, fillEntity);
            sbatchStrategy = null;
        }

        protected MySqlCommand getAllCmd = null;
        public virtual List<E> GetAll()
        {
            List<E> entities = new List<E>();

            if (getAllCmd == null)
                getAllCmd = baseSelectQuery.ToCommand(connection);
            
            MySqlDataReader reader = getAllCmd.ExecuteReader();
            if (reader.Read())
            {
                E entity = createEntityInternal(reader.GetInt32(idColumn));
                while (fillEntity(entity, reader))
                {
                    entities.Add(entity);
                    entity = createEntityInternal(reader.GetInt32(idColumn));
                }
                entities.Add(entity);
            }

            reader.Close();

            return entities;
        }

        protected MySqlCommand getByIdCmd = null;
        protected virtual void fillByIDSingle(E entity)
        {
            if (getByIdCmd == null)
                getByIdCmd = baseSelectQuery.Where($"{idColumn} = @id").ToPreparedCommand(connection);

            getByIdCmd.Parameters.Clear();
            getByIdCmd.Parameters.AddWithValue("@id", entity.ID);
            MySqlDataReader reader = getByIdCmd.ExecuteReader();
            if (reader.Read())
            {
                fillEntity(entity, reader);
            }

            reader.Close();
        }
    }
}
