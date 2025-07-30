using System.Collections;
using System.Collections.Generic;
using Firebase.Database;
using UnityEngine;
using Firebase;
using Firebase.Extensions;


public class FireBaseDataBaseManager : MonoBehaviour
{
    private DatabaseReference reference;
    public ImageSwitcher imageSwitcher;

    private void Awake()
    {
        FirebaseApp app = FirebaseApp.DefaultInstance;

        reference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    private void Start()
    {
        TilemapDetail tilemapDetail = new TilemapDetail(1, 1, TilemapSate.Tilemap);

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
    public void UpdateUserName(string userId, string newName)
    {
        reference.Child("Users").Child(userId).Child("Name").SetValueAsync(newName).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Tên người chơi đã được cập nhật trên Firebase: " + newName);
            }
            else
            {
                Debug.LogError("Lỗi khi cập nhật tên lên Firebase: " + task.Exception);
            }
        });
    }
    public void LoadUserName(string userId, System.Action<string> onNameLoaded)
    {
        reference.Child("Users").Child(userId).Child("Name").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                string name = snapshot.Value?.ToString();
                onNameLoaded?.Invoke(name);
            }
            else
            {
                Debug.LogError("Lỗi khi đọc tên từ Firebase: " + task.Exception);
                onNameLoaded?.Invoke(null);
            }
        });
    }
    
    // map
    public void UnlockMode(string userId)
    {
        reference.Child("Users").Child(userId).Child("mode").SetValueAsync(true).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
               imageSwitcher.UpData();
            }
            else
            {
                Debug.LogError("Lỗi khi cập nhật mode: " + task.Exception);
            }
        });
    }

    public void LoadMode(string userId, System.Action<bool> onLoaded)
    {
        reference.Child("Users").Child(userId).Child("mode").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                bool mode = false;
                if (snapshot != null && snapshot.Value != null)
                {
                    mode = (bool)snapshot.Value;
                }

                Debug.Log("Giá trị mode từ Firebase: " + mode);
                onLoaded?.Invoke(mode);
            }
            else
            {
                Debug.LogError("Không thể load mode: " + task.Exception);
                onLoaded?.Invoke(false);
            }
        });
    }



}  