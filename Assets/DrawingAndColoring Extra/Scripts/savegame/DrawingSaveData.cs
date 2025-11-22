using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DrawingSaveData
{
    public int shapeIndex;
    public bool hasDrawing; // просто флаг что есть рисунок
}

// Пока сохраняем только факт наличия рисунка