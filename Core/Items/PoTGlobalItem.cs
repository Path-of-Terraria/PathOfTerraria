using PathOfTerraria.Content.Items.Gear;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
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
}
