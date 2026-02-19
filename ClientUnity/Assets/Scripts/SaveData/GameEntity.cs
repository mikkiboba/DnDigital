using System.Collections.Generic;
using UnityEngine;

public class GameEntity
{
    private int _ID;
    public int ID => _ID;
    private int _color; 
    public int Color => _color;
    private int _current_pf;
    public int Current_PF => _current_pf;

    private bool _assigned;
    public bool Assigned => _assigned;

    public GameEntity() { }

    public struct Action
    {
        public string actionName;
        public string description;
        public int range;
        public int damage;
    }

    private List<Action> _actions;
    public List<Action> Actions => _actions;
}
