namespace PathOfTerraria.Common.Systems.Charges;

public class ModChargePlayer : ModPlayer
{
	// Charge stacks
	public int Charges { get; private set; }
	public const int MaxCharges = 3;
	protected virtual int BuffType => -1;
	public bool HasAnyCharges => Charges > 0;

	//Duration
	public const int DefaultChargeDuration = 600; // 10 seconds
	private int chargeDuration;

	// Chance modifier
	public float ChargeGainChance;

	protected Color ChargeColor;
	protected string ChargeName;

	public override void PostUpdateMiscEffects()
	{
		// Update shared charge duration
		if (HasAnyCharges && chargeDuration > 0)
		{
			chargeDuration--;
            
			// Update buff duration to match charge duration
			if (BuffType != -1 && Player.HasBuff(BuffType))
			{
				Player.buffTime[Player.FindBuffIndex(BuffType)] = chargeDuration;
			}

			if (chargeDuration <= 0)
			{
				RemoveAllCharges();
			}
		}
	}

	public override void ResetEffects()
	{
		ChargeGainChance = 0;
	}
	
	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		// Chance to gain charges on kill
		if (target.life <= 0)
		{
			TryGainChargeOnKill();
		}
	}

	protected virtual void ApplyChargeModifiers(EntityModifier modifier)
	{
		//Meant to be derived in child classes
	}

	//If we ever want to add any chance to gain charge on hit, this is here.
	protected virtual void TryGainChargeOnHit()
    {

    }

    protected virtual void TryGainChargeOnKill()
    {
	    if (ChargeGainChance > 0 && Main.rand.NextFloat(1) < (ChargeGainChance/100))
	    {
		    AddCharge();
	    }
    }

    public void AddCharge(int amount = 1)
    {
	    bool hadCharges = Charges > 0;
	    Charges = Math.Min(Charges + amount, MaxCharges);
	    RefreshChargeDuration();
	    //ShowChargeGainText();
        
	    // Add or refresh the buff
	    if (BuffType != -1)
	    { 
		    Player.AddBuff(BuffType, chargeDuration);
	    }

    }

    protected void RefreshChargeDuration()
    {
        chargeDuration = DefaultChargeDuration;
    }

    protected void ShowChargeGainText()
    {
        CombatText.NewText(Player.getRect(), ChargeColor, $"+{ChargeName} Charge", true);
    }
    
    public void RemoveAllCharges()
    {
	    if (Charges > 0)
	    {
		    Charges = 0;
		    chargeDuration = 0;
            
		    // Remove the buff
		    if (BuffType != -1)
		    {
			    Player.ClearBuff(BuffType);
		    }
	    }

    }
}


