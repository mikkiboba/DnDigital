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
    public class Action
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Range { get; set; }
        public string Damage { get; set; }
    }

    public class GetMonsterActions
    {
        public async Task<List<Action>> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string connString = "Host=database-1.czymyocosc4b.eu-north-1.rds.amazonaws.com;Port=5432;Username=postgres;Password=KWxhKrHsLtXsHYtr6JxO;Database=dnd";
            var actions = new List<Action>();
            string characterName = request.QueryStringParameters?["name"];

            try
            {
                await using var conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();

                string query = @"SELECT a.name, a.description, a.range, a.damage FROM action a JOIN action_pg apg ON a.name = apg.action JOIN monster m ON apg.creature = m.id WHERE m.name = @charName;";
                await using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("charName", characterName);
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var action = new Action
                    {
                        Name = reader.GetString(0),
                        Description = reader.GetString(1),
                        Range = reader.GetInt32(2),
                        Damage = reader.GetString(3)
                    };
                    actions.Add(action);
                }
                return actions;
            }
            catch (Exception ex)
            {
                context.Logger.LogError(ex.ToString());
                return new List<Action>();
            }
        }
    }
}
