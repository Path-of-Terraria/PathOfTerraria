using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.ElementalDamage;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Affixes.MobTypes;

internal class ChaoticAffix : MobAffix
{
	public override ItemRarity MinimumRarity => ItemRarity.Magic;

	public override void PostRarity(NPC npc)
	{
		ref ElementalDamage.ElementalDamage damageModifier = ref npc.GetGlobalNPC<ElementalNPC>().Container[ElementType.Chaos].DamageModifier;
		damageModifier = damageModifier.AddModifiers(10, 1f);
		npc.color = Color.Lerp(npc.color == Color.Transparent ? Color.White : npc.color, new Color(72, 139, 179), 0.25f);
	}

	public override void AI(NPC npc)
	{
		Color baseColor = ContentSamples.NpcsByNetId[npc.netID].color;
		npc.color = Color.Lerp(baseColor == Color.Transparent ? Color.White : baseColor, GetChaosColorGradienting(), 1f);
	}

	private static Color GetChaosColorGradienting()
	{
		float factor = MathF.Sin(Main.GameUpdateCount * 0.03f) * 0.5f + 0.5f;
		return Color.Lerp(Color.Lerp(Main.hslToRgb(new Vector3(0.6f, 0.25f, 0.5f)), Main.hslToRgb(new Vector3(0.4f, 0.25f, 0.75f)), factor), 
			Color.Lerp(Main.hslToRgb(new Vector3(0.4f, 0.25f, 0.6f)), Main.hslToRgb(new Vector3(0.2f, 0.25f, 1f)), factor), factor);
	}
}
