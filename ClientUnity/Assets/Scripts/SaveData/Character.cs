using UnityEngine;

[System.Serializable]
public class Character : CreatureEntity
{
    public int Pass_Perc { get; set; }
    public string Hit_Dice { get; set; }
    public int Str { get; set; }
    public int Dex { get; set; }
    public int Con { get; set; }
    public int Int { get; set; }
    public int Wis { get; set; }
    public int Cha { get; set; }
    public string Class { get; set; }
    public int Level { get; set; }
    public string Race { get; set; }
    public bool Assigned {get; set; }
    public int Color { get; set; }
}