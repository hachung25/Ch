using UnityEngine;
using Firebase.Database;
using Firebase.Auth;
using System;
using System.Threading.Tasks;

public static class SaveManeger
{
    private const string DailyLoginKey = "DailyLogin";
    public static DailyLoginSaveData DailyloginData { get; private set; }

    private static DatabaseReference dbRef => FirebaseDatabase.DefaultInstance.RootReference;
    private static FirebaseUser user => FirebaseAuth.DefaultInstance.CurrentUser;

    public static async void LoadDailylogin()
    {
        if (user == null)
        {
            Debug.LogError("Chưa đăng nhập Firebase");
            DailyloginData = new DailyLoginSaveData();
            return;
        }

        DataSnapshot snapshot = await dbRef.Child(DailyLoginKey).Child(user.UserId).GetValueAsync();

        if (snapshot.Exists)
        {
            string json = snapshot.GetRawJsonValue();
            DailyloginData = JsonUtility.FromJson<DailyLoginSaveData>(json);
        }
        else
        {
            DailyloginData = new DailyLoginSaveData();
        }
    }

    public static void SaveDailylogin()
    {
        if (user == null)
        {
            Debug.LogError("Chưa đăng nhập Firebase");
            return;
        }

        string json = JsonUtility.ToJson(DailyloginData);
        dbRef.Child(DailyLoginKey).Child(user.UserId).SetRawJsonValueAsync(json);
    }

    public static void ResetDailylogin()
    {
        if (user == null)
        {
            Debug.LogError("Chưa đăng nhập Firebase");
            return;
        }

        DailyloginData = new DailyLoginSaveData();
        SaveDailylogin(); // Ghi đè dữ liệu mới
    }
}