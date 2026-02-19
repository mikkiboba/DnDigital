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

    public class ObstacleInstance
    {
        public string Type { get; set; }
        public int Curr_Pf { get; set; }
        public int X {  get; set; }
        public int Y { get; set; }
    }

    public class GetObstacleInstances
    {
        public async Task<List<ObstacleInstance>> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string connString = "Host=database-1.czymyocosc4b.eu-north-1.rds.amazonaws.com;Port=5432;Username=postgres;Password=KWxhKrHsLtXsHYtr6JxO;Database=dnd";
            
            var obstacles = new List<ObstacleInstance>();
            
            try
            {
                await using var conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();

                string query = @"SELECT type, current_pf, x, y FROM obstacle_instance;";
                await using var cmd = new NpgsqlCommand(query, conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var obstacle = new ObstacleInstance
                    {
                        Type = reader.GetString(0),
                        Curr_Pf = reader.GetInt32(1),
                        X = reader.GetInt32(2),
                        Y = reader.GetInt32(3)
                    };

                    obstacles.Add(obstacle);
                }
                return obstacles;
            }
            catch (Exception ex)
            {
                context.Logger.LogError(ex.ToString());
                return obstacles;
            }
        }
    }
}
