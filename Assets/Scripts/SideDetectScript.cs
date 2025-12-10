using UnityEngine;

public class SideDetectScript : MonoBehaviour
{
    DiceRollScript diceRollScript;

    void Awake()
    {
        diceRollScript = FindFirstObjectByType<DiceRollScript>();    
    }

    private void OnTriggerStay(Collider sideCollider)
    {
        if(diceRollScript != null)
        {
            if(diceRollScript.GetComponent<Rigidbody>().linearVelocity == Vector3.zero)
            {
                diceRollScript.isLanded = true;
                diceRollScript.diceFaceNum = sideCollider.name;
            }
            else
            {
                diceRollScript.isLanded = false;
            }
        }
    }
}
