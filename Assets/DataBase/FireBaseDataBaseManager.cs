using System.Collections;
using System.Collections.Generic;
using Firebase.Database;
using UnityEngine;
using Firebase;
using Firebase.Extensions;


public class FireBaseDataBaseManager : MonoBehaviour
{
    private DatabaseReference reference;

    private void Awake()
    {
        FirebaseApp app = FirebaseApp.DefaultInstance;

        reference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    private void Start()
    {
        TilemapDetail tilemapDetail = new TilemapDetail(0, 0, TilemapSate.Tilemap);

        WriteDataBase("123", tilemapDetail.ToString());

        ReadDataBase("123");
    }

    public void WriteDataBase(string id, string message)
    {
        reference.Child("User").Child(id).SetValueAsync(message).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Ghi du lieu thanh cong");
            }
            else
            {
                Debug.Log("Ghi du lieu that bai: " +task.Exception);
            }
        });
    }

    public void ReadDataBase (string id)
    {
        reference.Child("User").Child(id).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                Debug.Log ("Doc du lieu thanh cong: " +  snapshot.Value.ToString());
            }
            else
            {
                Debug.Log("Doc du lieu that bai: " + task.Exception);
            }
        });
    }
}
