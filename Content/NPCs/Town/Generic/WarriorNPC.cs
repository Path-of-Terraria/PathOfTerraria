using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.NPCs.OverheadDialogue;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Town.Generic;

internal class WarriorNPC : PlayerContainerNPC, ITavernNPC, IOverheadDialogueNPC
{
	OverheadDialogueInstance IOverheadDialogueNPC.CurrentDialogue { get; set; }

	private GenericNPCPersonalities Personality;

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

		// Sword unused for the moment
		(int head, int body, int legs, int sword) = Main.rand.Next(8) switch
		{
			0 => (ItemID.CobaltHelmet, ItemID.CobaltBreastplate, ItemID.CobaltLeggings, ItemID.CobaltSword),
			1 => (ItemID.MythrilHelmet, ItemID.MythrilChainmail, ItemID.MythrilGreaves, ItemID.MythrilSword),
			2 => (ItemID.AdamantiteHelmet, ItemID.AdamantiteBreastplate, ItemID.AdamantiteLeggings, ItemID.AdamantiteSword),
			3 => (ItemID.PalladiumHelmet, ItemID.PalladiumBreastplate, ItemID.PalladiumLeggings, ItemID.PalladiumSword),
			4 => (ItemID.OrichalcumHelmet, ItemID.OrichalcumBreastplate, ItemID.OrichalcumLeggings, ItemID.OrichalcumSword),
			5 => (ItemID.TitaniumHelmet, ItemID.TitaniumBreastplate, ItemID.TitaniumLeggings, ItemID.TitaniumSword),
			6 => (ItemID.HallowedHelmet, ItemID.HallowedPlateMail, ItemID.HallowedGreaves, ItemID.Excalibur),
			_ => (ItemID.FrostHelmet, ItemID.FrostBreastplate, ItemID.FrostLeggings, ItemID.Frostbrand),
		};

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
			0 => ItemID.EoCShield,
			1 => ItemID.LightningBoots,
			2 => ItemID.CobaltShield,
			3 => ItemID.FeralClaws,
			4 => ItemID.BlizzardinaBottle,
			5 => ItemID.SandstorminaBottle,
			6 => ItemID.Tabi,
			7 => ItemID.FairyWings,
			8 => ItemID.JimsWings,
			_ => ItemID.BatWings,
		};
	}

	public override string GetChat()
	{
		return this.GetLocalizedValue("Dialogue." + Personality + "." + Main.rand.Next(4));
	}

	public override List<string> SetNPCNameList()
	{
		List<string> list = [];

		for (int i = 0; i < 18; ++i)
		{
			list.Add(this.GetLocalizedValue("Names." + (DrawDummy.Male ? "Male" : "Female") + "." + i));
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
		return this.GetLocalizedValue("BubbleDialogue." + Personality + "." + Main.rand.Next(4));
	}
}
