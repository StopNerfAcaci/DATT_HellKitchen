using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager
{
    private static int unlockedLevel = 1;

    public static int GetUnlockedLevel()
    {
        return unlockedLevel;
    }
    public static void SetUnlockedLevel(int level)
    {
        unlockedLevel = level;
    }
    public static void Unlocked()
    {
        unlockedLevel++;
    }
}
