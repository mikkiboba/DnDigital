using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Npgsql;
using System.Net;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaPostgres
{
    public class SetObstaclePf
    {
        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string connString = "Host=database-1.czymyocosc4b.eu-north-1.rds.amazonaws.com;Port=5432;Username=postgres;Password=KWxhKrHsLtXsHYtr6JxO;Database=dnd";
            try
            {
                int obstacleX = int.Parse(request.QueryStringParameters?["x"]);
                int obstacleY = int.Parse(request.QueryStringParameters?["y"]);
                int newPf = int.Parse(request.QueryStringParameters?["pf"]);

                await using var conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();
                string query = @"UPDATE obstacle_instance SET current_pf = @pf WHERE x = @x AND y=@y;";
                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("x", obstacleX);
                cmd.Parameters.AddWithValue("y", obstacleY);
                cmd.Parameters.AddWithValue("pf", newPf);
                await cmd.ExecuteNonQueryAsync();
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = "Update successful"
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