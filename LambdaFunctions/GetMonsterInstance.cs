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
    public class GetMonsterInstance
    {
        public async Task<string> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string connString = "Host=database-1.czymyocosc4b.eu-north-1.rds.amazonaws.com;Port=5432;Username=postgres;Password=KWxhKrHsLtXsHYtr6JxO;Database=dnd";
            int monsterX = int.Parse(request.QueryStringParameters?["x"]);
            int monsterY = int.Parse(request.QueryStringParameters?["y"]);
            try
            {
                await using var conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();

                string query = @"SELECT type FROM monster_instance WHERE x=@x AND y=@y;";
                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("x", monsterX);
                cmd.Parameters.AddWithValue("y", monsterY);
                await using var reader = await cmd.ExecuteReaderAsync();

                string type = "";

                while (await reader.ReadAsync())
                {
                    type = reader.GetString(0);

                }

                return type;

            }
            catch (Exception ex)
            {
                context.Logger.LogError(ex.ToString());
                return "";
            }
        }
    }
}
