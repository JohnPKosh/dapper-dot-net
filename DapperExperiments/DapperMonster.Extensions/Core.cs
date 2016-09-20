using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Dynamic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;


namespace DapperMonster.Extensions
{
    public static class Core
    {

        #region Query Method Overloads

        /// <summary>
        /// Return a sequence of JObjects with properties matching the columns
        /// </summary>
        /// <remarks>Note: each row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static IEnumerable<JObject> QueryToJObjects(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null, params JsonConverter[] converters)
        {
            var settings = converters != null && converters.Length > 0 ? new JsonSerializerSettings { Converters = converters } : null;
            var serializer = JsonSerializer.CreateDefault(settings);
            return cnn.Query(sql, param, transaction, buffered, commandTimeout, commandType).Select(o => JObject.FromObject(o, serializer)).Cast<JObject>();
        }

        /// <summary>
        /// Return a JArray with properties matching the columns
        /// </summary>
        /// <remarks>Note: each row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static JArray QueryToJArray(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null, params JsonConverter[] converters)
        {
            var settings = converters != null && converters.Length > 0 ? new JsonSerializerSettings { Converters = converters } : null;
            var serializer = JsonSerializer.CreateDefault(settings);
            return new JArray(cnn.Query(sql, param, transaction, buffered, commandTimeout, commandType).Select(o => JToken.FromObject(o, serializer)));
        }
        
        /// <summary>
        /// Return a sequence of JObjects with properties matching the columns
        /// </summary>
        /// <remarks>Note: each row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static Stream QueryToJsonStream(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null, Encoding encoding = null, params JsonConverter[] converters)
        {
            var rv = new MemoryStream();
            var writer = new JsonTextWriter(new StreamWriter(rv, encoding ?? Encoding.Default));
            (cnn.QueryToJArray(sql, param, transaction, buffered, commandTimeout, commandType, converters)).WriteTo(writer, converters);
            writer.Flush();
            if (rv.CanSeek) rv.Seek(0, SeekOrigin.Begin);
            return rv;
        }

        /// <summary>
        /// Return a JSON JArray as string with properties matching the columns
        /// </summary>
        /// <remarks>Note: each row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static string QueryToJsonString(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null, Encoding encoding = null, params JsonConverter[] converters)
        {
            var rv = new MemoryStream();
            using (var writer = new JsonTextWriter(new StreamWriter(rv, encoding ?? Encoding.Default)))
            {
                (cnn.QueryToJArray(sql, param, transaction, buffered, commandTimeout, commandType, converters)).WriteTo(writer, converters);
                writer.Flush();
                //if (rv.CanSeek) rv.Seek(0, SeekOrigin.Begin);
            }
            return encoding?.GetString(rv.ToArray()) ?? Encoding.Default.GetString(rv.ToArray());

            /* TODO: Review perfomance impact because alternately we could do this: return (cnn.QueryToJArray(sql, param, transaction, buffered, commandTimeout, commandType)).ToString(); */
        }

        /// <summary>
        /// Return a Stream BSON JArray with properties matching the columns
        /// </summary>
        /// <remarks>Note: each row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static Stream QueryToBsonStream(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null, params JsonConverter[] converters)
        {
            var rv = new MemoryStream();
            var writer = new BsonWriter(rv);
            (cnn.QueryToJArray(sql, param, transaction, buffered, commandTimeout, commandType, converters)).WriteTo(writer, converters);
            writer.Flush();
            if (rv.CanSeek) rv.Seek(0, SeekOrigin.Begin);
            return rv;
        }

        /// <summary>
        /// Return a BSON JArray as byte[] with properties matching the columns
        /// </summary>
        /// <remarks>Note: each row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static byte[] QueryToBsonBytes(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null, params JsonConverter[] converters)
        {
            var rv = new MemoryStream();
            var writer = new BsonWriter(rv);
            (cnn.QueryToJArray(sql, param, transaction, buffered, commandTimeout, commandType, converters)).WriteTo(writer, converters);
            writer.Flush();
            if (rv.CanSeek) rv.Seek(0, SeekOrigin.Begin);
            return rv.ToArray();
        }

        /// <summary>
        /// Return a BSON JArray as Base64 string with properties matching the columns
        /// </summary>
        /// <remarks>Note: each row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static string QueryToBsonBase64(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null, params JsonConverter[] converters)
        {
            var rv = new MemoryStream();
            var writer = new BsonWriter(rv);
            (cnn.QueryToJArray(sql, param, transaction, buffered, commandTimeout, commandType, converters)).WriteTo(writer, converters);
            writer.Flush();
            if (rv.CanSeek) rv.Seek(0, SeekOrigin.Begin);
            return Convert.ToBase64String(rv.ToArray(), Base64FormattingOptions.None);
        }
        
        #endregion

        /// <summary>
        /// Return a JArray with properties matching the columns
        /// </summary>
        /// <remarks>Note: each row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static JObject QueryFirstJObject(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, params JsonConverter[] converters)
        {
            var settings = converters != null && converters.Length > 0 ? new JsonSerializerSettings { Converters = converters } : null;
            var serializer = JsonSerializer.CreateDefault(settings);
            return new JObject(JToken.FromObject(cnn.QueryFirst(sql, param, transaction, commandTimeout, commandType), serializer));
        }
        
        /// <summary>
        /// Return a sequence of JObjects with properties matching the columns
        /// </summary>
        /// <remarks>Note: each row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static Stream QueryFirstToJsonStream(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, Encoding encoding = null, params JsonConverter[] converters)
        {
            var rv = new MemoryStream();
            var writer = new JsonTextWriter(new StreamWriter(rv, encoding ?? Encoding.Default));
            (cnn.QueryFirstJObject(sql, param, transaction, commandTimeout, commandType, converters)).WriteTo(writer, converters);
            writer.Flush();
            if (rv.CanSeek) rv.Seek(0, SeekOrigin.Begin);
            return rv;
        }

        /// <summary>
        /// Return a JSON JArray as string with properties matching the columns
        /// </summary>
        /// <remarks>Note: each row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static string QueryFirstToJsonString(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, Encoding encoding = null, params JsonConverter[] converters)
        {
            var rv = new MemoryStream();
            using (var writer = new JsonTextWriter(new StreamWriter(rv, encoding ?? Encoding.Default)))
            {
                (cnn.QueryFirstJObject(sql, param, transaction, commandTimeout, commandType, converters)).WriteTo(writer, converters);
                writer.Flush();
                //if (rv.CanSeek) rv.Seek(0, SeekOrigin.Begin);
            }
            return encoding?.GetString(rv.ToArray()) ?? Encoding.Default.GetString(rv.ToArray());

            /* TODO: Review perfomance impact because alternately we could do this: return (cnn.QueryToJArray(sql, param, transaction, buffered, commandTimeout, commandType)).ToString(); */
        }
        
        /// <summary>
        /// Return a Stream BSON JObject with properties matching the columns
        /// </summary>
        /// <remarks>Note: each row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static Stream QueryFirstToBsonStream(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null, params JsonConverter[] converters)
        {
            var rv = new MemoryStream();
            var writer = new BsonWriter(rv);
            (cnn.QueryFirstJObject(sql, param, transaction, commandTimeout, commandType, converters)).WriteTo(writer, converters);
            writer.Flush();
            if (rv.CanSeek) rv.Seek(0, SeekOrigin.Begin);
            return rv;
        }

        /// <summary>
        /// Return a BSON JArray as byte[] with properties matching the columns
        /// </summary>
        /// <remarks>Note: each row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static byte[] QueryFirstToBsonBytes(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, params JsonConverter[] converters)
        {
            var rv = new MemoryStream();
            var writer = new BsonWriter(rv);
            (cnn.QueryFirstJObject(sql, param, transaction,  commandTimeout, commandType, converters)).WriteTo(writer, converters);
            writer.Flush();
            if (rv.CanSeek) rv.Seek(0, SeekOrigin.Begin);
            return rv.ToArray();
        }

        /// <summary>
        /// Return a BSON JArray as Base64 string with properties matching the columns
        /// </summary>
        /// <remarks>Note: each row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public static string QueryFirstToBsonBase64(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, params JsonConverter[] converters)
        {
            var rv = new MemoryStream();
            var writer = new BsonWriter(rv);
            (cnn.QueryFirstJObject(sql, param, transaction, commandTimeout, commandType, converters)).WriteTo(writer, converters);
            writer.Flush();
            if (rv.CanSeek) rv.Seek(0, SeekOrigin.Begin);
            return Convert.ToBase64String(rv.ToArray(), Base64FormattingOptions.None);
        }





        ///// <summary>
        ///// Return a dynamic object with properties matching the columns
        ///// </summary>
        ///// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        //public static dynamic QueryFirst(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        //{
        //    return QueryFirst<DapperRow>(cnn, sql, param as object, transaction, commandTimeout, commandType);
        //}
        ///// <summary>
        ///// Return a dynamic object with properties matching the columns
        ///// </summary>
        ///// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        //public static dynamic QueryFirstOrDefault(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        //{
        //    return QueryFirstOrDefault<DapperRow>(cnn, sql, param as object, transaction, commandTimeout, commandType);
        //}
        ///// <summary>
        ///// Return a dynamic object with properties matching the columns
        ///// </summary>
        ///// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        //public static dynamic QuerySingle(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        //{
        //    return QuerySingle<DapperRow>(cnn, sql, param as object, transaction, commandTimeout, commandType);
        //}
        ///// <summary>
        ///// Return a dynamic object with properties matching the columns
        ///// </summary>
        ///// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        //public static dynamic QuerySingleOrDefault(this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        //{
        //    return QuerySingleOrDefault<DapperRow>(cnn, sql, param as object, transaction, commandTimeout, commandType);
        //}

        ///// <summary>
        ///// Executes a query, returning the data typed as per T
        ///// </summary>
        ///// <returns>A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is
        ///// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        ///// </returns>
        //public static IEnumerable<T> Query<T>(
        //    this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null
        //)
        //{
        //    var command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, buffered ? CommandFlags.Buffered : CommandFlags.None);
        //    var data = QueryImpl<T>(cnn, command, typeof(T));
        //    return command.Buffered ? data.ToList() : data;
        //}

        ///// <summary>
        ///// Executes a single-row query, returning the data typed as per T
        ///// </summary>
        ///// <returns>A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is
        ///// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        ///// </returns>
        //public static T QueryFirst<T>(
        //    this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null
        //)
        //{
        //    var command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, CommandFlags.None);
        //    return QueryRowImpl<T>(cnn, Row.First, ref command, typeof(T));
        //}
        ///// <summary>
        ///// Executes a single-row query, returning the data typed as per T
        ///// </summary>
        ///// <returns>A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is
        ///// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        ///// </returns>
        //public static T QueryFirstOrDefault<T>(
        //    this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null
        //)
        //{
        //    var command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, CommandFlags.None);
        //    return QueryRowImpl<T>(cnn, Row.FirstOrDefault, ref command, typeof(T));
        //}
        ///// <summary>
        ///// Executes a single-row query, returning the data typed as per T
        ///// </summary>
        ///// <returns>A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is
        ///// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        ///// </returns>
        //public static T QuerySingle<T>(
        //    this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null
        //)
        //{
        //    var command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, CommandFlags.None);
        //    return QueryRowImpl<T>(cnn, Row.Single, ref command, typeof(T));
        //}
        ///// <summary>
        ///// Executes a single-row query, returning the data typed as per T
        ///// </summary>
        ///// <returns>A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is
        ///// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        ///// </returns>
        //public static T QuerySingleOrDefault<T>(
        //    this IDbConnection cnn, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null
        //)
        //{
        //    var command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, CommandFlags.None);
        //    return QueryRowImpl<T>(cnn, Row.SingleOrDefault, ref command, typeof(T));
        //}

        ///// <summary>
        ///// Executes a single-row query, returning the data typed as per the Type suggested
        ///// </summary>
        ///// <returns>A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is
        ///// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        ///// </returns>
        //public static IEnumerable<object> Query(
        //    this IDbConnection cnn, Type type, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null
        //)
        //{
        //    if (type == null) throw new ArgumentNullException(nameof(type));
        //    var command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, buffered ? CommandFlags.Buffered : CommandFlags.None);
        //    var data = QueryImpl<object>(cnn, command, type);
        //    return command.Buffered ? data.ToList() : data;
        //}
        ///// <summary>
        ///// Executes a single-row query, returning the data typed as per the Type suggested
        ///// </summary>
        ///// <returns>A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is
        ///// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        ///// </returns>
        //public static object QueryFirst(
        //    this IDbConnection cnn, Type type, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null
        //)
        //{
        //    if (type == null) throw new ArgumentNullException(nameof(type));
        //    var command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, CommandFlags.None);
        //    return QueryRowImpl<object>(cnn, Row.First, ref command, type);
        //}
        ///// <summary>
        ///// Executes a single-row query, returning the data typed as per the Type suggested
        ///// </summary>
        ///// <returns>A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is
        ///// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        ///// </returns>
        //public static object QueryFirstOrDefault(
        //    this IDbConnection cnn, Type type, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null
        //)
        //{
        //    if (type == null) throw new ArgumentNullException(nameof(type));
        //    var command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, CommandFlags.None);
        //    return QueryRowImpl<object>(cnn, Row.FirstOrDefault, ref command, type);
        //}
        ///// <summary>
        ///// Executes a single-row query, returning the data typed as per the Type suggested
        ///// </summary>
        ///// <returns>A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is
        ///// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        ///// </returns>
        //public static object QuerySingle(
        //    this IDbConnection cnn, Type type, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null
        //)
        //{
        //    if (type == null) throw new ArgumentNullException(nameof(type));
        //    var command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, CommandFlags.None);
        //    return QueryRowImpl<object>(cnn, Row.Single, ref command, type);
        //}
        ///// <summary>
        ///// Executes a single-row query, returning the data typed as per the Type suggested
        ///// </summary>
        ///// <returns>A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is
        ///// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        ///// </returns>
        //public static object QuerySingleOrDefault(
        //    this IDbConnection cnn, Type type, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null
        //)
        //{
        //    if (type == null) throw new ArgumentNullException(nameof(type));
        //    var command = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, CommandFlags.None);
        //    return QueryRowImpl<object>(cnn, Row.SingleOrDefault, ref command, type);
        //}
        ///// <summary>
        ///// Executes a query, returning the data typed as per T
        ///// </summary>
        ///// <remarks>the dynamic param may seem a bit odd, but this works around a major usability issue in vs, if it is Object vs completion gets annoying. Eg type new [space] get new object</remarks>
        ///// <returns>A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is
        ///// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        ///// </returns>
        //public static IEnumerable<T> Query<T>(this IDbConnection cnn, CommandDefinition command)
        //{
        //    var data = QueryImpl<T>(cnn, command, typeof(T));
        //    return command.Buffered ? data.ToList() : data;
        //}

        ///// <summary>
        ///// Executes a query, returning the data typed as per T
        ///// </summary>
        ///// <remarks>the dynamic param may seem a bit odd, but this works around a major usability issue in vs, if it is Object vs completion gets annoying. Eg type new [space] get new object</remarks>
        ///// <returns>A single instance or null of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is
        ///// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        ///// </returns>
        //public static T QueryFirst<T>(this IDbConnection cnn, CommandDefinition command)
        //{
        //    return QueryRowImpl<T>(cnn, Row.First, ref command, typeof(T));
        //}
        ///// <summary>
        ///// Executes a query, returning the data typed as per T
        ///// </summary>
        ///// <remarks>the dynamic param may seem a bit odd, but this works around a major usability issue in vs, if it is Object vs completion gets annoying. Eg type new [space] get new object</remarks>
        ///// <returns>A single or null instance of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is
        ///// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        ///// </returns>
        //public static T QueryFirstOrDefault<T>(this IDbConnection cnn, CommandDefinition command)
        //{
        //    return QueryRowImpl<T>(cnn, Row.FirstOrDefault, ref command, typeof(T));
        //}
        ///// <summary>
        ///// Executes a query, returning the data typed as per T
        ///// </summary>
        ///// <remarks>the dynamic param may seem a bit odd, but this works around a major usability issue in vs, if it is Object vs completion gets annoying. Eg type new [space] get new object</remarks>
        ///// <returns>A single instance of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is
        ///// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        ///// </returns>
        //public static T QuerySingle<T>(this IDbConnection cnn, CommandDefinition command)
        //{
        //    return QueryRowImpl<T>(cnn, Row.Single, ref command, typeof(T));
        //}
        ///// <summary>
        ///// Executes a query, returning the data typed as per T
        ///// </summary>
        ///// <remarks>the dynamic param may seem a bit odd, but this works around a major usability issue in vs, if it is Object vs completion gets annoying. Eg type new [space] get new object</remarks>
        ///// <returns>A single instance of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column in assumed, otherwise an instance is
        ///// created per row, and a direct column-name===member-name mapping is assumed (case insensitive).
        ///// </returns>
        //public static T QuerySingleOrDefault<T>(this IDbConnection cnn, CommandDefinition command)
        //{
        //    return QueryRowImpl<T>(cnn, Row.SingleOrDefault, ref command, typeof(T));
        //}





    }
}
