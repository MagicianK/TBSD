using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    public Vector2Int coord;
    private static Cursor _instance;
    
    public static Cursor instance
    { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public void StateCursorAt(Vector3 coord, Vector2Int coord2d)
    {
        this.transform.position = coord;
        this.coord = coord2d;
    }
}
