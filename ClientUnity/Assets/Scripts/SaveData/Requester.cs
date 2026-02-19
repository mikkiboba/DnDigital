using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class Requester
{

    public struct Instance
    {
        public string type;
        public int curr_pf;
        public int x;
        public int y;
    }

    public static async Task<List<Character>> RequestCharacters()
    {
        List<Character> characters = new List<Character>();
        UnityWebRequest www = UnityWebRequest.Get("https://o9wbc90xbl.execute-api.eu-north-1.amazonaws.com/characters");
        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Couldn't send GET request: " + www.error);
            return characters;
        }
        else
        {
            var json = www.downloadHandler.text;
            JArray array = JArray.Parse(json);
            foreach (JObject obj in array.Children<JObject>())
            {
                var character = new Character
                {
                    Name = obj["Name"]?.ToString(),
                    Max_Pf = obj["Max_Pf"].Value<int>(),
                    Bio = obj["Bio"]?.ToString(),
                    AC = obj["AC"].Value<int>(),
                    Initiative = obj["Initiative"].Value<int>(),
                    Actions = new List<CreatureEntity.Action>(),
                    Pass_Perc = obj["Pass_Perc"].Value<int>(),
                    Hit_Dice = obj["Hit_Dice"]?.ToString(),
                    Str = obj["Str"].Value<int>(),
                    Dex = obj["Dex"].Value<int>(),
                    Con = obj["Con"].Value<int>(),
                    Int = obj["Int"].Value<int>(),
                    Wis = obj["Wis"].Value<int>(),
                    Cha = obj["Cha"].Value<int>(),
                    Current_Pf = obj["Max_Pf"].Value<int>(),
                    Class = obj["Class"]?.ToString(),
                    Level = obj["Level"].Value<int>(),
                    Race = obj["Race"]?.ToString(),
                    Color = 0
                };
                character.Color = character.Name switch
                {
                    "Karina" => 3,
                    "Kabo" => 2,
                    "Cheese" => 1,
                    _ => 0
                };
                characters.Add(character);
            }
            characters.Sort((x, y) => x.Name.CompareTo(y.Name));
            return characters;
        }
    }

    public static async Task<List<Monster>> RequestMonsters()
    {
        List<Monster> listMonsters = new List<Monster>();
        UnityWebRequest www = UnityWebRequest.Get("https://o9wbc90xbl.execute-api.eu-north-1.amazonaws.com/monsters");
        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Couldn't send GET request: " + www.error);
            return listMonsters;
        }
        else
        {
            var json = www.downloadHandler.text;
            JArray array = JArray.Parse(json);
            foreach (JObject obj in array.Children<JObject>())
            {
                var monster = new Monster
                {
                    Name = obj["Name"]?.ToString(),
                    Max_Pf = obj["Max_Pf"].Value<int>(),
                    Bio = obj["Bio"]?.ToString(),
                    AC = obj["AC"].Value<int>(),
                    Initiative = obj["Initiative"].Value<int>(),
                    Actions = new List<CreatureEntity.Action>(),
                    CR = obj["CR"]?.Value<float?>(),
                    Type = obj["Type"]?.ToString(),
                    Current_Pf = obj["Max_Pf"].Value<int>()
                };
                listMonsters.Add(monster);
            }
            return listMonsters;
        }
    }

    public static async Task<List<Obstacle>> RequestObstacles()
    {
        List<Obstacle> listObstacles = new List<Obstacle>();
        UnityWebRequest www = UnityWebRequest.Get("https://o9wbc90xbl.execute-api.eu-north-1.amazonaws.com/obstacles");
        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Couldn't send GET request: " + www.error);
            return listObstacles;
        }
        else
        {
            var json = www.downloadHandler.text;
            JArray array = JArray.Parse(json);
            foreach (JObject obj in array.Children<JObject>())
            {
                var obstacle = new Obstacle
                {
                    Name = obj["Name"]?.ToString(),
                    Max_Pf = obj["Max_PF"].Value<int>(),
                    Bio = obj["Description"]?.ToString(),
                    Current_Pf = obj["Max_PF"].Value<int>()
                };
                listObstacles.Add(obstacle);
            }
            return listObstacles;
        }
    }

    public static async Task<List<CreatureEntity.Action>> RequestActions(string name)
    {
        List<CreatureEntity.Action> actions = new List<CreatureEntity.Action>();
        UnityWebRequest www = UnityWebRequest.Get("https://o9wbc90xbl.execute-api.eu-north-1.amazonaws.com/characters/actions?name=" + name);
        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Couldn't send GET request: " + www.error);
            return actions;
        }
        else
        {
            var json = www.downloadHandler.text;
            JArray array = JArray.Parse(json);
            foreach (JObject obj in array.Children<JObject>())
            {
                var action = new CreatureEntity.Action
                {
                    actionName = obj["Name"]?.ToString(),
                    description = obj["Description"]?.ToString(),
                    range = obj["Range"].Value<int>(),
                    damage = obj["Damage"]?.ToString()
                };
                actions.Add(action);
            }
            return actions;
        }
    }

    public static async Task<List<CreatureEntity.Action>> RequestMonsterActions(string name)
    {
        List<CreatureEntity.Action> actions = new List<CreatureEntity.Action>();
        UnityWebRequest www = UnityWebRequest.Get("https://o9wbc90xbl.execute-api.eu-north-1.amazonaws.com/monsters/actions?name=" + name);
        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Couldn't send GET request: " + www.error);
            return actions;
        }
        else
        {
            var json = www.downloadHandler.text;
            JArray array = JArray.Parse(json);
            foreach (JObject obj in array.Children<JObject>())
            {
                var action = new CreatureEntity.Action
                {
                    actionName = obj["Name"]?.ToString(),
                    description = obj["Description"]?.ToString(),
                    range = obj["Range"].Value<int>(),
                    damage = obj["Damage"]?.ToString()
                };
                actions.Add(action);
            }
            return actions;
        }
    }

    public static async Task<List<CreatureEntity.Action>> RequestSpells(string name)
    {
        List<CreatureEntity.Action> spells = new List<CreatureEntity.Action>();
        UnityWebRequest www = UnityWebRequest.Get("https://o9wbc90xbl.execute-api.eu-north-1.amazonaws.com/characters/spells?name=" + name);
        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Couldn't send GET request: " + www.error);
            return spells;
        }
        else
        {
            var json = www.downloadHandler.text;
            JArray array = JArray.Parse(json);
            foreach (JObject obj in array.Children<JObject>())
            {
                var spell = new CreatureEntity.Action
                {
                    actionName = obj["Name"]?.ToString(),
                    description = obj["Description"]?.ToString(),
                    range = obj["Range"].Value<int>(),
                    damage = obj["Damage"]?.ToString()
                };
                spells.Add(spell);
            }
            return spells;
        }
    }

    public static async void AddMonster(string type, int pf, int x, int y)
    {
        UnityWebRequest www = UnityWebRequest.PostWwwForm($"https://o9wbc90xbl.execute-api.eu-north-1.amazonaws.com/monsters?type={type}&pf={pf}&x={x}&y={y}", string.Empty);
        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Couldn't send POST request: " + www.error);
        }
        return;
    }

    public static async void AddObstacle(string type, int pf, int x, int y)
    {
        UnityWebRequest www = UnityWebRequest.PostWwwForm($"https://o9wbc90xbl.execute-api.eu-north-1.amazonaws.com/obstacles?type={type}&pf={pf}&x={x}&y={y}", string.Empty);
        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Couldn't send POST request: " + www.error);
        }
        return;
    }

    public static async Task<bool> ObstacleFound(int x, int y)
    {
        UnityWebRequest www = UnityWebRequest.Get($"https://o9wbc90xbl.execute-api.eu-north-1.amazonaws.com/obstacles/instance?x={x}&y={y}");
        await www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Couldn't send GET request: " + www.error);
            return false;
        }
        else
        {
            string result = www.downloadHandler.text;
            return result != "";
        }
    }

    public static async Task<bool> MonsterFound(int x, int y)
    {
        UnityWebRequest www = UnityWebRequest.Get($"https://o9wbc90xbl.execute-api.eu-north-1.amazonaws.com/monsters/instance?x={x}&y={y}");
        await www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Couldn't send GET request: " + www.error);
            return false;
        }
        else
        {
            string result = www.downloadHandler.text;
            return result != "";
        }
    }

    public static async void DeleteMonster(int x, int y)
    {
        UnityWebRequest www = UnityWebRequest.Delete($"https://o9wbc90xbl.execute-api.eu-north-1.amazonaws.com/monsters/instance?x={x}&y={y}");
        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Couldn't send DELETE request: " + www.error);
        }
        return;
    }

    public static async void DeleteObstacle(int x, int y)
    {
        UnityWebRequest www = UnityWebRequest.Delete($"https://o9wbc90xbl.execute-api.eu-north-1.amazonaws.com/obstacles/instance?x={x}&y={y}");
        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Couldn't send DELETE request: " + www.error);
        }
        return;
    }

    public static async Task<List<Instance>> GetMonsterInstances()
    {
        List<Instance> listMonsters = new List<Instance>();
        UnityWebRequest www = UnityWebRequest.Get("https://o9wbc90xbl.execute-api.eu-north-1.amazonaws.com/monsters/instances");
        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Couldn't send GET request: " + www.error);
            return listMonsters;
        }
        else
        {
            var json = www.downloadHandler.text;
            JArray array = JArray.Parse(json);
            foreach (JObject obj in array.Children<JObject>())
            {
                var monster = new Instance
                {
                    type = obj["Type"]?.ToString(),
                    curr_pf = obj["Curr_Pf"].Value<int>(),
                    x = obj["X"].Value<int>(),
                    y = obj["Y"].Value<int>(),
                };
                listMonsters.Add(monster);
            }
            return listMonsters;
        }
    }

    public static async Task<List<Instance>> GetObstacleInstances()
    {
        List < Instance > listObstacles = new List<Instance>();
        UnityWebRequest www = UnityWebRequest.Get("https://o9wbc90xbl.execute-api.eu-north-1.amazonaws.com/obstacles/instances");
        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Couldn't send GET request: " + www.error);
            return listObstacles;
        }
        else
        {
            var json = www.downloadHandler.text;
            JArray array = JArray.Parse(json);
            foreach (JObject obj in array.Children<JObject>())
            {
                var obstacle = new Instance
                {
                    type = obj["Type"]?.ToString(),
                    curr_pf = obj["Curr_Pf"].Value<int>(),
                    x = obj["X"].Value<int>(),
                    y = obj["Y"].Value<int>(),
                };
                listObstacles.Add(obstacle);
            }
            return listObstacles;
        }
    }

    public static async void SetPlayerPf(string name, int new_pf)
    {
        UnityWebRequest www = UnityWebRequest.Put($"https://o9wbc90xbl.execute-api.eu-north-1.amazonaws.com/characters?name={name}&pf={new_pf}", "");
        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Couldn't send PUT request: " + www.error);
        }
        return;
    }

    public static async void SetMonsterPf(int x, int new_pf, int y)
    {
        UnityWebRequest www = UnityWebRequest.Put($"https://o9wbc90xbl.execute-api.eu-north-1.amazonaws.com/monsters/instance?x={x}&pf={new_pf}&y={y}", "");
        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Couldn't send PUT request: " + www.error);
        }
        return;
    }

    public static async void SetObstaclePf(int x, int new_pf, int y)
    {
        UnityWebRequest www = UnityWebRequest.Put($"https://o9wbc90xbl.execute-api.eu-north-1.amazonaws.com/obstacles/instance?x={x}&pf={new_pf}&y={y}", "");
        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Couldn't send PUT request: " + www.error);
        }
        return;
    }

    public static async Task<int> GetPlayerPf (string name)
    {
        UnityWebRequest www = UnityWebRequest.Get($"https://o9wbc90xbl.execute-api.eu-north-1.amazonaws.com/characters/character?name={name}");
        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Couldn't send POST request: " + www.error);
            return 0;
        }
        else
        {
            int result = int.Parse(www.downloadHandler.text);
            return result;
        }
    }
}