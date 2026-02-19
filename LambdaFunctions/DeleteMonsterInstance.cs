using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Npgsql;
using System.Net;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaPostgres
{
    public class DeleteMonsterInstance
    {
        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string connString = "Host=database-1.czymyocosc4b.eu-north-1.rds.amazonaws.com;Port=5432;Username=postgres;Password=KWxhKrHsLtXsHYtr6JxO;Database=dnd";
            try
            {
                int monsterX = int.Parse(request.QueryStringParameters?["x"]);
                int monsterY = int.Parse(request.QueryStringParameters?["y"]);

                await using var conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();
                string query = @"DELETE FROM monster_instance WHERE x=@x AND y=@y";
                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("x", monsterX);
                cmd.Parameters.AddWithValue("y", monsterY);
                await cmd.ExecuteNonQueryAsync();
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = "Mostro cancellato correttamente"
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