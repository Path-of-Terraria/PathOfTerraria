using System.Collections.Generic;
using System.IO;
using PathOfTerraria.Common.Data;
using PathOfTerraria.Common.Data.Models;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Core.Items;
using SubworldLibrary;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Systems.MobSystem;

internal class MobAprgSystem : GlobalNPC
{
	public override bool InstancePerEntity => true;

	public int? Experience;
	public ItemRarity Rarity = ItemRarity.Normal;

	private readonly Player _lastPlayerHit = null;

	private List<MobAffix> _affixes = [];
	private bool _synced = false;

	// should somehow work together with magic find (that i assume we will have) to increase rarity / if its a unique
	private float DropRarity
	{
		get
		{
			float dropRarity = 0;
			_affixes.ForEach(a => dropRarity += a.DropRarityFlat);
			_affixes.ForEach(a => dropRarity *= a.DropRarityMultiplier);
			return dropRarity; // rounds down iirc
		}
	}

	private const float MinDropChanceScale = 0.4f;
	// if we have 3 DropQuantity, 0.4f would mean we can spawn somewhere between 1 and 3 items
	// we would take the 3 * 0.4 = 1.2
	// 1.2 * 100 = 120
	// 3 * 100 = 300
	// pick a number between 120 and 300
	// every 100% is an item and the rest is chance for another droop
	// so if we roll 120, we'd get 1 item and 20% chance for another

	private float DropQuantity // max drop amount, should probably affect min a little too
	{
		get
		{
			float dropQuantity = 1;
			_affixes.ForEach(a => dropQuantity += a.DropQuantityFlat);
			_affixes.ForEach(a => dropQuantity *= a.DropQuantityMultiplier);
			return dropQuantity;
		}
	}

	public override bool PreAI(NPC npc)
	{
		bool doRunNormalAi = true;
		_affixes.ForEach(a => doRunNormalAi = doRunNormalAi && a.PreAi(npc));
		return doRunNormalAi;
	}

	public override void AI(NPC npc)
	{
		_affixes.ForEach(a => a.Ai(npc));
	}

	public override void PostAI(NPC npc)
	{
		_affixes.ForEach(a => a.PostAi(npc));
	}

	public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		bool doDraw = true;
		_affixes.ForEach(a => doDraw = doDraw && a.PreDraw(npc, spriteBatch, screenPos, drawColor));
		return doDraw;
	}

	public override void OnKill(NPC npc)
	{
		_affixes.ForEach(a => a.OnKill(npc));

		if (npc.lifeMax <= 5 || npc.SpawnedFromStatue || npc.boss)
		{
			return;
		}

		int minDrop = (int)(DropQuantity * MinDropChanceScale * 100f);
		int maxDrop = (int)(DropQuantity * 100f);
		int rand = Main.rand.Next(minDrop, maxDrop + 1);
		float magicFind = 0;
		int itemLevel = 0;

		if (_lastPlayerHit != null)
		{
			magicFind = 1f + _lastPlayerHit.GetModPlayer<MinorStatsModPlayer>().MagicFind;
		}

		if (SubworldSystem.Current is BossDomainSubworld domain)
		{
			itemLevel = domain.DropItemLevel;
		}

		while (rand > 99)
		{
			rand -= 100;
			ItemSpawner.SpawnMobKillItem(npc.Center, itemLevel, DropRarity * magicFind);
		}

		if (rand < 25) // 10
		{
			ItemSpawner.SpawnMobKillItem(npc.Center, itemLevel, DropRarity * magicFind);
		}
	}

	public override bool PreKill(NPC npc)
	{
		bool doKill = true;
		_affixes.ForEach(a => doKill = doKill && a.PreKill(npc));
		return doKill;
	}

	public override void SetDefaultsFromNetId(NPC npc)
	{
		//We only want to trigger these changes on hostile non-boss, non Eater of Worlds mobs in-game
		if (npc.friendly || npc.boss || Main.gameMenu || npc.type is NPCID.EaterofWorldsBody or NPCID.EaterofWorldsHead or NPCID.EaterofWorldsTail)
		{
			return;
		}

		if (Main.netMode != NetmodeID.MultiplayerClient)
		{
			// Base chance is determined by item level; default is 300 - 5*4 (275).
			// By hardmode it's 300 - 50*4 (100).
			// Will need adjustment for hardmode.
			int chance = 300;
			chance -= (int)(PoTItemHelper.PickItemLevel() * 4f);

			if (chance < 16)
			{
				chance = 16;
			}

			Rarity = Main.rand.Next(chance) switch
			{
				< 2 => ItemRarity.Rare,
				< 17 => ItemRarity.Magic,
				_ => ItemRarity.Normal
			};

			ApplyRarity(npc);
			npc.netUpdate = true;
		}
	}

	public void ApplyRarity(NPC npc)
	{
		// npc.TypeName uses only the netID, which is not set by SetDefaults...for some reason.
		// This works the same, just using type if netID isn't helpful.
		string typeName = NPCLoader.ModifyTypeName(npc, Lang.GetNPCNameValue(npc.netID == 0 ? npc.type : npc.netID));

		npc.GivenName = Rarity switch
		{
			ItemRarity.Magic or ItemRarity.Rare => $"{Language.GetTextValue($"Mods.{PoTMod.ModName}.Misc.RarityNames." + Enum.GetName(Rarity))} {typeName}",
			ItemRarity.Unique => "UNIQUE MOB",
			_ => typeName
		};

		if (MobRegistry.TryGetMobData(npc.type, out MobData mobData))
		{
			MobEntry entry = MobRegistry.SelectMobEntry(mobData.NetId);

			if (entry != null)
			{
				Experience = entry.Stats.Experience;
				if (!string.IsNullOrEmpty(entry.Prefix))
				{
					npc.GivenName = $"{Language.GetTextValue($"Mods.{PoTMod.ModName}.EnemyPrefixes." + entry.Prefix)} {npc.GivenOrTypeName}";
				}

				npc.scale *= entry.Scale ?? 1f;
			}
		}
#if DEBUG
		else
		{
			Main.NewText($"Failed to load MobData for NPC ID {npc.type} ({typeName})!", Color.Red);
		}
#endif

		if (Rarity == ItemRarity.Normal || Rarity == ItemRarity.Unique)
		{
			return;
		}

		List<MobAffix> possible = AffixHandler.GetAffixes(Rarity);
		_affixes = Rarity switch
		{
			ItemRarity.Magic => Affix.GenerateAffixes(possible, PoTItemHelper.GetAffixCount(Rarity)),
			ItemRarity.Rare => Affix.GenerateAffixes(possible, PoTItemHelper.GetAffixCount(Rarity)),
			_ => []
		};

		_affixes.ForEach(a => a.PreRarity(npc));

		switch (Rarity)
		{
			case ItemRarity.Normal:
				break;
			case ItemRarity.Magic:
				npc.color = Color.Lerp(npc.color, new Color(125, 125, 255), 0.5f);
				npc.lifeMax *= 2; //Magic mobs get 100% increased life
				npc.life = npc.lifeMax + 1; //This will trigger health bar to appear
				npc.damage = (int)(npc.damage * 1.1f); //Magic mobs get 10% increase damage
				break;
			case ItemRarity.Rare:
				npc.color = Color.Lerp(npc.color, new Color(255, 255, 0), 0.5f);
				npc.lifeMax *= 3; //Rare mobs get 200% Increased Life
				npc.life = npc.lifeMax + 1; //This will trigger health bar to appear
				npc.damage = (int)(npc.damage * 1.2f); //Magic mobs get 20% increase damage
				break;
			case ItemRarity.Unique:
				break;
			default:
				throw new InvalidOperationException("Invalid rarity!");
		}

		_affixes.ForEach(a => a.PostRarity(npc));
		_synced = true;
	}

	public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
	{
		binaryWriter.Write((byte)Rarity);
	}

	public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
	{
		Rarity = (ItemRarity)binaryReader.ReadByte();

		// TODO: Find cause of read overflow/underflow in subworlds
		if (npc.life <= 0 && SubworldSystem.Current is not null)
		{
			return;
		}

		// Only apply rarity the first time the rarity is sent. 
		// This may need to be changed if we want variable rarity for some reason.
		if (Rarity != ItemRarity.Normal && !_synced)
		{
			ApplyRarity(npc);
		}
	}
}
