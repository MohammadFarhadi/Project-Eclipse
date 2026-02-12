using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public interface InterfaceEnemies
{
    int HealthPoint { get; }      
    void TakeDamage(int damage, Transform attacker);
    
    void SetHealth(int hp);            

}