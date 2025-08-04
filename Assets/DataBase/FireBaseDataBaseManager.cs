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

        // 🔒 Tắt chế độ lưu offline (tránh lỗi LOCK)
        FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);

        reference = FirebaseDatabase.DefaultInstance.RootReference;
    }


    private void Start()
    {
        // Test nếu cần
        // TilemapDetail tilemapDetail = new TilemapDetail(1, 1, TilemapSate.Tilemap);
        // WriteUserData("123", tilemapDetail.ToString());
        // ReadRawDataForDebug("123");
    }

    // ✅ Ghi toàn bộ dữ liệu User (ví dụ: class User → ToString)
    public void WriteUserData(string id, string message)
    {
        reference.Child("Users").Child(id).SetValueAsync(message).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("✅ Ghi dữ liệu thành công.");
            }
            else
            {
                Debug.LogError("❌ Ghi dữ liệu thất bại: " + task.Exception);
            }
        });
    }

    // ✅ Ghi giá trị đơn vào path cụ thể (ví dụ: Users/userId/deviceId)
    public void WriteDataBase(string path, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            // Xóa path nếu value là null
            reference.Child(path).RemoveValueAsync();
        }
        else
        {
            reference.Child(path).SetValueAsync(value).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                    Debug.Log("✅ Ghi giá trị thành công: " + path);
                else
                    Debug.LogError("❌ Ghi giá trị thất bại: " + task.Exception);
            });
        }
    }


    // ✅ Đọc path tùy ý, trả về string (dùng cho: Users/userId/deviceId...)
    public void ReadDataBase(string path, System.Action<string> onResult)
    {
        reference.Child(path).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                string value = snapshot?.Value?.ToString();
                onResult?.Invoke(value);
            }
            else
            {
                Debug.LogError("❌ Lỗi khi đọc dữ liệu: " + task.Exception);
                onResult?.Invoke(null);
            }
        });
    }

    // ✅ Chỉ dùng test debug toàn bộ node user
    public void ReadRawDataForDebug(string id)
    {
        reference.Child("Users").Child(id).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                Debug.Log("📦 Dữ liệu user: " + snapshot.Value);
            }
            else
            {
                Debug.LogError("❌ Đọc dữ liệu thất bại: " + task.Exception);
            }
        });
    }

    // ✅ Cập nhật tên người chơi
    public void UpdateUserName(string userId, string newName)
    {
        reference.Child("Users").Child(userId).Child("Name").SetValueAsync(newName).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("✅ Đã cập nhật tên: " + newName);
            }
            else
            {
                Debug.LogError("❌ Lỗi khi cập nhật tên: " + task.Exception);
            }
        });
    }

    // ✅ Đọc tên người chơi
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
                Debug.LogError("❌ Lỗi khi đọc tên: " + task.Exception);
                onNameLoaded?.Invoke(null);
            }
        });
    }

    // ✅ Mở khóa chế độ map (mode = true)
    public void UnlockMode(string userId)
    {
        reference.Child("Users").Child(userId).Child("mode").SetValueAsync(true).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("✅ Cập nhật mode thành công");
                imageSwitcher?.UpData();
            }
            else
            {
                Debug.LogError("❌ Lỗi khi cập nhật mode: " + task.Exception);
            }
        });
    }

    // ✅ Đọc mode: true/false
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
                    bool.TryParse(snapshot.Value.ToString(), out mode);
                }

                Debug.Log("🌐 Mode hiện tại: " + mode);
                onLoaded?.Invoke(mode);
            }
            else
            {
                Debug.LogError("❌ Không thể load mode: " + task.Exception);
                onLoaded?.Invoke(false);
            }
        });
    }
}
