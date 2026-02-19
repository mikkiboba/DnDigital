using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Npgsql;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaPostgres
{
    public class Obstacle
    {
        public string Name { get; set; }
        public int Max_PF { get; set; }
        public string Description { get; set; }
    }

    public class GetObstacles
    {
        public async Task<List<Obstacle>> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string connString = "Host=database-1.czymyocosc4b.eu-north-1.rds.amazonaws.com;Port=5432;Username=postgres;Password=KWxhKrHsLtXsHYtr6JxO;Database=dnd";
            var obstacles = new List<Obstacle>();

            try
            {
                await using var conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();

                string query = "SELECT name, description, max_pf FROM obstacle;";
                await using var cmd = new NpgsqlCommand(query, conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var obstacle = new Obstacle
                    {
                        Name = reader.GetString(0),
                        Description = reader.GetString(1),
                        Max_PF = reader.GetInt32(2)
                    };
                    obstacles.Add(obstacle);
                }
                return obstacles;
            }
            catch (Exception ex)
            {
                context.Logger.LogError(ex.ToString());
                return new List<Obstacle>();
            }
        }
    }
}
