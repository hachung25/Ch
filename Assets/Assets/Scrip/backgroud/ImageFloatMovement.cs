using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ImageFloatMovement : MonoBehaviour
{
    public List<RectTransform> imagesToMove;
    public float amplitude = 10f;      // Biên độ dao động
    public float speed = 1f;           // Tốc độ dao động
    public float offsetBetweenImages = 0.5f;  // Độ lệch thời gian giữa các ảnh

    private Vector2[] initialPositions;

    void Start()
    {
        // Lưu vị trí ban đầu của từng ảnh
        initialPositions = new Vector2[imagesToMove.Count];
        for (int i = 0; i < imagesToMove.Count; i++)
        {
            initialPositions[i] = imagesToMove[i].anchoredPosition;
        }
    }

    void Update()
    {
        float time = Time.time;
        for (int i = 0; i < imagesToMove.Count; i++)
        {
            Vector2 pos = initialPositions[i];
            pos.y += Mathf.Sin(time * speed + i * offsetBetweenImages) * amplitude;
            imagesToMove[i].anchoredPosition = pos;
        }
    }
}
