using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Npgsql;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaPostgres
{
    public class Monster
    {
        public string Name { get; set; }
        public int Max_Pf { get; set; }
        public int AC { get; set; }
        public int Initiative { get; set; }
        public float CR { get; set; }
        public string Type { get; set; }
    }

    public class GetMonsters
    {
        public async Task<List<Monster>> FunctionHandler(ILambdaContext context)
        {
            string connString = "Host=database-1.czymyocosc4b.eu-north-1.rds.amazonaws.com;Port=5432;Username=postgres;Password=KWxhKrHsLtXsHYtr6JxO;Database=dnd";
            
            var monsters = new List<Monster>();

            try
            {
                await using var conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand("SELECT m.name, m.cr, m.type, c.max_pf, c.ac, c.initiative FROM monster m JOIN creature c ON c.id=m.id;", conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var monster = new Monster
                    {
                        Name = reader.GetString(0),
                        CR = reader.GetFloat(1),
                        Type = reader.GetString(2),
                        Max_Pf = reader.GetInt32(3),
                        AC = reader.GetInt32(4),
                        Initiative = reader.GetInt32(5)
                    };
                    monsters.Add(monster);
                }
                return monsters;
            }
            catch (Exception ex)
            {
                context.Logger.LogError(ex.ToString());
                return new List<Monster>();
            }
        }
    }
}
