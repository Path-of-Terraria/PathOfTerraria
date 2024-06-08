namespace PathOfTerraria.Content.Buffs;

public abstract class SmartBuff(string name, string tooltip, bool debuff, bool summon = false)
	: ModBuff
{
	private readonly string ThisName = name;
	private readonly string ThisTooltip = tooltip;

	public bool Inflicted(Player Player)
	{
		return Player.active && Player.HasBuff(Type);
	}

	public bool Inflicted(NPC NPC)
	{
		if (ModContent.GetModBuff(Type) != null && NPC.buffImmune.Length > Type)
		{
			return NPC.active && NPC.HasBuff(Type);
		}

		return false;
	}

	public virtual void SafeSetDefaults() { }

	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = debuff;

		if (summon)
		{
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}

		SafeSetDefaults();
	}
}