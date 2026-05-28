using PathOfTerraria.Common.Config;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Common.Looting.ItemFiltering;

internal sealed class ItemFilterLootBeamGlobalItem : GlobalItem
{
	private const float BeamScale = 0.8f;
	private const float GroundScale = 0.72f;
	private const float FlashScale = 3.75f;

	private static Asset<Texture2D> BeamTexture = null!;
	private static Asset<Texture2D> CenterGlowTexture = null!;
	private static Asset<Texture2D> SimpleGlowTexture = null!;

	private int _trackedType = ItemID.None;
	private float _fadeIn;
	private int _animationTimer;
	private bool _highlightedThisFrame;

	public override bool InstancePerEntity => true;

	public override bool AppliesToEntity(Item entity, bool lateInstantiation)
	{
		return true;
	}

	public override void Load()
	{
		if (Main.dedServ)
		{
			return;
		}

		BeamTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/LootBeams/SimpleBeam");
		CenterGlowTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/LootBeams/CenterGlow");
		SimpleGlowTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/LootBeams/SimpleGlow");
	}

	public override void Unload()
	{
		BeamTexture = null!;
		CenterGlowTexture = null!;
		SimpleGlowTexture = null!;
	}

	public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
	{
		if (!CanDrawLootBeam(item, out Color beamColor))
		{
			return base.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
		}

		ResetVisualStateIfNeeded(item);
		_highlightedThisFrame = true;

		Texture2D itemTexture = TextureAssets.Item[item.type].Value;
		int itemFrameHeight = GetItemFrameHeight(item, itemTexture);
		Vector2 ground = new(item.Center.X - Main.screenPosition.X, item.Hitbox.Bottom - Main.screenPosition.Y);
		Vector2 itemCenter = ground - new Vector2(0f, itemFrameHeight * 0.5f);
		float time = _animationTimer / 60f;
		float breath = 0.92f + 0.08f * MathF.Sin(time * MathHelper.TwoPi);
		float fade = _fadeIn;
		Color beamColorWithFade = beamColor * fade;

		DrawVerticalSignal(spriteBatch, ground, beamColorWithFade, breath);
		DrawGroundFlare(spriteBatch, ground, beamColorWithFade, breath, fade);
		DrawItemHalo(spriteBatch, itemTexture, itemFrameHeight, itemCenter, beamColorWithFade, time);

		_fadeIn = Utils.Clamp(_fadeIn + FrameAdjustedStep(0.035f), 0f, 1f);

		return base.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
	}

	public override bool GrabStyle(Item item, Player player)
	{
		_fadeIn = Utils.Clamp(_fadeIn - FrameAdjustedStep(0.16f), 0f, 1f);
		return base.GrabStyle(item, player);
	}

	public override void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
	{
		if (!_highlightedThisFrame)
		{
			return;
		}

		_animationTimer++;
		_highlightedThisFrame = false;
	}

	private static bool CanDrawLootBeam(Item item, out Color beamColor)
	{
		beamColor = Color.White;

		return Main.netMode != NetmodeID.Server &&
		       ModContent.GetInstance<UIConfig>().ShowLootBeams &&
		       item is not null &&
		       !item.IsAir &&
		       TryGetHighlightColor(item, out beamColor);
	}

	private void ResetVisualStateIfNeeded(Item item)
	{
		if (_trackedType == item.type)
		{
			return;
		}

		_trackedType = item.type;
		_fadeIn = 0f;
		_animationTimer = 0;
		_highlightedThisFrame = false;
	}

	private static int GetItemFrameHeight(Item item, Texture2D itemTexture)
	{
		if (Main.itemAnimationsRegistered.Contains(item.type) && Main.itemAnimations[item.type].FrameCount > 1)
		{
			return itemTexture.Height / Main.itemAnimations[item.type].FrameCount;
		}

		return itemTexture.Height;
	}

	private static void DrawVerticalSignal(SpriteBatch spriteBatch, Vector2 ground, Color color, float breath)
	{
		Vector2 center = ground - new Vector2(0f, 70f * BeamScale);
		Texture2D texture = BeamTexture.Value;
		Vector2 origin = texture.Size() * 0.5f;

		spriteBatch.Draw(texture, center, null, color * 0.72f, 0f, origin, new Vector2(BeamScale * breath, BeamScale), SpriteEffects.None, 0f);
		spriteBatch.Draw(texture, center, null, color * 0.34f, 0f, origin, new Vector2(BeamScale * 1.7f, BeamScale), SpriteEffects.FlipHorizontally, 0f);
	}

	private static void DrawGroundFlare(SpriteBatch spriteBatch, Vector2 ground, Color color, float breath, float fade)
	{
		Texture2D texture = CenterGlowTexture.Value;
		Vector2 origin = texture.Size() * 0.5f;
		float arrivalFlash = GroundScale * (FlashScale - fade * (FlashScale - 1f));

		spriteBatch.Draw(texture, ground + new Vector2(0f, 2f), null, color * 0.48f, 0f, origin, GroundScale * (1.1f + (1f - breath) * 0.4f), SpriteEffects.None, 0f);
		spriteBatch.Draw(texture, ground + new Vector2(0f, 2f), null, color * 0.22f, MathHelper.PiOver4, origin, arrivalFlash, SpriteEffects.None, 0f);
	}

	private static void DrawItemHalo(SpriteBatch spriteBatch, Texture2D itemTexture, int itemFrameHeight, Vector2 itemCenter, Color color, float time)
	{
		float itemSizeFactor = Utils.Clamp((itemTexture.Width + itemFrameHeight) / 40f, 0.45f, 3f);
		Texture2D texture = SimpleGlowTexture.Value;
		Vector2 origin = texture.Size() * 0.5f;

		spriteBatch.Draw(texture, itemCenter, null, color * 0.55f, 0f, origin, 0.22f * itemSizeFactor, SpriteEffects.None, 0f);

		for (int i = 0; i < 3; i++)
		{
			float angle = time * 1.8f + i * MathHelper.TwoPi / 3f;
			Vector2 offset = angle.ToRotationVector2() * (4f + itemSizeFactor * 1.5f);
			spriteBatch.Draw(texture, itemCenter + offset, null, color * 0.18f, 0f, origin, 0.07f * itemSizeFactor, SpriteEffects.None, 0f);
		}
	}

	private static float FrameAdjustedStep(float amount)
	{
		return amount / Math.Max((float)Main.frameRate / 60f, 0.017f);
	}

	private static bool TryGetHighlightColor(Item item, out Color color)
	{
		color = Color.White;

		if (Main.LocalPlayer is not { active: true } player)
		{
			return false;
		}

		ItemFilter filter = player.GetModPlayer<ItemFilterPlayer>().ActiveFilter;

		if (filter is null || !filter.Evaluate(item, out ItemFilterEvaluationResult result) || !result.IsHighlighted)
		{
			return false;
		}

		color = GetBeamColor(item);
		return true;
	}

	private static Color GetBeamColor(Item item)
	{
		if (item.TryGetGlobalItem(out PoTInstanceItemData data) &&
		    (data.ItemType != ItemType.None || data.Rarity > ItemRarity.Normal))
		{
			return ItemTooltips.GetRarityColor(data.Rarity);
		}

		if (item.expert || item.rare == ItemRarityID.Expert)
		{
			return Main.DiscoColor;
		}

		if (item.master || item.rare == ItemRarityID.Master)
		{
			return new Color(255, (int)(Main.masterColor * 200), 0);
		}

		return item.rare switch
		{
			ItemRarityID.Gray => new Color(130, 130, 130),
			ItemRarityID.Blue => new Color(150, 150, 255),
			ItemRarityID.Green => new Color(150, 255, 150),
			ItemRarityID.Orange => new Color(255, 200, 150),
			ItemRarityID.LightRed => new Color(255, 150, 150),
			ItemRarityID.Pink => new Color(255, 150, 255),
			ItemRarityID.LightPurple => new Color(210, 160, 255),
			ItemRarityID.Lime => new Color(150, 255, 10),
			ItemRarityID.Yellow => new Color(255, 255, 10),
			ItemRarityID.Cyan => new Color(5, 200, 255),
			ItemRarityID.Red => new Color(255, 40, 100),
			ItemRarityID.Purple => new Color(180, 40, 255),
			ItemRarityID.Quest => new Color(255, 175, 0),
			_ => Color.White,
		};
	}
}
