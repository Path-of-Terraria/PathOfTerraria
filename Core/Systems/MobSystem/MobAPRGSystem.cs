﻿using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Core.Systems.ModPlayers;
using PathOfTerraria.Data;
using PathOfTerraria.Data.Models;
using System.Collections.Generic;
using Terraria.DataStructures;

namespace PathOfTerraria.Core.Systems.MobSystem;

internal class MobAprgSystem : GlobalNPC
{
	private List<MobAffix> _affixes = [];
	private readonly Player _lastPlayerHit = null;
	public int? Experience;
	public override bool InstancePerEntity => true;
	public Rarity Rarity = Rarity.Magic;

	private float DropRarity
	// should somehow work together with magic find (that i assume we will have) to increase rarity / if its a unique
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
		if (_lastPlayerHit != null)
		{
			magicFind = 1f + _lastPlayerHit.GetModPlayer<MinorStatsModPlayer>().MagicFind;
		}

		while (rand > 99)
		{
			rand -= 100;
			PoTItem.SpawnRandomItem(npc.Center, dropRarityModifier: DropRarity * magicFind);
		}

		if (rand < 25) // 10
		{
			PoTItem.SpawnRandomItem(npc.Center, dropRarityModifier: DropRarity * magicFind);
		}
	}

	public override bool PreKill(NPC npc)
	{
		bool doKill = true;
		_affixes.ForEach(a => doKill = doKill && a.PreKill(npc));
		return doKill;
	}

	public override void OnSpawn(NPC npc, IEntitySource source)
	{
		if (npc.friendly || npc.boss) //We only want to trigger these changes on hostile non-boss mobs
		{
			return;
		}
		
		Rarity = Main.rand.Next(100) switch
		{
			<2 => Rarity.Rare, //2% Rare
			<17 => Rarity.Magic, //15% Magic 
			_ => Rarity.Normal,
		};
		npc.GivenName = Rarity switch
		{
			Rarity.Magic or Rarity.Rare => $"{Enum.GetName(Rarity)} - {npc.GivenOrTypeName}",
			Rarity.Unique => "UNIQUE MOB",
			_ => npc.GivenName
		};

		MobData mobData = MobRegistry.TryGetMobData(npc.type);
		if (mobData != null)
		{
			MobEntry entry = MobRegistry.SelectMobEntry(mobData.NetId);
			if (entry != null)
			{
				Experience = entry.Stats.Experience;
				if (!string.IsNullOrEmpty(entry.Prefix))
				{
					npc.GivenName = $"{entry.Prefix} - {npc.GivenOrTypeName}";
				}

				if (entry.Scale != null)
				{
					npc.scale *= (float) entry.Scale;	
				}
			}
		}

		if (Rarity == Rarity.Normal || Rarity == Rarity.Unique)
		{
			return;
		}

		List<MobAffix> possible = AffixHandler.GetAffixes(Rarity);
		_affixes = Rarity switch
		{
			Rarity.Magic => Affix.GenerateAffixes(possible, Main.rand.Next(1, 3)),
			Rarity.Rare => Affix.GenerateAffixes(possible, Main.rand.Next(2, 5)),
			_ => []
		};

		_affixes.ForEach(a => a.PreRarity(npc));

		switch (Rarity)
		{
			case Rarity.Normal:
				break;
			case Rarity.Magic:
				npc.color = new Color(0, 0, 255);
				npc.lifeMax *= 2; //Magic mobs get 100% increased life
				npc.life = npc.lifeMax + 1; //This will trigger health bar to appear
				npc.damage = Convert.ToInt32(npc.damage * 1.1); //Magic mobs get 10% increase damage
				break;
			case Rarity.Rare:
				npc.color = new Color(255, 255, 0);
				npc.lifeMax *= 3; //Rare mobs get 200% Increased Life
				npc.life = npc.lifeMax + 1; //This will trigger health bar to appear
				npc.damage = Convert.ToInt32(npc.damage * 1.2); //Magic mobs get 20% increase damage
				break;
			case Rarity.Unique:
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}

		_affixes.ForEach(a => a.PostRarity(npc));
	}
}