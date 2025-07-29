namespace PathOfTerraria.Content.Buffs;

internal class BlindedDebuff : ModBuff
{
	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;
	}
}

/// <summary> Handles miss effects for <see cref="BlindedDebuff"/>. </summary>
internal class BlindedPlayer : ModPlayer
{
	public const byte MissCooldownMax = 60;

	private int _missedTarget = -1;
	private byte _missCooldown;

	public override void ResetEffects()
	{
		if (_missCooldown > 0)
		{
			_missCooldown--;
		}
		else
		{
			_missedTarget = -1;
		}
	}

	public override bool CanHitNPC(NPC target)
	{
		if (_missedTarget == -1 && Player.HasBuff<BlindedDebuff>() && Main.rand.NextFloat() <= 0.1f)
		{
			_missedTarget = target.whoAmI;
			_missCooldown = MissCooldownMax;
		}

		return _missedTarget != target.whoAmI;
	}
}