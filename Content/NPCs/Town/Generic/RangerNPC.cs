using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.OverheadDialogue;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.NPCs.Town.Generic;

internal class RangerNPC : PlayerContainerNPC, ITavernNPC, IOverheadDialogueNPC
{
	private GenericNPCPersonalities Personality;

	OverheadDialogueInstance IOverheadDialogueNPC.CurrentDialogue { get; set; }

	public override void SetStaticDefaults()
	{
		NPCID.Sets.NoTownNPCHappiness[Type] = true;

		Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Guide];
	}

	public override void Defaults()
	{
		NPC.CloneDefaults(NPCID.Guide);
		NPC.defense = 60;
	}

	protected override void InitializePlayer()
	{
		new PlayerColors
		{
			Eyes = Main.rand.Next(6) switch
			{
				0 => Color.Red,
				1 => Color.White,
				2 => Color.Blue,
				3 => Color.Brown,
				4 => Color.Black,
				_ => Color.Pink,
			},
			Skin = Main.hslToRgb(0.05f, Main.rand.NextFloat(0.4f, 0.6f), Main.rand.NextFloat(0.4f, 0.6f)),
			Shirt = Color.Gold,
		}.SetColors(DrawDummy);

		Personality = (GenericNPCPersonalities)Main.rand.Next((int)GenericNPCPersonalities.Count);

		DrawDummy.Male = false;

		(int head, int body, int legs) = Main.rand.Next(8) switch
		{
			0 => (ItemID.CobaltMask, ItemID.CobaltBreastplate, ItemID.CobaltLeggings),
			1 => (ItemID.MythrilHood, ItemID.MythrilChainmail, ItemID.MythrilGreaves),
			2 => (ItemID.AdamantiteMask, ItemID.AdamantiteBreastplate, ItemID.AdamantiteLeggings),
			3 => (ItemID.PalladiumMask, ItemID.PalladiumBreastplate, ItemID.PalladiumLeggings),
			4 => (ItemID.OrichalcumMask, ItemID.OrichalcumBreastplate, ItemID.OrichalcumLeggings),
			5 => (ItemID.TitaniumMask, ItemID.TitaniumBreastplate, ItemID.TitaniumLeggings),
			6 => (ItemID.HallowedMask, ItemID.HallowedPlateMail, ItemID.HallowedGreaves),
			_ => (ItemID.ShroomiteHelmet, ItemID.FrostBreastplate, ItemID.FrostLeggings),
		};

		if (head == ItemID.ShroomiteHelmet && Main.rand.NextBool(2, 3))
		{
			head = Main.rand.NextBool() ? ItemID.ShroomiteMask : ItemID.ShroomiteHeadgear; 
		}

		DrawDummy.armor[0] = new Item(head);
		DrawDummy.armor[1] = new Item(body);
		DrawDummy.armor[2] = new Item(legs);

		short firstAcc = GetRandomAccessoryType();
		DrawDummy.armor[3] = new Item(firstAcc);

		short secondAcc = GetRandomAccessoryType(false);

		while (firstAcc == secondAcc)
		{
			secondAcc = GetRandomAccessoryType(false);
		}

		DrawDummy.armor[4] = new Item(secondAcc);
	}

	private static short GetRandomAccessoryType(bool canHaveWings = true)
	{
		return Main.rand.Next(canHaveWings ? 10 : 7) switch
		{
			0 => ItemID.MoltenQuiver,
			1 => ItemID.LightningBoots,
			2 => ItemID.StarVeil,
			3 => ItemID.MagicQuiver,
			4 => ItemID.BlizzardinaBottle,
			5 => ItemID.SandstorminaBottle,
			6 => ItemID.Tabi,
			7 => ItemID.FrozenWings,
			8 => ItemID.RedsWings,
			_ => ItemID.FlameWings,
		};
	}

	protected override void PreDrawPlayer()
	{
	}

	public override string GetChat()
	{
		return Language.GetTextValue($"Mods.{PoTMod.ModName}.NPCs.WarriorNPC.Dialogue." + Personality + "." + Main.rand.Next(4));
	}

	public override List<string> SetNPCNameList()
	{
		List<string> list = [];

		for (int i = 0; i < 18; ++i)
		{
			list.Add(Language.GetTextValue($"Mods.{PoTMod.ModName}.NPCs.WarriorNPC.Names." + (DrawDummy.Male ? "Male" : "Female") + "." + i));
		}

		return list;
	}

	public override bool CanTownNPCSpawn(int numTownNPCs)
	{
		return true; //Tavern NPCs can only move into Tavern rooms
	}

	public float SpawnChanceInTavern()
	{
		return 0.3f;
	}

	public string GetDialogue()
	{
		return Language.GetTextValue($"Mods.{PoTMod.ModName}.NPCs.WarriorNPC.BubbleDialogue." + Personality + "." + Main.rand.Next(4));
	}
}
