using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Npgsql;
using System.Net;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaPostgres
{
    public class SetPlayerPf
    {
        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string connString = "Host=database-1.czymyocosc4b.eu-north-1.rds.amazonaws.com;Port=5432;Username=postgres;Password=KWxhKrHsLtXsHYtr6JxO;Database=dnd";
            try
            {
                string name = request.QueryStringParameters?["name"];
                int newPf = int.Parse(request.QueryStringParameters?["pf"]);

                await using var conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();
                string query = @"UPDATE character SET current_pf = @pf WHERE name = @name;";
                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("name", name);
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