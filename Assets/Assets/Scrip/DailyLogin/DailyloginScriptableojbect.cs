using System;
using System.Collections.Generic; 
using UnityEngine;

[CreateAssetMenu(fileName = "DailyloginScriptableojbect", menuName = "ScriptableObjects/DailyloginScriptableojbect" )]
public class DailyloginScriptableojbect: ScriptableObject
{
    public List<Itemdays> rewards = new List<Itemdays>();
}

[Serializable]
public class Itemdays
{
    public int day;
    public int amount;
}
