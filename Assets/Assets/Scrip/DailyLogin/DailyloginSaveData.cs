using System.Collections.Generic;
[System.Serializable]
public class DailyLoginSaveData
{
    public int LoginDays = 0;
    public string DailyloginDate = "";

    public Dictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>()
        {
            { "LoginDays", LoginDays },
            { "DailyloginDate", DailyloginDate }
        };
    }
}