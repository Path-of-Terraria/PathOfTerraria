using PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer;
using PathOfTerraria.Content.NPCs.Mapping.Forest;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Common.NPCs;

internal class DropModifierNPC : GlobalNPC
{
	internal static Dictionary<int, float> TypeDropRates = [];

	public override bool InstancePerEntity => true;

	/// <summary>
	/// Disables gear drops for this specific instance.<br/>
	/// If you need to disable drops for an entire type, use <see cref="TypeDropRates"/>.<br/>
	/// Defaults to <see cref="null"/>, which means the drop rate isn't modified (equal to 1). This is checked before the value in <see cref="TypeDropRates"/>.
	/// </summary>
	public float? InstancedDropRate = null;

	public static float GetDropRate(NPC npc)
	{
		float? dropRate = npc.GetGlobalNPC<DropModifierNPC>().InstancedDropRate;

		if (dropRate.HasValue)
		{
			return dropRate.Value;
		}

		if (TypeDropRates.TryGetValue(npc.type, out float rate))
		{
			return rate;
		}

		return 1;
	}

	public static void AddRateModifier(int type, float rate)
	{
		TypeDropRates.Add(type, rate);
	}

	public static void AddRateModifier<T>(float rate) where T : ModNPC
	{
		TypeDropRates.Add(ModContent.NPCType<T>(), rate);
	}

	public override void SetStaticDefaults()
	{
		AddRateModifier<CanopyBird>(0);
		AddRateModifier<WormLightning>(0);
		AddRateModifier(NPCID.SlimeSpiked, 0);
		AddRateModifier(NPCID.Bee, 0.33f);
		AddRateModifier(NPCID.BeeSmall, 0.33f);
		AddRateModifier(NPCID.PlanterasHook, 0f);
		AddRateModifier(NPCID.QueenSlimeMinionBlue, 0.2f);
		AddRateModifier(NPCID.QueenSlimeMinionPurple, 0.2f);
		AddRateModifier(NPCID.QueenSlimeMinionPink, 0.2f);
	}
}
