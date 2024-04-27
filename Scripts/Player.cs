using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Player stats
    public float health;
    public float damage;

    public Transform menuPlace;

    public void TakeDamage(float incomingDamage)
    {
        health -= incomingDamage;

        if (health <= 0 )
        {
            Die();
        }
    }

    public void Attack()
    {

    }

    public void Die()
    {
        Debug.Log("Player died");

        GoToMenu();
    }

    private void GoToMenu()
    {
        transform.position = menuPlace.position;
        ChooseOption();
    }

    //Choosing the option for the game (Restart, Quit)
    private void ChooseOption()
    {

    }
}
