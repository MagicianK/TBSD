using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectedView : MonoBehaviour
{
    private static SelectedView _instance;
    public static SelectedView instance
    { get { return _instance; } }

    private void Awake() {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }
    public void Hide()
    {

    }

    public void MoveTo(Vector3 pos){
        this.transform.position = new Vector3(pos.x, pos.y + 2.5f, pos.z);
    }
}
