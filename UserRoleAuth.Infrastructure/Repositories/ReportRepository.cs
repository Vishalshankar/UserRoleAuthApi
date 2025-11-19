using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace UserRoleAuth.Infrastructure.Repositories
{
    public class ReportRepository
    {
        private readonly IConfiguration _config;
        public ReportRepository(IConfiguration config) => _config = config;

        public async Task<int> GetTotalUsersAsync()
        {
            var connStr = _config.GetConnectionString("DefaultConnection");
            await using var conn = new SqlConnection(connStr);
            await using var cmd = new SqlCommand("SELECT COUNT(*) FROM AspNetUsers", conn);
            await conn.OpenAsync();
            var res = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(res);
        }
    }
}
