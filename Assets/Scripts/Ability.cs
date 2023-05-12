using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : ScriptableObject
{
    [SerializeField]
    private new string name;
    
    [SerializeField]
    private float cooldownTurns;
    
}
