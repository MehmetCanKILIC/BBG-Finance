using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace BBGFinance.Core
{
    /// <summary>
    /// Bu portalın kendi kullanıcı/oturum tablolarını barındıran veritabanı (yazılabilir).
    /// </summary>
    public static class DbHelper
    {
        private const string ConnectionName = "AuthDB";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionName].ConnectionString);
        }

        public static DataTable ExecuteQuery(string sql, params SqlParameter[] parameters)
        {
            return SqlHelperCore.ExecuteQuery(GetConnection, sql, parameters);
        }

        public static int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            return SqlHelperCore.ExecuteNonQuery(GetConnection, sql, parameters);
        }

        public static object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            return SqlHelperCore.ExecuteScalar(GetConnection, sql, parameters);
        }

        public static SqlParameter Param(string name, object value)
        {
            return SqlHelperCore.Param(name, value);
        }

        public static SqlParameter Param(string name, SqlDbType type, object value)
        {
            return SqlHelperCore.Param(name, type, value);
        }
    }

    /// <summary>
    /// JP_ROIBEDS rezervasyon veritabanı. SADECE OKUNUR — bu sınıf üzerinden hiçbir yazma
    /// işlemi (INSERT/UPDATE/DELETE) çalıştırılmamalıdır; kasıtlı olarak ExecuteNonQuery
    /// metodu yoktur.
    /// </summary>
    public static class ReportDbHelper
    {
        private const string ConnectionName = "ReportDB";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings[ConnectionName].ConnectionString);
        }

        public static DataTable ExecuteQuery(string sql, params SqlParameter[] parameters)
        {
            return SqlHelperCore.ExecuteQuery(GetConnection, sql, parameters);
        }

        /// <summary>Gerçek async ADO.NET çağrısı (OpenAsync/ExecuteReaderAsync) - DataTable.Load
        /// senkron bir reader tüketimi olsa da, DB'ye gidip cevap BEKLEME kısmı I/O tamamlanma
        /// portlarını kullanır ve thread'i BLOKE ETMEZ. Dashboard'daki paralel rapor sorguları
        /// bunu kullanır - Task.Run + senkron Fill() ThreadPool worker thread'lerini I/O boyunca
        /// meşgul tutuyordu, bu ise tutmaz.</summary>
        public static Task<DataTable> ExecuteQueryAsync(string sql, params SqlParameter[] parameters)
        {
            return SqlHelperCore.ExecuteQueryAsync(GetConnection, sql, parameters);
        }

        public static object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            return SqlHelperCore.ExecuteScalar(GetConnection, sql, parameters);
        }

        public static SqlParameter Param(string name, object value)
        {
            return SqlHelperCore.Param(name, value);
        }

        public static SqlParameter Param(string name, SqlDbType type, object value)
        {
            return SqlHelperCore.Param(name, type, value);
        }
    }

    internal static class SqlHelperCore
    {
        public static DataTable ExecuteQuery(Func<SqlConnection> connFactory, string sql, SqlParameter[] parameters)
        {
            var dt = new DataTable();
            using (var con = connFactory())
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.CommandTimeout = 60;
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                con.Open();
                using (var adapter = new SqlDataAdapter(cmd))
                    adapter.Fill(dt);
            }
            return dt;
        }

        public static async Task<DataTable> ExecuteQueryAsync(Func<SqlConnection> connFactory, string sql, SqlParameter[] parameters)
        {
            var dt = new DataTable();
            using (var con = connFactory())
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.CommandTimeout = 60;
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                await con.OpenAsync().ConfigureAwait(false);
                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                    dt.Load(reader);
            }
            return dt;
        }

        public static int ExecuteNonQuery(Func<SqlConnection> connFactory, string sql, SqlParameter[] parameters)
        {
            using (var con = connFactory())
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.CommandTimeout = 60;
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                con.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(Func<SqlConnection> connFactory, string sql, SqlParameter[] parameters)
        {
            using (var con = connFactory())
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.CommandTimeout = 60;
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                con.Open();
                return cmd.ExecuteScalar();
            }
        }

        public static SqlParameter Param(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        public static SqlParameter Param(string name, SqlDbType type, object value)
        {
            return new SqlParameter(name, type) { Value = value ?? DBNull.Value };
        }
    }
}
