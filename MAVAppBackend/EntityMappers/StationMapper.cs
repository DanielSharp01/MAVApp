using MAVAppBackend.DataAccess;
using MAVAppBackend.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.EntityMappers
{
    public class StationMapper : EntityMapper<Station>
    {
        public StationMapper(MySqlConnection connection)
            : base(connection, QueryBuilder.SelectEveryColumn("stations").Where("mav_found = 1"))
        {
            EntityCacheForName  = new ReadOnlyDictionary<string, Station>(entityCacheForName);
        }

        protected Dictionary<string, Station> entityCacheForName = new Dictionary<string, Station>();

        public IReadOnlyDictionary<string, Station> EntityCacheForName { get; private set; }

        protected override Station createEntity(int id)
        {
            return new Station(id);
        }

        private Station createEntity(string normName)
        {
            Station entity;
            if (entityCacheForName.ContainsKey(normName))
            {
                entity = entityCacheForName[normName];
            }
            else
            {
                entity = new Station(normName);
                entityCacheForName.Add(normName, entity);
            }
            return entity;
        }

        protected override bool fillEntity(Station entity, MySqlDataReader reader)
        {
            if (entity.ID != -1)
            {
                string name = reader.GetStringOrNull("name");
                string normName = reader.GetStringOrNull("norm_name");
                Vector2 gpsCoord = reader.GetVector2OrNull("lat", "lon");
                entity.Fill(name, normName, gpsCoord);
            }
            else
            {
                int id = reader.GetInt32("id");
                string name = reader.GetStringOrNull("name");
                Vector2 gpsCoord = reader.GetVector2OrNull("lat", "lon");
                entity.Fill(id, name, gpsCoord);
            }

            return reader.Read();
        }

        protected BatchSelectStrategy<string, Station> sbatchStrategyNormName = null;

        public void BeginSelectNormName(BatchSelectStrategy<string, Station> sbatchStrategy)
        {
            if (sbatchStrategyNormName != null) throw new InvalidOperationException("Can't begin a new batch before ending the active one.");
            sbatchStrategyNormName = sbatchStrategy ?? throw new ArgumentNullException("sbatchStrategy");
        }

        public Station GetByName(string name, bool forceUpdate = true)
        {
            string normName = NormalizeName(name);
            bool hasCache = entityCacheForName.ContainsKey(normName);
            Station entity = createEntity(normName);
            if (!hasCache || forceUpdate) FillByName(entity);
            return entity;
        }

        public void FillByName(Station entity)
        {
            if (sbatchStrategyNormName == null)
                fillByNormNameSingle(entity);
            else
                sbatchStrategyNormName.AddEntity(entity.NormalizedName, entity);
        }

        public void EndSelectNormName()
        {
            if (sbatchStrategyNormName == null) throw new InvalidOperationException("Can't end a batch before starting it.");
            sbatchStrategyNormName.BatchFill(connection, baseSelectQuery, "norm_name", fillEntity);
            sbatchStrategyNormName = null;
        }

        MySqlCommand getByNormNameCmd = null;
        protected virtual void fillByNormNameSingle(Station entity)
        {
            if (getByNormNameCmd == null)
                getByNormNameCmd = baseSelectQuery.Where($"norm_name = @norm_name").ToPreparedCommand(connection);

            getByNormNameCmd.Parameters.Clear();
            getByNormNameCmd.Parameters.AddWithValue("@norm_name", entity.NormalizedName);
            MySqlDataReader reader = getByNormNameCmd.ExecuteReader();
            if (reader.Read())
            {
                fillEntity(entity, reader);
            }

            reader.Close();
        }

        /// <summary>
        /// Normalizes a station name for comparison (removes Hungarian accents, replaces hyphens with spaces, removes redundant information such as station)
        /// </summary>
        /// <param name="stationName">Name to normalize</param>
        /// <returns>Normalized version of the same name</returns>
        public static string NormalizeName(string stationName)
        {
            stationName = stationName.ToLower();

            stationName = stationName.Replace('á', 'a');
            stationName = stationName.Replace('é', 'e');
            stationName = stationName.Replace('í', 'i');
            stationName = stationName.Replace('ó', 'o');
            stationName = stationName.Replace('ö', 'o');
            stationName = stationName.Replace('ő', 'o');
            stationName = stationName.Replace('ú', 'u');
            stationName = stationName.Replace('ü', 'u');
            stationName = stationName.Replace('ű', 'u');

            stationName = stationName.Replace(" railway station crossing", "");
            stationName = stationName.Replace(" railway station", "");
            stationName = stationName.Replace(" train station", "");
            stationName = stationName.Replace(" station", "");
            stationName = stationName.Replace(" vonatallomas", "");
            stationName = stationName.Replace(" vasutallomas", "");
            stationName = stationName.Replace(" mav pu", "");
            stationName = stationName.Replace("-", " ");

            return stationName;
        }
    }
}
