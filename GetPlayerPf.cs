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

    public class GetPlayerPf
    {
        public async Task<int> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string connString = "Host=database-1.czymyocosc4b.eu-north-1.rds.amazonaws.com;Port=5432;Username=postgres;Password=KWxhKrHsLtXsHYtr6JxO;Database=dnd";
            string characterName = request.QueryStringParameters?["name"];
            int curr_pf = 0;

            try
            {
                await using var conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();

                string query = @"SELECT current_pf FROM character WHERE name = @charName;";
                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("charName", characterName);
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    curr_pf = reader.GetInt32(0);
                }
                return curr_pf;
            }
            catch (Exception ex)
            {
                context.Logger.LogError(ex.ToString());
                return curr_pf;
            }
        }
    }
}
