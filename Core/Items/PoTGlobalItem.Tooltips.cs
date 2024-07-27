using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.UI;
using System.Text.RegularExpressions;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.Affixes;

namespace PathOfTerraria.Core.Items;

// Handles modifying and rendering tooltips.

partial class PoTGlobalItem
{
	private sealed class Patcher : ILoadable
	{
		void ILoadable.Load(Mod mod)
		{
			On_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += DrawSpecial;
		}

		void ILoadable.Unload() { }
	}

	#region Modify tooltips and rendering
	// TODO: Our scaling causes the tooltip box to be drawn with extra empty
	// space, this is not something we can remedy on our side without hacky IL
	// edits and similar.  Pending: TML-2449, TML-3546; it's been years with no
	// resolution so I'm not holding my breath on this being implemented
	// anytime some.  If we get close to release, I (Tomat) can write an IL
	// edit to fix behavior for our lines.

	public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
	{
		// Don't mess with tooltip lines that we aren't responsible for.
		if (line.Mod != Mod.Name)
		{
			return true;
		}

		switch (line.Name)
		{
			case "Name":
				yOffset = -2;
				line.BaseScale = new Vector2(1.1f);
				return true;

			case "Rarity":
				yOffset = -8;
				line.BaseScale = new Vector2(0.8f);
				return true;

			case "ItemLevel":
				yOffset = 2;
				line.BaseScale = new Vector2(0.8f);
				return true;

			case "AltUseDescription":
			case "Description":
				yOffset = 2;
				line.BaseScale = new Vector2(0.8f);
				return true;
		}

		if (line.Name.Contains("Affix") || line.Name.Contains("Socket") || line.Name == "Damage" || line.Name == "Defense")
		{
			line.BaseScale = new Vector2(0.95f);
			yOffset = line.Name == "Damage" || line.Name == "Defense" ? 2 : -4;
			return true;
		}

		if (line.Name == "Space")
		{
			line.BaseScale *= 0f;
			yOffset = -20;
			return true;
		}

		if (line.Name.Contains("Change"))
		{
			line.BaseScale = new Vector2(0.75f);
			yOffset = -10;
			return true;
		}

		return base.PreDrawTooltipLine(item, line, ref yOffset);
	}

	public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
	{
		base.ModifyTooltips(item, tooltips);

		tooltips.Clear();

		PoTInstanceItemData data = item.GetInstanceData();
		PoTStaticItemData staticData = item.GetStaticData();

		var nameLine = new TooltipLine(Mod, "Name", data.SpecialName)
		{
			OverrideColor = GetRarityColor(data.Rarity)
		};
		tooltips.Add(nameLine);

		string rarityDesc = GetDescriptor(data.ItemType, data.Rarity, data.Influence);
		if (!string.IsNullOrWhiteSpace(rarityDesc))
		{
			var rarityLine = new TooltipLine(Mod, "Rarity", rarityDesc)
			{
				OverrideColor = Color.Lerp(GetRarityColor(data.Rarity), Color.White, 0.5f)
			};
			tooltips.Add(rarityLine);
		}

		var itemLevelLine = new TooltipLine(Mod, "ItemLevel", $" {(data.ItemType == ItemType.Map ? "Tier" : "Item level")}: [c/CCCCFF:{GetItemLevel.Invoke(item)}]")
		{
			OverrideColor = new Color(170, 170, 170)
		};
		tooltips.Add(itemLevelLine);

		if (!string.IsNullOrWhiteSpace(staticData.AltUseDescription))
		{
			tooltips.Add(new TooltipLine(Mod, "AltUseDescription", staticData.AltUseDescription));
		}

		if (item.damage > 0)
		{
			// TODO: Slice first space in damage type display name...
			string highlightNumbers = HighlightNumbers(
				$"[{Math.Round(item.damage * 0.8f, 2)}-{Math.Round(item.damage * 1.2f, 2)}] Damage ({item.DamageType.DisplayName})",
				baseColor: "DDDDDD");
			var damageLine = new TooltipLine(Mod, "Damage", $"[i:{ItemID.SilverBullet}] {highlightNumbers}");
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

		if (!string.IsNullOrWhiteSpace(staticData.Description))
		{
			tooltips.Add(new TooltipLine(Mod, "Description", staticData.Description));
		}

		// Change in stats if equipped.
		var thisItemModifier = new EntityModifier();
		PoTItemHelper.ApplyAffixes(item, thisItemModifier);
		InsertAdditionalTooltipLines.Invoke(item, tooltips, thisItemModifier);

		var currentItemModifier = new EntityModifier();
		SwapItemModifiers.Invoke(item, currentItemModifier);

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

	private static string HighlightNumbers(string input, string numColor = "CCCCFF", string baseColor = "A0A0A0")
	{
		Regex regex = NumberHighlightRegex();

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

	private static Color GetRarityColor(ItemRarity rarity)
	{
		return rarity switch
		{
			ItemRarity.Normal => Color.White,
			ItemRarity.Magic => new Color(110, 160, 255),
			ItemRarity.Rare => new Color(255, 255, 50),
			ItemRarity.Unique => new Color(25, 255, 25),
			_ => Color.White,
		};
	}

	private static string GetDescriptor(ItemType type, ItemRarity rare, Influence influence)
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
			ItemType.Boomerang => "Boomerand",
			ItemType.MeleeFlail => "Flail (Melee)",
			ItemType.RangedFlail => "Flail (Ranged)",
			ItemType.Launcher => "Launcher",
			_ => ""
		};

		string rareName = rare switch
		{
			ItemRarity.Normal => "",
			ItemRarity.Magic => "Magic ",
			ItemRarity.Rare => "Rare ",
			ItemRarity.Unique => "Unique ",
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

	[GeneratedRegex(@"(\d+)|(\D+)")]
	private static partial Regex NumberHighlightRegex();
	#endregion

	#region Special rendering for rarities and influences
	private static void DrawSpecial(On_ItemSlot.orig_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color orig, SpriteBatch sb,
		Item[] inv, int context, int slot, Vector2 position, Color color)
	{
		if (!GearGlobalItem.IsGearItem(inv[slot]) || context == 21)
		{
			orig(sb, inv, context, slot, position, color);
			return;
		}

		PoTInstanceItemData data = inv[slot].GetInstanceData();

		string rareName = data.Rarity switch
		{
			ItemRarity.Normal => "Normal",
			ItemRarity.Magic => "Magic",
			ItemRarity.Rare => "Rare",
			ItemRarity.Unique => "Unique",
			_ => "Normal"
		};

		Texture2D back = ModContent.Request<Texture2D>($"{nameof(PathOfTerraria)}/Assets/Slots/{rareName}Back")
			.Value;
		Color backColor = Color.White * 0.75f;

		sb.Draw(back, position, null, backColor, 0f, default, Main.inventoryScale, SpriteEffects.None, 0f);
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

	/// <summary>
	/// Draws the shader overlay for solar items
	/// </summary>
	/// <param name="spriteBatch"></param>
	/// <param name="pos"></param>
	private static void DrawSolarSlot(SpriteBatch spriteBatch, Vector2 pos)
	{
		Texture2D tex = ModContent.Request<Texture2D>($"{nameof(PathOfTerraria)}/Assets/Slots/SlotMap").Value;

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
			.SetValue(ModContent.Request<Texture2D>($"{nameof(PathOfTerraria)}/Assets/Misc/SwirlNoise").Value);
		effect.Parameters["mapTexture"]
			.SetValue(ModContent.Request<Texture2D>($"{nameof(PathOfTerraria)}/Assets/Misc/SwirlNoise").Value);

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
		Texture2D tex = ModContent.Request<Texture2D>($"{nameof(PathOfTerraria)}/Assets/Slots/SlotMap").Value;

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
			.SetValue(ModContent.Request<Texture2D>($"{nameof(PathOfTerraria)}/Assets/Misc/ShaderNoise").Value);
		effect.Parameters["mapTexture"]
			.SetValue(ModContent.Request<Texture2D>($"{nameof(PathOfTerraria)}/Assets/Misc/ShaderNoise").Value);

		spriteBatch.End();
		spriteBatch.Begin(default, BlendState.Additive, default, default, RasterizerState.CullNone, effect,
			Main.UIScaleMatrix);

		spriteBatch.Draw(tex, pos, null, Color.White, 0, Vector2.Zero, Main.inventoryScale, 0, 0);

		spriteBatch.End();
		spriteBatch.Begin(default, default, default, default, RasterizerState.CullNone, default,
			Main.UIScaleMatrix);
	}
	#endregion
}
