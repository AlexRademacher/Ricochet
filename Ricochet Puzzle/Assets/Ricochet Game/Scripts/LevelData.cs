using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelData : MonoBehaviour
{
    public int shotCount;
    public int currShots;

    public bool updateShots() {
        currShots++;
        if (currShots > shotCount)
        {
            //To-Do: UI for Failure, also have it check for no more targets in the if statement
            return false;
        }
        return true;
    }
}
