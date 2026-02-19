using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Npgsql;
using System.Net;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaPostgres
{
    public class SetMonsterInstance
    {
        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string connString = "Host=database-1.czymyocosc4b.eu-north-1.rds.amazonaws.com;Port=5432;Username=postgres;Password=KWxhKrHsLtXsHYtr6JxO;Database=dnd";
            try
            {
                string monsterType = request.QueryStringParameters?["type"];
                int monsterPf = int.Parse(request.QueryStringParameters?["pf"]);
                int monsterX = int.Parse(request.QueryStringParameters?["x"]);
                int monsterY = int.Parse(request.QueryStringParameters?["y"]);

                await using var conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();
                string query = @"INSERT INTO monster_instance(type, curr_pf, x, y) VALUES (@monsterType, @monsterPf, @monsterX, @monsterY);";
                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("monsterType", monsterType);
                cmd.Parameters.AddWithValue("monsterPf", monsterPf);
                cmd.Parameters.AddWithValue("monsterX", monsterX);
                cmd.Parameters.AddWithValue("monsterY", monsterY);
                await cmd.ExecuteNonQueryAsync();
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = "Mostro inserito correttamente"
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