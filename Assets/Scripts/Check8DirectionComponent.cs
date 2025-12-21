using UnityEngine;

public class Check8DirectionComponent : MonoBehaviour
{
    MAP_STATE[,] curStateArea = new MAP_STATE[3, 3];
    
    void Start()
    {
            
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Update8Direction(Vector3Int curPos)
    {
        //MAP_STATE dest_prev;
        curStateArea[1, 1] = MAP_STATE.PLAYER_POS;

        int cur_y = curPos.y;
        int cur_x = curPos.x;


        for (int i = 0; i < 3; i++)
        {
            //curStateArea[0, i] = 
        }
    }
}
