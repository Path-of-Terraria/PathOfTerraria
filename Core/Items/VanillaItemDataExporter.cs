using PathOfTerraria.Common.Data.Models;
using PathOfTerraria.Common.Enums;
using System.IO;
using System.Text.Json;
using Terraria.ID;

namespace PathOfTerraria.Core.Items;

internal class VanillaItemDataExporter : ModSystem
{
	public override bool IsLoadingEnabled(Mod mod)
	{
		return false;
	}

	public override void SetStaticDefaults()
	{
		Directory.CreateDirectory("VanillaItemData");
		var options = new JsonSerializerOptions() { IncludeFields = true, WriteIndented = true };

		for (int i = 0; i < ItemID.Count; ++i)
		{
			Item item = new(i);
			ItemType type = ItemType.None;

			if (item.accessory && !item.vanity)
			{
				type = ItemType.Accessories;
			}
			else if (item.defense > 0 && !item.vanity)
			{
				if (item.headSlot > 0)
				{
					type = ItemType.Helmet;
				}
				else if (item.bodySlot > 0)
				{
					type = ItemType.Chestplate;
				}
				else if (item.legSlot > 0)
				{
					type = ItemType.Helmet;
				}
			}
			else if (item.damage > 0)
			{
				if (item.CountsAsClass<MeleeDamageClass>())
				{
					if (!item.noMelee && item.axe > 0)
					{
						type = ItemType.Battleaxe;
					}
					else if (item.shoot > ProjectileID.None && ContentSamples.ProjectilesByType[item.shoot].aiStyle == ProjAIStyleID.Spear)
					{
						type = ItemType.Spear;
					}
					else if (item.shoot > ProjectileID.None && ContentSamples.ProjectilesByType[item.shoot].aiStyle == ProjAIStyleID.Flail)
					{
						type = ItemType.MeleeFlail;
					}
					else if (!item.noMelee || item.shoot > ProjectileID.None && ContentSamples.ProjectilesByType[item.shoot].aiStyle == ProjAIStyleID.ShortSword)
					{
						type = ItemType.Sword;
					}
					else
					{
						type = ItemType.Melee;
					}
				}
				else if (item.CountsAsClass<RangedDamageClass>())
				{
					if (item.ammo == AmmoID.Bullet)
					{
						type = ItemType.Gun;
					}
					else if (item.ammo == AmmoID.Arrow)
					{
						type = ItemType.Bow;
					}
					else if (item.ammo == AmmoID.Rocket)
					{
						type = ItemType.Launcher;
					}
					else
					{
						type = ItemType.Ranged;
					}
				}
				else if (item.CountsAsClass<MagicDamageClass>())
				{
					type = ItemType.Staff;
				}
				else if (item.CountsAsClass<SummonDamageClass>())
				{
					if (item.CountsAsClass<SummonMeleeSpeedDamageClass>() && item.shoot > ProjectileID.None)
					{
						type = ItemType.Whip;
					}
					else
					{
						type = ItemType.Summoner;
					}
				}
			}

			if (type != ItemType.None)
			{
				using FileStream stream = File.OpenWrite("VanillaItemData\\" + ItemID.Search.GetName(i) + ".json");
				JsonSerializer.Serialize(stream, new VanillaItemData() { ItemType = type.ToString() }, options);
			}
		}
	}
}
