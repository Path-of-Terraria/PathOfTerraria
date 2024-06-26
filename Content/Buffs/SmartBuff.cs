using Terraria.Localization;

namespace PathOfTerraria.Content.Buffs;

public abstract class SmartBuff(bool debuff, bool summon = false) : ModBuff
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Buffs/{GetType().Name}";

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

		Language.GetOrRegister("Mods.PathOfTerraria.Buffs." + GetType().Name + ".DisplayName", () => GetType().Name);
		Language.GetOrRegister("Mods.PathOfTerraria.Buffs." + GetType().Name + ".Description", () => "");
	}
}