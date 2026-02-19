using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Npgsql;
using System.Net;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaPostgres
{
    public class SetObstacleInstance
    {
        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string connString = "Host=database-1.czymyocosc4b.eu-north-1.rds.amazonaws.com;Port=5432;Username=postgres;Password=KWxhKrHsLtXsHYtr6JxO;Database=dnd";
            try
            {
                string obstacleType = request.QueryStringParameters?["type"];
                int obstaclePf = int.Parse(request.QueryStringParameters?["pf"]);
                int obstacleX = int.Parse(request.QueryStringParameters?["x"]);
                int obstacleY = int.Parse(request.QueryStringParameters?["y"]);

                await using var conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();
                string query = @"INSERT INTO obstacle_instance(x, y, type, current_pf) VALUES (@obstacleX, @obstacleY, @obstacleType, @obstaclePf);";
                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("obstacleType", obstacleType);
                cmd.Parameters.AddWithValue("obstaclePf", obstaclePf);
                cmd.Parameters.AddWithValue("obstacleX", obstacleX);
                cmd.Parameters.AddWithValue("obstacleY", obstacleY);
                await cmd.ExecuteNonQueryAsync();
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = "Ostacolo inserito correttamente"
                };
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Errore: {ex.Message}");
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Body = $"Errore interno: {ex.Message}"
                };
            }
        }
    }
}