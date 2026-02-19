using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Npgsql;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaPostgres
{
    public class Character
    {
        public int AC { get; set; }
        public int Initiative { get; set; }
        public int Pass_Perc { get; set; }
        public string Hit_Dice { get; set; }
        public int Str { get; set; }
        public int Dex { get; set; }
        public int Con { get; set; }
        public int Int { get; set; }
        public int Wis { get; set; }
        public int Cha { get; set; }
        public int Curr_Pf { get; set; }
        public int Max_Pf { get; set; }
        public string Class { get; set; }
        public int Level { get; set; }
        public string Race { get; set; }
        public string Name { get; set; }
        public string Bio {get; set; }

    }

    public class GetSheet
    {
        public async Task<List<Character>> FunctionHandler(ILambdaContext context)
        {
            string connString = "Host=database-1.czymyocosc4b.eu-north-1.rds.amazonaws.com;Port=5432;Username=postgres;Password=KWxhKrHsLtXsHYtr6JxO;Database=dnd";
            var characters = new List<Character>();

            try
            {
                await using var conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand("SELECT ch.class, ch.level, ch.race, ch.name, c.ac, c.initiative, c.passive_perception, c.hit_dice, c.strength, c.dexterity, c.constitution, c.intelligence, c.wisdom, c.charisma, ch.current_pf, c.max_pf, c.bio FROM character ch JOIN creature c ON c.id=ch.id;", conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var character = new Character
                    {
                        Class = reader.GetString(0),
                        Level = reader.GetInt32(1),
                        Race = reader.GetString(2),
                        Name = reader.GetString(3),
                        AC = reader.GetInt32(4),
                        Initiative = reader.GetInt32(5),
                        Pass_Perc = reader.GetInt32(6),
                        Hit_Dice = reader.GetString(7),
                        Str = reader.GetInt32(8),
                        Dex = reader.GetInt32(9),
                        Con = reader.GetInt32(10),
                        Int = reader.GetInt32(11),
                        Wis = reader.GetInt32(12),
                        Cha = reader.GetInt32(13),
                        Curr_Pf = reader.GetInt32(14),
                        Max_Pf = reader.GetInt32(15),
                        Bio = reader.GetString(16)
                    };
                    characters.Add(character);
                }
                return characters;
            }
            catch (Exception ex)
            {
                context.Logger.LogError(ex.ToString());
                return new List<Character>();
            }
        }
    }
}
