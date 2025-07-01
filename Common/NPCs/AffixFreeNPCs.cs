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
		ArpgNPC.NoAffixesSet.Add(NPCID.PrimeVice); // Buffed skeletron (prime) arms are needlessly tanky and difficult
		ArpgNPC.NoAffixesSet.Add(NPCID.PrimeSaw);
		ArpgNPC.NoAffixesSet.Add(NPCID.PrimeLaser);
		ArpgNPC.NoAffixesSet.Add(NPCID.PrimeCannon);
		ArpgNPC.NoAffixesSet.Add(NPCID.SkeletronHand);
		ArpgNPC.NoAffixesSet.Add(NPCID.Creeper); // Stops the BoC healthbar from bugging
		ArpgNPC.NoAffixesSet.Add(NPCID.TheDestroyerBody);
		ArpgNPC.NoAffixesSet.Add(NPCID.TheDestroyerTail);
		ArpgNPC.NoAffixesSet.Add(NPCID.TheDestroyer);
		ArpgNPC.NoAffixesSet.Add(NPCID.EaterofWorldsHead);
		ArpgNPC.NoAffixesSet.Add(NPCID.EaterofWorldsBody);
		ArpgNPC.NoAffixesSet.Add(NPCID.EaterofWorldsTail);
		ArpgNPC.NoAffixesSet.Add(NPCID.WallofFlesh); // Neither WoF body part should be buffed, works weirdly
		ArpgNPC.NoAffixesSet.Add(NPCID.WallofFleshEye);
		ArpgNPC.NoAffixesSet.Add(NPCID.Sharkron); // Both of these are effectively projectiles, should be ignored
		ArpgNPC.NoAffixesSet.Add(NPCID.Sharkron2);
		ArpgNPC.NoAffixesSet.Add(NPCID.GiantWormBody); // All of these are worms which should not be modified
		ArpgNPC.NoAffixesSet.Add(NPCID.GiantWormHead);
		ArpgNPC.NoAffixesSet.Add(NPCID.GiantWormTail);
		ArpgNPC.NoAffixesSet.Add(NPCID.WyvernBody);
		ArpgNPC.NoAffixesSet.Add(NPCID.WyvernBody2);
		ArpgNPC.NoAffixesSet.Add(NPCID.WyvernBody3);
		ArpgNPC.NoAffixesSet.Add(NPCID.WyvernHead);
		ArpgNPC.NoAffixesSet.Add(NPCID.WyvernLegs);
		ArpgNPC.NoAffixesSet.Add(NPCID.WyvernTail);
		ArpgNPC.NoAffixesSet.Add(NPCID.DevourerBody);
		ArpgNPC.NoAffixesSet.Add(NPCID.DevourerHead);
		ArpgNPC.NoAffixesSet.Add(NPCID.DevourerTail);
		ArpgNPC.NoAffixesSet.Add(NPCID.CultistDragonBody1);
		ArpgNPC.NoAffixesSet.Add(NPCID.CultistDragonBody2);
		ArpgNPC.NoAffixesSet.Add(NPCID.CultistDragonBody3);
		ArpgNPC.NoAffixesSet.Add(NPCID.CultistDragonBody4);
		ArpgNPC.NoAffixesSet.Add(NPCID.CultistDragonHead);
		ArpgNPC.NoAffixesSet.Add(NPCID.CultistDragonTail);
		ArpgNPC.NoAffixesSet.Add(NPCID.DiggerBody);
		ArpgNPC.NoAffixesSet.Add(NPCID.DiggerHead);
		ArpgNPC.NoAffixesSet.Add(NPCID.DiggerTail);
		ArpgNPC.NoAffixesSet.Add(NPCID.LeechBody);
		ArpgNPC.NoAffixesSet.Add(NPCID.LeechHead);
		ArpgNPC.NoAffixesSet.Add(NPCID.LeechTail);
		ArpgNPC.NoAffixesSet.Add(NPCID.SeekerHead);
		ArpgNPC.NoAffixesSet.Add(NPCID.SeekerBody);
		ArpgNPC.NoAffixesSet.Add(NPCID.SeekerTail);
		ArpgNPC.NoAffixesSet.Add(NPCID.MoonLordFreeEye); // All moon lord parts should be affixless
		ArpgNPC.NoAffixesSet.Add(NPCID.MoonLordHand);
		ArpgNPC.NoAffixesSet.Add(NPCID.MoonLordHead);
		ArpgNPC.NoAffixesSet.Add(NPCID.MoonLordLeechBlob);
	}
}
