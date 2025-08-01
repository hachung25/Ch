using System;
using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseRankingLoader : MonoBehaviour
{
    private DatabaseReference dbRef;

    void Awake()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void LoadTopRanking(Action<List<RankingData>> onCompleted)
    {
        dbRef.Child("Users").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            List<RankingData> list = new List<RankingData>();

            if (task.IsCompleted && task.Result.Exists)
            {
                foreach (var user in task.Result.Children)
                {
                    string name = "Unknown";
                    int ticket = 0;

                    if (user.Child("Name").Exists)
                        name = user.Child("Name").Value.ToString();

                    if (user.Child("Ticket").Exists &&
                        int.TryParse(user.Child("Ticket").Value.ToString(), out int parsed))
                        ticket = parsed;

                    list.Add(new RankingData
                    {
                        UserId = user.Key,   // ✅ THÊM DÒNG NÀY
                        Name = name,
                        Ticket = ticket
                    });
                }

                list.Sort((a, b) => b.Ticket.CompareTo(a.Ticket));
            }

            onCompleted?.Invoke(list);
        });
    }
}
