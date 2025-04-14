using PathOfTerraria.Common.Systems.MobSystem;
using Terraria.ID;

namespace PathOfTerraria.Common.NPCs;

internal class AffixFreeNPCs : GlobalNPC
{
	public override void SetStaticDefaults()
	{
		ArpgNPC.NoAffixesSet.Add(NPCID.Golem); // Most of these break if sped up or are extremely tanky
		ArpgNPC.NoAffixesSet.Add(NPCID.GolemFistLeft);
		ArpgNPC.NoAffixesSet.Add(NPCID.GolemFistRight);
		ArpgNPC.NoAffixesSet.Add(NPCID.GolemHead);
		ArpgNPC.NoAffixesSet.Add(NPCID.GolemHeadFree);
		ArpgNPC.NoAffixesSet.Add(NPCID.BoundTownSlimeOld); // All bound slimes aren't really enemies
		ArpgNPC.NoAffixesSet.Add(NPCID.BoundTownSlimePurple);
		ArpgNPC.NoAffixesSet.Add(NPCID.BoundTownSlimeYellow);
		ArpgNPC.NoAffixesSet.Add(NPCID.PrimeVice); // Buffed prime arms are needlessly tanky and difficult
		ArpgNPC.NoAffixesSet.Add(NPCID.PrimeSaw);
		ArpgNPC.NoAffixesSet.Add(NPCID.PrimeLaser);
		ArpgNPC.NoAffixesSet.Add(NPCID.PrimeCannon);
		ArpgNPC.NoAffixesSet.Add(NPCID.Creeper); // Stops the BoC healthbar from bugging
	}
}
