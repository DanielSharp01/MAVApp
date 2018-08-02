using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharpEntities
{
    public static class DataReaderExtensions
    {
        /// <summary>Gets the value of the specified column as a Boolean.</summary>
        /// <param name="columnName">The column name</param>
        /// <returns>The value of the specified column.</returns>
        /// <exception cref="T:System.InvalidCastException">The specified cast is not valid.</exception>
        public static bool GetBoolean(this DbDataReader reader, string columnName)
        {
            return reader.GetBoolean(reader.GetOrdinal(columnName));
        }

        /// <summary>Gets the value of the specified column as a byte.</summary>
        /// <param name="columnName">The column name</param>
        /// <returns>The value of the specified column.</returns>
        /// <exception cref="T:System.InvalidCastException">The specified cast is not valid.</exception>
        public static byte GetByte(this DbDataReader reader, string columnName)
        {
            return reader.GetByte(reader.GetOrdinal(columnName));
        }

        /// <summary>Reads a stream of bytes from the specified column, starting at location indicated by <paramref name="dataOffset">dataOffset</paramref>, into the buffer, starting at the location indicated by <paramref name="bufferOffset">bufferOffset</paramref>.</summary>
        /// <param name="columnName">The column name</param>
        /// <param name="dataOffset">The index within the row from which to begin the read operation.</param>
        /// <param name="buffer">The buffer into which to copy the data.</param>
        /// <param name="bufferOffset">The index with the buffer to which the data will be copied.</param>
        /// <param name="length">The maximum number of characters to read.</param>
        /// <returns>The actual number of bytes read.</returns>
        /// <exception cref="T:System.InvalidCastException">The specified cast is not valid.</exception>
        public static long GetBytes(this DbDataReader reader, string columnName, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            return reader.GetBytes(reader.GetOrdinal(columnName), dataOffset, buffer, bufferOffset, length);
        }

        /// <summary>Gets the value of the specified column as a single character.</summary>
        /// <param name="columnName">The column name</param>
        /// <returns>The value of the specified column.</returns>
        /// <exception cref="T:System.InvalidCastException">The specified cast is not valid.</exception>
        public static char GetChar(this DbDataReader reader, string columnName)
        {
            return reader.GetChar(reader.GetOrdinal(columnName));
        }

        /// <summary>Reads a stream of characters from the specified column, starting at location indicated by <paramref name="dataOffset">dataOffset</paramref>, into the buffer, starting at the location indicated by <paramref name="bufferOffset">bufferOffset</paramref>.</summary>
        /// <param name="columnName">The column name</param>
        /// <param name="dataOffset">The index within the row from which to begin the read operation.</param>
        /// <param name="buffer">The buffer into which to copy the data.</param>
        /// <param name="bufferOffset">The index with the buffer to which the data will be copied.</param>
        /// <param name="length">The maximum number of characters to read.</param>
        /// <returns>The actual number of characters read.</returns>
        public static long GetChars(this DbDataReader reader, string columnName, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            return reader.GetChars(reader.GetOrdinal(columnName), dataOffset, buffer, bufferOffset, length);
        }

        /// <summary>Gets name of the data type of the specified column.</summary>
        /// <param name="columnName">The column name</param>
        /// <returns>A string representing the name of the data type.</returns>
        /// <exception cref="T:System.InvalidCastException">The specified cast is not valid.</exception>
        public static string GetDataTypeName(this DbDataReader reader, string columnName)
        {
            return reader.GetDataTypeName(reader.GetOrdinal(columnName));
        }

        /// <summary>Gets the value of the specified column as a <see cref="T:System.DateTime"></see> object.</summary>
        /// <param name="columnName">The column name</param>
        /// <returns>The value of the specified column.</returns>
        /// <exception cref="T:System.InvalidCastException">The specified cast is not valid.</exception>
        public static DateTime GetDateTime(this DbDataReader reader, string columnName)
        {
            return reader.GetDateTime(reader.GetOrdinal(columnName));
        }

        /// <summary>Gets the value of the specified column as a <see cref="T:System.Decimal"></see> object.</summary>
        /// <param name="columnName">The column name</param>
        /// <returns>The value of the specified column.</returns>
        /// <exception cref="T:System.InvalidCastException">The specified cast is not valid.</exception>
        public static Decimal GetDecimal(this DbDataReader reader, string columnName)
        {
            return reader.GetDecimal(reader.GetOrdinal(columnName));
        }

        /// <summary>Gets the value of the specified column as a double-precision floating point number.</summary>
        /// <param name="columnName">The column name</param>
        /// <returns>The value of the specified column.</returns>
        /// <exception cref="T:System.InvalidCastException">The specified cast is not valid.</exception>
        public static double GetDouble(this DbDataReader reader, string columnName)
        {
            return reader.GetDouble(reader.GetOrdinal(columnName));
        }

        /// <summary>Gets the data type of the specified column.</summary>
        /// <param name="columnName">The column name</param>
        /// <returns>The data type of the specified column.</returns>
        /// <exception cref="T:System.InvalidCastException">The specified cast is not valid.</exception>
        public static Type GetFieldType(this DbDataReader reader, string columnName)
        {
            return reader.GetFieldType(reader.GetOrdinal(columnName));
        }

        /// <summary>Synchronously gets the value of the specified column as a type.</summary>
        /// <param name="columnName">The column name</param>
        /// <typeparam name="T">Synchronously gets the value of the specified column as a type.</typeparam>
        /// <returns>The column to be retrieved.</returns>
        /// <exception cref="T:System.InvalidOperationException">The connection drops or is closed during the data retrieval.   The <see cref="T:System.Data.SqlClient.DbDataReader"></see> is closed during the data retrieval.   There is no data ready to be read (for example, the first <see cref="M:System.Data.SqlClient.DbDataReader.Read"></see> hasn't been called, or returned false).   Tried to read a previously-read column in sequential mode.   There was an asynchronous operation in progress. This applies to all Get* methods when running in sequential mode, as they could be called while reading a stream.</exception>
        /// <exception cref="T:System.IndexOutOfRangeException">Trying to read a column that does not exist.</exception>
        /// <exception cref="T:System.InvalidCastException"><paramref name="T">T</paramref> doesn’t match the type returned by SQL Server or cannot be cast.</exception>
        public static T GetFieldValue<T>(this DbDataReader reader, string columnName)
        {
            return reader.GetFieldValue<T>(reader.GetOrdinal(columnName));
        }

        /// <summary>Asynchronously gets the value of the specified column as a type.</summary>
        /// <param name="columnName">The column name</param>
        /// <typeparam name="T">The type of the value to be returned.</typeparam>
        /// <returns>The type of the value to be returned.</returns>
        /// <exception cref="T:System.InvalidOperationException">The connection drops or is closed during the data retrieval.   The <see cref="T:System.Data.Common.DbDataReader"></see> is closed during the data retrieval.   There is no data ready to be read (for example, the first <see cref="M:System.Data.Common.DbDataReader.Read"></see> hasn't been called, or returned false).   Tried to read a previously-read column in sequential mode.   There was an asynchronous operation in progress. This applies to all Get* methods when running in sequential mode, as they could be called while reading a stream.</exception>
        /// <exception cref="T:System.IndexOutOfRangeException">Trying to read a column that does not exist.</exception>
        /// <exception cref="T:System.InvalidCastException"><paramref name="T">T</paramref> doesn’t match the type returned by the data source  or cannot be cast.</exception>
        public static Task<T> GetFieldValueAsync<T>(this DbDataReader reader, string columnName)
        {
            return reader.GetFieldValueAsync<T>(reader.GetOrdinal(columnName));
        }

        /// <summary>Asynchronously gets the value of the specified column as a type.</summary>
        /// <param name="columnName">The column name</param>
        /// <param name="cancellationToken">The cancellation instruction, which propagates a notification that operations should be canceled. This does not guarantee the cancellation. A setting of CancellationToken.None makes this method equivalent to <see cref="M:System.Data.Common.DbDataReader.GetFieldValueAsync``1(System.Int32)"></see>. The returned task must be marked as cancelled.</param>
        /// <typeparam name="T">The type of the value to be returned.</typeparam>
        /// <returns>The type of the value to be returned.</returns>
        /// <exception cref="T:System.InvalidOperationException">The connection drops or is closed during the data retrieval.   The <see cref="T:System.Data.Common.DbDataReader"></see> is closed during the data retrieval.   There is no data ready to be read (for example, the first <see cref="M:System.Data.Common.DbDataReader.Read"></see> hasn't been called, or returned false).   Tried to read a previously-read column in sequential mode.   There was an asynchronous operation in progress. This applies to all Get* methods when running in sequential mode, as they could be called while reading a stream.</exception>
        /// <exception cref="T:System.IndexOutOfRangeException">Trying to read a column that does not exist.</exception>
        /// <exception cref="T:System.InvalidCastException"><paramref name="T">T</paramref> doesn’t match the type returned by the data source or cannot be cast.</exception>
        public static Task<T> GetFieldValueAsync<T>(this DbDataReader reader, string columnName, CancellationToken cancellationToken)
        {
            return reader.GetFieldValueAsync<T>(reader.GetOrdinal(columnName), cancellationToken);
        }

        /// <summary>Gets the value of the specified column as a single-precision floating point number.</summary>
        /// <param name="columnName">The column name</param>
        /// <returns>The value of the specified column.</returns>
        /// <exception cref="T:System.InvalidCastException">The specified cast is not valid.</exception>
        public static float GetFloat(this DbDataReader reader, string columnName)
        {
            return reader.GetFloat(reader.GetOrdinal(columnName));
        }

        /// <summary>Gets the value of the specified column as a globally-unique identifier (GUID).</summary>
        /// <param name="columnName">The column name</param>
        /// <returns>The value of the specified column.</returns>
        /// <exception cref="T:System.InvalidCastException">The specified cast is not valid.</exception>
        public static Guid GetGuid(this DbDataReader reader, string columnName)
        {
            return reader.GetGuid(reader.GetOrdinal(columnName));
        }

        /// <summary>Gets the value of the specified column as a 16-bit signed integer.</summary>
        /// <param name="columnName">The column name</param>
        /// <returns>The value of the specified column.</returns>
        /// <exception cref="T:System.InvalidCastException">The specified cast is not valid.</exception>
        public static short GetInt16(this DbDataReader reader, string columnName)
        {
            return reader.GetInt16(reader.GetOrdinal(columnName));
        }

        /// <summary>Gets the value of the specified column as a 32-bit signed integer.</summary>
        /// <param name="columnName">The column name</param>
        /// <returns>The value of the specified column.</returns>
        /// <exception cref="T:System.InvalidCastException">The specified cast is not valid.</exception>
        public static int GetInt32(this DbDataReader reader, string columnName)
        {
            return reader.GetInt32(reader.GetOrdinal(columnName));
        }

        /// <summary>Gets the value of the specified column as a 64-bit signed integer.</summary>
        /// <param name="columnName">The column name</param>
        /// <returns>The value of the specified column.</returns>
        /// <exception cref="T:System.InvalidCastException">The specified cast is not valid.</exception>
        public static long GetInt64(this DbDataReader reader, string columnName)
        {
            return reader.GetInt64(reader.GetOrdinal(columnName));
        }

        /// <summary>Retrieves data as a <see cref="T:System.IO.Stream"></see>.</summary>
        /// <param name="columnName">The column name</param>
        /// <returns>The returned object.</returns>
        /// <exception cref="T:System.InvalidOperationException">The connection drops or is closed during the data retrieval.   The <see cref="T:System.Data.Common.DbDataReader"></see> is closed during the data retrieval.   There is no data ready to be read (for example, the first <see cref="M:System.Data.Common.DbDataReader.Read"></see> hasn't been called, or returned false).   Tried to read a previously-read column in sequential mode.   There was an asynchronous operation in progress. This applies to all Get* methods when running in sequential mode, as they could be called while reading a stream.</exception>
        /// <exception cref="T:System.IndexOutOfRangeException">Trying to read a column that does not exist.</exception>
        /// <exception cref="T:System.InvalidCastException">The returned type was not one of the types below:  binary  image  varbinary  udt</exception>
        public static Stream GetStream(this DbDataReader reader, string columnName)
        {
            return reader.GetStream(reader.GetOrdinal(columnName));
        }

        /// <summary>Gets the value of the specified column as an instance of <see cref="T:System.String"></see>.</summary>
        /// <param name="columnName">The column name</param>
        /// <returns>The value of the specified column.</returns>
        /// <exception cref="T:System.InvalidCastException">The specified cast is not valid.</exception>
        public static string GetString(this DbDataReader reader, string columnName)
        {
            return reader.GetString(reader.GetOrdinal(columnName));
        }

        /// <summary>Retrieves data as a <see cref="T:System.IO.TextReader"></see>.</summary>
        /// <param name="columnName">The column name</param>
        /// <returns>The returned object.</returns>
        /// <exception cref="T:System.InvalidOperationException">The connection drops or is closed during the data retrieval.   The <see cref="T:System.Data.Common.DbDataReader"></see> is closed during the data retrieval.   There is no data ready to be read (for example, the first <see cref="M:System.Data.Common.DbDataReader.Read"></see> hasn't been called, or returned false).   Tried to read a previously-read column in sequential mode.   There was an asynchronous operation in progress. This applies to all Get* methods when running in sequential mode, as they could be called while reading a stream.</exception>
        /// <exception cref="T:System.IndexOutOfRangeException">Trying to read a column that does not exist.</exception>
        /// <exception cref="T:System.InvalidCastException">The returned type was not one of the types below:  char  nchar  ntext  nvarchar  text  varchar</exception>
        public static TextReader GetTextReader(this DbDataReader reader, string columnName)
        {
            return reader.GetTextReader(reader.GetOrdinal(columnName));
        }

        /// <summary>Gets the value of the specified column as an instance of <see cref="T:System.Object"></see>.</summary>
        /// <param name="columnName">The column name</param>
        /// <returns>The value of the specified column.</returns>
        public static object GetValue(this DbDataReader reader, string columnName)
        {
            return reader.GetValue(reader.GetOrdinal(columnName));
        }

        /// <summary>Gets a value that indicates whether the column contains nonexistent or missing values.</summary>
        /// <param name="columnName">The column name</param>
        /// <returns>true if the specified column is equivalent to <see cref="T:System.DBNull"></see>; otherwise false.</returns>
        public static bool IsDBNull(this DbDataReader reader, string columnName)
        {
            return reader.IsDBNull(reader.GetOrdinal(columnName));
        }

        /// <summary>An asynchronous version of <see cref="M:System.Data.Common.DbDataReader.IsDBNull(System.Int32)"></see>, which gets a value that indicates whether the column contains non-existent or missing values.</summary>
        /// <param name="columnName">The column name</param>
        /// <returns>true if the specified column value is equivalent to DBNull otherwise false.</returns>
        /// <exception cref="T:System.InvalidOperationException">The connection drops or is closed during the data retrieval.   The <see cref="T:System.Data.Common.DbDataReader"></see> is closed during the data retrieval.   There is no data ready to be read (for example, the first <see cref="M:System.Data.Common.DbDataReader.Read"></see> hasn't been called, or returned false).   Trying to read a previously read column in sequential mode.   There was an asynchronous operation in progress. This applies to all Get* methods when running in sequential mode, as they could be called while reading a stream.</exception>
        /// <exception cref="T:System.IndexOutOfRangeException">Trying to read a column that does not exist.</exception>
        public static Task<bool> IsDBNullAsync(this DbDataReader reader, string columnName)
        {
            return reader.IsDBNullAsync(reader.GetOrdinal(columnName));
        }

        /// <summary>An asynchronous version of <see cref="M:System.Data.Common.DbDataReader.IsDBNull(System.Int32)"></see>, which gets a value that indicates whether the column contains non-existent or missing values. Optionally, sends a notification that operations should be cancelled.</summary>
        /// <param name="columnName">The column name</param>
        /// <param name="cancellationToken">The cancellation instruction, which propagates a notification that operations should be canceled. This does not guarantee the cancellation. A setting of CancellationToken.None makes this method equivalent to <see cref="M:System.Data.Common.DbDataReader.IsDBNullAsync(System.Int32)"></see>. The returned task must be marked as cancelled.</param>
        /// <returns>true if the specified column value is equivalent to DBNull otherwise false.</returns>
        /// <exception cref="T:System.InvalidOperationException">The connection drops or is closed during the data retrieval.   The <see cref="T:System.Data.Common.DbDataReader"></see> is closed during the data retrieval.   There is no data ready to be read (for example, the first <see cref="M:System.Data.Common.DbDataReader.Read"></see> hasn't been called, or returned false).   Trying to read a previously read column in sequential mode.   There was an asynchronous operation in progress. This applies to all Get* methods when running in sequential mode, as they could be called while reading a stream.</exception>
        /// <exception cref="T:System.IndexOutOfRangeException">Trying to read a column that does not exist.</exception>
        public static Task<bool> IsDBNullAsync(this DbDataReader reader, string columnName, CancellationToken cancellationToken)
        {
            return reader.IsDBNullAsync(reader.GetOrdinal(columnName), cancellationToken);
        }

        /// <summary>
        /// Gets the value of a specified column as a string object, allowing null.
        /// </summary>
        /// <param name="columnName">The column name</param>
        public static string GetStringOrNull(this DbDataReader reader, string columnName)
        {
            return reader.IsDBNull(reader.GetOrdinal(columnName)) ? null : reader.GetString(columnName);
        }

        /// <summary>
        /// Gets the value of a specified column as a string object. When null the default value is used instead.
        /// </summary>
        /// <param name="columnName">The column name</param>
        /// <param name="default">Default string to return if column is null</param>
        /// <returns>String at columnName if not null, default otherwise</returns>
        public static string GetStringOrDefault(this DbDataReader reader, string columnName, string @default)
        {
            return reader.IsDBNull(reader.GetOrdinal(columnName)) ? @default : reader.GetString(columnName);
        }

        /// <summary>
        /// Gets the value of a specified column as a DateTime.
        /// </summary>
        /// <param name="columnName">The column name</param>
        /// <returns>DateTime at columnName if not null, null otherwise</returns>
        public static DateTime? GetDateTimeOrNull(this DbDataReader reader, string columnName)
        {
            return reader.IsDBNull(reader.GetOrdinal(columnName)) ? null : (DateTime?)reader.GetDateTime(columnName);
        }

        /// <summary>
        /// Gets the value of a specified column as an integer.
        /// </summary>
        /// <param name="columnName">The column name</param>
        /// <returns>Integer at columnName if not null, null otherwise</returns>
        public static int? GetInt32OrNull(this DbDataReader reader, string columnName)
        {
            return reader.IsDBNull(reader.GetOrdinal(columnName)) ? null : (int?)reader.GetInt32(columnName);
        }

        /// <summary>
        /// Gets the value of a specified column as an integer. When null the default value is used instead.
        /// </summary>
        /// <param name="columnName">The column name</param>
        /// <param name="default">Default integer to return if column is null</param>
        /// <returns>Integer at columnName if not null, default otherwise</returns>
        public static int GetInt32OrDefault(this DbDataReader reader, string columnName, int @default)
        {
            return reader.IsDBNull(reader.GetOrdinal(columnName)) ? @default : reader.GetInt32(columnName);
        }
    }
}
