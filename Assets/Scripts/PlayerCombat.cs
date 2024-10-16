using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerCombat : MonoBehaviour
{
    public List<AttackSO> combo; // List of combo attacks
    public InputAction attack, heavyAttack; // Input action for attack
    public int comboCounter; // Current combo step
    public bool attackPressed, heavyPressed; // Buffered input indicator
    [SerializeField] PlayerController controller;

    public Animator anim; // Reference to the Animator component
    [SerializeField] Weapon weapon; // Reference to the weapon

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (attack.WasPerformedThisFrame())
        {
            CancelInvoke("EndCombo");
            Attack();
        }
        if (heavyAttack.IsPressed() && !anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            anim.SetBool("heavyStarted", true);
            anim.SetBool("heavyHeld", true);
            anim.SetBool("heavyEnded", false);
        }
        else if (heavyAttack.IsPressed() && anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.6)
        {
            heavyPressed = true;
            StartCoroutine("WaitForHeavy");
        }
        if (heavyAttack.IsInProgress())
        {
            anim.SetBool("heavyHeld", true);
        }
        else
        {
            anim.SetBool("heavyHeld", false);
        }
        if (anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            controller.CanMove(false);
        }
        if(!anim.GetBool("heavyEnded"))
        {
            CancelInvoke("EndCombo");
        }
        ExitAttack();
        anim.SetFloat("Multiplier", 1.4f); //Adjusting animation speed
    }

    #region LightAttacks
    void Attack()
    {
        anim.SetBool("attack", true);
        if (comboCounter < combo.Count)
        {
            // If not in the middle of an attack animation, start the attack
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") && !anim.GetCurrentAnimatorStateInfo(0).IsName("HeavyAttack"))
            {
                StartAttack();
            }
            else if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f && attack.IsPressed())
            {
                // Buffer the input for the next attack
                attackPressed = true;
                StartCoroutine(WaitForAttack());
            }
        }
    }

    void StartAttack()
    {
        // Set the appropriate attack animation and properties
        anim.runtimeAnimatorController = combo[comboCounter].animatorOV;
        anim.Play("Attack", 0, 0);
        weapon.damage = combo[comboCounter].damage;
        comboCounter++;

        if (comboCounter >= combo.Count)
        {
            comboCounter = 0;
        }
    }

    IEnumerator WaitForAttack()
    {
        // Avoid multiple coroutine instances running simultaneously
        if (!attackPressed) yield break;

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") || anim.GetCurrentAnimatorStateInfo(0).IsName("HeavyAttack"))
        {
            yield return new WaitWhile(() => anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f);
        }

        if (attackPressed)
        {
            attackPressed = false; // Clear the buffer
            CancelInvoke("EndCombo");
            StartAttack(); // Execute the buffered attack
        }
    }
    IEnumerator WaitForHeavy()
    {
        // Avoid multiple coroutine instances running simultaneously
        if (!heavyPressed) yield break;

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            yield return new WaitWhile(() => anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.6f);
        }

        if (heavyPressed)
        {
            heavyPressed = false; // Clear the buffer
            CancelInvoke("EndCombo");
            anim.SetBool("heavyStarted", true);
            anim.SetBool("heavyHeld", true);
            controller.CanMove(false);
        }
    }
    void ExitAttack()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.9f && anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack") && attackPressed == false)
        {
            anim.SetBool("attack", false);
            Invoke("EndCombo", 0.5f);
        }
    }

    #endregion LightAttacks
    void EndCombo()
    {
        comboCounter = 0;
        controller.CanMove(true);
        comboCounter = 0;
    }
    private void OnEnable()
    {
        attack.Enable();
        heavyAttack.Enable();
    }

    private void OnDisable()
    {
        attack.Disable();
        heavyAttack.Disable();
    }
}
