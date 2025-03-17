using PathOfTerraria.Common.Systems.MobSystem;
using Terraria.ID;

namespace PathOfTerraria.Common.NPCs;

internal class AffixFreeNPCs : GlobalNPC
{
	public override void SetStaticDefaults()
	{
		ArpgNPC.NoAffixesSet.Add(NPCID.Golem);
		ArpgNPC.NoAffixesSet.Add(NPCID.GolemFistLeft);
		ArpgNPC.NoAffixesSet.Add(NPCID.GolemFistRight);
		ArpgNPC.NoAffixesSet.Add(NPCID.GolemHead);
		ArpgNPC.NoAffixesSet.Add(NPCID.GolemHeadFree);
		ArpgNPC.NoAffixesSet.Add(NPCID.BoundTownSlimeOld);
		ArpgNPC.NoAffixesSet.Add(NPCID.BoundTownSlimePurple);
		ArpgNPC.NoAffixesSet.Add(NPCID.BoundTownSlimeYellow);
		ArpgNPC.NoAffixesSet.Add(NPCID.PrimeVice);
		ArpgNPC.NoAffixesSet.Add(NPCID.PrimeSaw);
		ArpgNPC.NoAffixesSet.Add(NPCID.PrimeLaser);
		ArpgNPC.NoAffixesSet.Add(NPCID.PrimeCannon);
	}
}
