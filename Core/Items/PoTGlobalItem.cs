using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core.Items.Hooks;
using PathOfTerraria.Core.Systems;
using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Core.Systems.ModPlayers;
using PathOfTerraria.Core.Systems.VanillaInterfaceSystem;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Core.Items;

/// <summary>
///		Per-type item data such as fields normally set in
///		<see cref="ModType.SetStaticDefaults"/>.
/// </summary>
internal sealed class PoTGlobalItem : GlobalItem
{
	private static Dictionary<int, PoTStaticItemData> _staticData = [];

	public override void Load()
	{
		On_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += DrawSpecial;
	}

	public override void Unload()
	{
		_staticData.Clear();
		_staticData = null;
	}

	// IMPORTANT: Called *after* ModItem::SetDefaults.
	// https://github.com/tModLoader/tModLoader/blob/1.4.4/patches/tModLoader/Terraria/ModLoader/Core/GlobalLoaderUtils.cs#L20
	public override void SetDefaults(Item entity)
	{
		base.SetDefaults(entity);

		Roll(entity, PickItemLevel());
	}

	public void Roll(Item item, int itemLevel)
	{
		PoTInstanceItemData data = item.GetInstanceData();
		PoTStaticItemData staticData = item.GetStaticData();

		data.ItemLevel = itemLevel;

		// Only level 50+ gear can get influence.
		if (data.ItemLevel > 50 && !staticData.IsUnique && (data.ItemType & ItemType.AllGear) == ItemType.AllGear)
		{
			// Quality does not affect influence right now.
			// Might not need to, seems to generaet plenty often late game.
			int inf = Main.rand.Next(400) - data.ItemLevel;

			if (inf < 30)
			{
				data.Influence = Main.rand.NextBool() ? Influence.Solar : Influence.Lunar;
			}
		}

		RollAffixes();

		if (item.TryGetInterface(out IPostRollItem postRollItem))
		{
			postRollItem.PostRoll();
		}

		data.SpecialName = GenerateName();
	}

	public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
	{
		base.ModifyTooltips(item, tooltips);

		tooltips.Clear();

		PoTInstanceItemData data = item.GetInstanceData();

		var nameLine = new TooltipLine(Mod, "Name", data.SpecialName)
		{
			OverrideColor = GetRarityColor(data.Rarity)
		};
		tooltips.Add(nameLine);

		var rarityLine = new TooltipLine(Mod, "Rarity", GetDescriptor(data.ItemType, data.Rarity, data.Influence))
		{
			OverrideColor = Color.Lerp(GetRarityColor(data.Rarity), Color.White, 0.5f)
		};
		tooltips.Add(rarityLine);

		var itemLevelLine = new TooltipLine(Mod, "ItemLevel", $" {(data.ItemType == ItemType.Map ? "Tier" : "Item level")}: [c/CCCCFF:{data.ItemLevel}]")
		{
			OverrideColor = new Color(170, 170, 170)
		};
		tooltips.Add(itemLevelLine);

		if (!string.IsNullOrWhiteSpace(data.AltUseDescription))
		{
			tooltips.Add(new TooltipLine(Mod, "AltUseDescription", data.AltUseDescription));
		}

		if (item.damage > 0)
		{
			// TODO: Slice first space in damage type display name...
			string highlightNumbers = HighlightNumbers(
				$"[{Math.Round(item.damage * 0.8f, 2)}-{Math.Round(item.damage * 1.2f, 2)}] Damage ({item.DamageType.DisplayName})",
				baseColor: "DDDDDD");
			var damageLine = new TooltipLine(Mod, "Damage", $"[i:{ItemID.SilverBullet}] ");
			tooltips.Add(damageLine);
		}

		if (item.defense > 0)
		{
			var defenseLine = new TooltipLine(Mod, "Defense", $"[i:{ItemID.SilverBullet}] " + HighlightNumbers($"+{item.defense} Defense", baseColor: "DDDDDD"));
			tooltips.Add(defenseLine);
		}

		for (int i = 0; i < data.Affixes.Count; i++)
		{
			ItemAffix affix = data.Affixes[i];
			string text = affix.RequiredInfluence switch
			{
				Influence.Solar => $"[i:{ItemID.IchorBullet}] " +
								   HighlightNumbers($"{affix.GetTooltip(item)}", "FFEE99", "CCB077"),
				Influence.Lunar => $"[i:{ItemID.CrystalBullet}] " +
								   HighlightNumbers($"{affix.GetTooltip(item)}", "BBDDFF", "99AADD"),
				_ => i < data.ImplicitCount
				? $"[i:{ItemID.SilverBullet}] " + HighlightNumbers($"{affix.GetTooltip(item)}", baseColor: "8B8000")
				: $"[i:{ItemID.MusketBall}] " + HighlightNumbers($"{affix.GetTooltip(item)}"),
			};

			var affixLine = new TooltipLine(Mod, $"Affix{i}", text);
			tooltips.Add(affixLine);
		}

		if (!string.IsNullOrWhiteSpace(data.Description))
		{
			tooltips.Add(new TooltipLine(Mod, "Description", data.Description));
		}

		// Change in stats if equipped.
		var thisItemModifier = new EntityModifier();
		ApplyAffixes(item, data.Affixes, thisItemModifier);

		if (item.TryGetInterface(out IInsertAdditionalTooltipLinesItem insertAdditionalTooltipLinesItem))
		{
			insertAdditionalTooltipLinesItem.InsertAdditionalTooltipLines(item, tooltips, thisItemModifier);
		}

		var currentItemModifier = new EntityModifier();
		if (item.TryGetInterface(out ISwapItemModifiersItem swapItemModifiersItem))
		{
			swapItemModifiersItem.SwapItemModifiers(item, currentItemModifier);
		}

		List<string> red = [];
		List<string> green = [];
		currentItemModifier.GetDifference(thisItemModifier).ForEach(s =>
		{
			if (s.Item2)
			{
				green.Add(s.Item1);
			}
			else
			{
				red.Add(s.Item1);
			}
		});

		if (red.Count + green.Count > 0)
		{
			tooltips.Add(new TooltipLine(Mod, "Space", " "));
		}

		int changeCount = 0;

		foreach (string changes in green)
		{
			tooltips.Add(new TooltipLine(Mod, $"Change{changeCount++}", $"[c/00FF00:{changes}]"));
		}

		foreach (string changes in red)
		{
			tooltips.Add(new TooltipLine(Mod, $"Change{changeCount++}", $"[c/FF0000:{changes}]"));
		}
	}

	public override void UpdateEquip(Item item, Player player)
	{
		base.UpdateEquip(item, player);

		ApplyAffixes(item, item.GetInstanceData().Affixes, player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier);
	}

	public void ApplyAffixes(Item item, List<ItemAffix> affixes, EntityModifier entityModifier)
	{
		foreach (ItemAffix affix in affixes)
		{
			affix.ApplyAffix(entityModifier, item);
		}
	}

	public void ClearAffixes(List<ItemAffix> affixes)
	{
		affixes.Clear();
	}

	public static PoTStaticItemData GetStaticData(int type)
	{
		if (_staticData.TryGetValue(type, out PoTStaticItemData data))
		{
			return data;
		}

		return _staticData[type] = new PoTStaticItemData();
	}

	private static void DrawSpecial(On_ItemSlot.orig_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color orig, SpriteBatch sb,
		Item[] inv, int context, int slot, Vector2 position, Color color)
	{
		if (inv[slot].ModItem is Gear gear && context != 21)
		{
			PoTInstanceItemData data = gear.GetInstanceData();

			string rareName = data.Rarity switch
			{
				Rarity.Normal => "Normal",
				Rarity.Magic => "Magic",
				Rarity.Rare => "Rare",
				Rarity.Unique => "Unique",
				_ => "Normal"
			};

			Texture2D back = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/Slots/{rareName}Back")
				.Value;
			Color backcolor = Color.White * 0.75f;

			sb.Draw(back, position, null, backcolor, 0f, default, Main.inventoryScale, SpriteEffects.None, 0f);
			ItemSlot.Draw(sb, ref inv[slot], 21, position);

			if (data.Influence == Influence.Solar)
			{
				DrawSolarSlot(sb, position);
			}

			if (data.Influence == Influence.Lunar)
			{
				DrawLunarSlot(sb, position);
			}
		}
		else
		{
			orig(sb, inv, context, slot, position, color);
		}
	}

	/// <summary>
	/// Draws the shader overlay for solar items
	/// </summary>
	/// <param name="spriteBatch"></param>
	/// <param name="pos"></param>
	private static void DrawSolarSlot(SpriteBatch spriteBatch, Vector2 pos)
	{
		Texture2D tex = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/Slots/SlotMap").Value;

		Effect effect = Filters.Scene["ColoredFire"].GetShader().Shader;

		if (effect is null)
		{
			return;
		}

		effect.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.015f % 2f);
		effect.Parameters["primary"].SetValue(new Vector3(1, 1, 0.2f) * 0.7f);
		effect.Parameters["primaryScaling"].SetValue(new Vector3(1, 1.3f, 1));
		effect.Parameters["secondary"].SetValue(new Vector3(0.85f, 0.6f, 0.35f) * 0.7f);

		effect.Parameters["sampleTexture"]
			.SetValue(ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/Misc/SwirlNoise").Value);
		effect.Parameters["mapTexture"]
			.SetValue(ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/Misc/SwirlNoise").Value);

		spriteBatch.End();
		spriteBatch.Begin(default, BlendState.Additive, default, default, RasterizerState.CullNone, effect,
			Main.UIScaleMatrix);

		spriteBatch.Draw(tex, pos, null, Color.White, 0, Vector2.Zero, Main.inventoryScale, 0, 0);

		spriteBatch.End();
		spriteBatch.Begin(default, default, default, default, RasterizerState.CullNone, default,
			Main.UIScaleMatrix);
	}

	/// <summary>
	/// Draws the shader overlay for lunar items
	/// </summary>
	/// <param name="spriteBatch"></param>
	/// <param name="pos"></param>
	private static void DrawLunarSlot(SpriteBatch spriteBatch, Vector2 pos)
	{
		Texture2D tex = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/Slots/SlotMap").Value;

		Effect effect = Filters.Scene["LunarEffect"].GetShader().Shader;

		if (effect is null)
		{
			return;
		}

		effect.Parameters["u_time"].SetValue(Main.GameUpdateCount * 0.006f % 2f);
		effect.Parameters["primary"].SetValue(new Vector3(0.4f, 0.8f, 1f) * 0.7f);
		effect.Parameters["primaryScaling"].SetValue(new Vector3(1, 1.1f, 1));
		effect.Parameters["secondary"].SetValue(new Vector3(0.4f, 0.4f, 0.9f) * 0.7f);

		effect.Parameters["sampleTexture"]
			.SetValue(ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/Misc/ShaderNoise").Value);
		effect.Parameters["mapTexture"]
			.SetValue(ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/Misc/ShaderNoise").Value);

		spriteBatch.End();
		spriteBatch.Begin(default, BlendState.Additive, default, default, RasterizerState.CullNone, effect,
			Main.UIScaleMatrix);

		spriteBatch.Draw(tex, pos, null, Color.White, 0, Vector2.Zero, Main.inventoryScale, 0, 0);

		spriteBatch.End();
		spriteBatch.Begin(default, default, default, default, RasterizerState.CullNone, default,
			Main.UIScaleMatrix);
	}

	private static string HighlightNumbers(string input, string numColor = "CCCCFF", string baseColor = "A0A0A0")
	{
		// TODO: Use regex source generator
		var regex = new Regex(@"(\d+)|(\D+)");

		return regex.Replace(input, match =>
		{
			if (match.Groups[1].Success)
			{
				return $"[c/{numColor}:{match.Value}]";
			}

			if (match.Groups[2].Success)
			{
				return $"[c/{baseColor}:{match.Value}]";
			}

			return match.Value;
		});
	}

	private static Color GetRarityColor(Rarity rarity)
	{
		return rarity switch
		{
			Rarity.Normal => Color.White,
			Rarity.Magic => new Color(110, 160, 255),
			Rarity.Rare => new Color(255, 255, 50),
			Rarity.Unique => new Color(25, 255, 25),
			_ => Color.White,
		};
	}

	private static string GetDescriptor(ItemType type, Rarity rare, Influence influence)
	{
		string typeName = type switch
		{
			ItemType.Sword => "Sword",
			ItemType.Spear => "Spear",
			ItemType.Bow => "Bow",
			ItemType.Gun => "Guns",
			ItemType.Staff => "Staff",
			ItemType.Tome => "Tome",
			ItemType.Helmet => "Helmet",
			ItemType.Chestplate => "Chestplate",
			ItemType.Leggings => "Leggings",
			ItemType.Ring => "Ring",
			ItemType.Charm => "Charm",
			_ => ""
		};

		string rareName = rare switch
		{
			Rarity.Normal => "",
			Rarity.Magic => "Magic ",
			Rarity.Rare => "Rare ",
			Rarity.Unique => "Unique ",
			_ => ""
		};

		string influenceName = influence switch
		{
			Influence.None => "",
			Influence.Solar => "Solar ",
			Influence.Lunar => "Lunar ",
			_ => ""
		};

		return $" {influenceName}{rareName}{typeName}";
	}
}
