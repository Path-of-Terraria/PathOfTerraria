using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives;

internal class AddedIntelligencePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<AttributesPlayer>().Intelligence += Value * Level;
	}
}

internal class AddedStrengthPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<AttributesPlayer>().Strength += Value * Level;
	}
}

internal class AddedDexterityPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<AttributesPlayer>().Dexterity += Value * Level;
	}
}

internal abstract class HybridAttributePassive : Passive
{
	protected float HybridValue => Value * 0.5f * Level;

	public override string DisplayTooltip => Language.GetText($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").Format(Value * 0.5f);
}

internal class AddedIntelligenceStrengthPassive : HybridAttributePassive
{
	public override string TexturePath => $"{PoTMod.ModName}/Assets/Passives/Placeholder";

	public override void BuffPlayer(Player player)
	{
		AttributesPlayer attributesPlayer = player.GetModPlayer<AttributesPlayer>();
		float hybridValue = HybridValue;

		attributesPlayer.Intelligence += hybridValue;
		attributesPlayer.Strength += hybridValue;
	}
}

internal class AddedIntelligenceDexterityPassive : HybridAttributePassive
{
	public override string TexturePath => $"{PoTMod.ModName}/Assets/Passives/Placeholder";

	public override void BuffPlayer(Player player)
	{
		AttributesPlayer attributesPlayer = player.GetModPlayer<AttributesPlayer>();
		float hybridValue = HybridValue;

		attributesPlayer.Intelligence += hybridValue;
		attributesPlayer.Dexterity += hybridValue;
	}
}

internal class AddedDexterityStrengthPassive : HybridAttributePassive
{
	public override string TexturePath => $"{PoTMod.ModName}/Assets/Passives/Placeholder";

	public override void BuffPlayer(Player player)
	{
		AttributesPlayer attributesPlayer = player.GetModPlayer<AttributesPlayer>();
		float hybridValue = HybridValue;

		attributesPlayer.Dexterity += hybridValue;
		attributesPlayer.Strength += hybridValue;
	}
}
