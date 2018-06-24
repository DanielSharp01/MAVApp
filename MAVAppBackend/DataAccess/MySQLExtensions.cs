using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MAVAppBackend.DataAccess
{
    public static class MySQLExtensions
    {
        /// <summary>
        /// Gets the value of a specified column as a string object, allowing null.
        /// </summary>
        /// <param name="columnName">The column name</param>
        public static string GetStringOrNull(this MySqlDataReader reader, string columnName)
        {
            return reader.IsDBNull(reader.GetOrdinal(columnName)) ? null : reader.GetString(columnName);
        }

        /// <summary>
        /// Gets the value of a specified column as a string object. When null the default value is used instead.
        /// </summary>
        /// <param name="columnName">The column name</param>
        /// <param name="default">Default string to return if column is null</param>
        /// <returns>String at columnName if not null, default otherwise</returns>
        public static string GetStringOrDefault(this MySqlDataReader reader, string columnName, string @default)
        {
            return reader.IsDBNull(reader.GetOrdinal(columnName)) ? @default : reader.GetString(columnName);
        }

        /// <summary>
        /// Gets the value of a specified column as an integer. When null the default value is used instead.
        /// </summary>
        /// <param name="columnName">The column name</param>
        /// <param name="default">Default integer to return if column is null</param>
        /// <returns>Integer at columnName if not null, default otherwise</returns>
        public static int GetInt32OrDefault(this MySqlDataReader reader, string columnName, int @default)
        {
            return reader.IsDBNull(reader.GetOrdinal(columnName)) ? @default : reader.GetInt32(columnName);
        }

        /// <summary>
        /// Gets the value of 2 double columns converted into a Vector2 object.
        /// </summary>
        /// <param name="xCoordName">Column name of the .X coordinate</param>
        /// <param name="yCoordName">Column name of the .Y coordinate</param>
        /// <returns></returns>
        public static Vector2 GetVector2OrNull(this MySqlDataReader reader, string xCoordName, string yCoordName)
        {
            if (reader.IsDBNull(reader.GetOrdinal(xCoordName)) || reader.IsDBNull(reader.GetOrdinal(yCoordName))) return null;
            else return new Vector2(reader.GetDouble(xCoordName), reader.GetDouble(yCoordName));
        }

        /// <summary>
        /// Gets a value indicating whether the MySQLDataReader contains one or more rows. If it contains no rows it also closes the connection.
        /// </summary>
        /// <returns>True if the MySQLDataReader contains one or more rows, false otherwise</returns>
        public static bool HasRowsOrClose(this MySqlDataReader reader)
        {
            if (reader.HasRows) return true;
            reader.Close();
            return false;
        }

        /// <summary>
        /// Adds an array of values in place of @arrayName_{i} parameters in an SQL query.
        /// </summary> 
        /// <param name="arrayName">Parameter name of the array</param>
        /// <param name="value">Value to add</param>
        public static void AddArrayWithValue(this MySqlParameterCollection parameters, string arrayName, int[] value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                parameters.AddWithValue(arrayName + $"_{i}", value[i]);
            }
        }

        /// <summary>
        /// Adds an array of values in place of @arrayName_{i} parameters in an SQL query.
        /// </summary> 
        /// <param name="arrayName">Parameter name of the array</param>
        /// <param name="value">Value to add</param>
        public static void AddArrayWithValue(this MySqlParameterCollection parameters, string arrayName, string[] value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                parameters.AddWithValue(arrayName + $"_{i}", value[i]);
            }
        }

        /// <summary>
        /// Adds two double parameters with the value of a single Vector2.
        /// </summary> 
        /// <param name="xCoordName">Parameter name of the .X coordinate</param>
        /// <param name="yCoordName">Parameter name of the .Y coordinate</param>
        /// <param name="value">Value to add</param>
        public static void AddVector2WithValue(this MySqlParameterCollection parameters, string xCoordName, string yCoordName, Vector2 value)
        {
            if (value == null)
            {
                parameters.AddWithValue(xCoordName, null);
                parameters.AddWithValue(yCoordName, null);
            }
            else
            {
                parameters.AddWithValue(xCoordName, value.X);
                parameters.AddWithValue(yCoordName, value.Y);
            }
        }
    }
}
