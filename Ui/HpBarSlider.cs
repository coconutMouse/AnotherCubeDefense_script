using UnityEngine;
using Minimalist.Bar.Quantity;

public class HpBarSlider : MonoBehaviour
{
    public QuantityBhv healthBar;
    public Character character;

    void Update()
    {
        if (character == null)
            return;
        if(healthBar.MaximumAmount != character.MaxHp)
            healthBar.MaximumAmount = character.MaxHp;
        if (healthBar.Amount != character.hp)
            healthBar.Amount = character.hp;
    }
}
