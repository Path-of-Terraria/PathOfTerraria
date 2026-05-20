using PathOfTerraria.Common.Config;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Common.Looting.ItemFiltering;

internal struct ItemFilterLootBeamData
{
	public int Type;
	public float FadeIn;
	public int TimeSinceSpawn;
	public bool Initialized;
	public bool HighlightedThisFrame;
}

internal sealed class ItemFilterLootBeamSystem : ModSystem
{
	public static ItemFilterLootBeamData[] DataByIndex = [];

	public static void Init()
	{
		DataByIndex = new ItemFilterLootBeamData[Main.maxItems];
	}

	public override void Load()
	{
		Init();
	}

	public override void Unload()
	{
		DataByIndex = [];
	}

	public override void PostUpdateItems()
	{
		if (DataByIndex.Length < Main.maxItems)
		{
			Array.Resize(ref DataByIndex, Main.maxItems);
		}

		for (int i = 0; i < Main.maxItems; i++)
		{
			Item item = Main.item[i];
			ref ItemFilterLootBeamData data = ref DataByIndex[i];

			if (!item.active)
			{
				if (data.Type > ItemID.None)
				{
					data = new ItemFilterLootBeamData();
				}

				continue;
			}

			if (item.type <= ItemID.None || data.Type == item.type)
			{
				continue;
			}

			data = new ItemFilterLootBeamData
			{
				Type = item.type,
				Initialized = true
			};
		}
	}
}

internal sealed class ItemFilterLootBeamPlayer : ModPlayer
{
	public override void OnEnterWorld()
	{
		if (Main.myPlayer == Player.whoAmI)
		{
			ItemFilterLootBeamSystem.Init();
		}
	}
}

internal sealed class ItemFilterLootBeamGlobalItem : GlobalItem
{
	private const float BeamScale = 0.75f;
	private const float GlowScale = 0.75f;
	private const float FlashScale = 5f;

	private static Asset<Texture2D> BeamTexture = null!;
	private static Asset<Texture2D> CenterGlowTexture = null!;
	private static Asset<Texture2D> SimpleGlowTexture = null!;

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
		if (Main.netMode == NetmodeID.Server || !ModContent.GetInstance<UIConfig>().ShowLootBeams ||
		    item is null || item.IsAir || !TryGetHighlightColor(item, out Color beamColor))
		{
			return base.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
		}

		ref ItemFilterLootBeamData data = ref EnsureData(item, whoAmI);
		data.HighlightedThisFrame = true;

		Texture2D itemTexture = TextureAssets.Item[item.type].Value;
		int itemFrameHeight = itemTexture.Height;

		if (Main.itemAnimationsRegistered.Contains(item.type) && Main.itemAnimations[item.type].FrameCount > 1)
		{
			itemFrameHeight /= Main.itemAnimations[item.type].FrameCount;
		}

		float pulse = Utils.Clamp(((float)Math.Sin(MathHelper.ToRadians(data.TimeSinceSpawn * 2)) + 1f) * 0.5f, 0f, 1f);
		float fade = data.FadeIn;
		float flashGlowScale = GlowScale * (FlashScale - fade * (FlashScale - 1f));
		Vector2 screenCenter = new(item.Center.X - Main.screenPosition.X, item.Hitbox.Bottom - Main.screenPosition.Y);
		Vector2 glowPosition = screenCenter - new Vector2(0, itemFrameHeight * 0.5f);
		Color drawColor = beamColor * fade * (0.75f + pulse * 0.25f);

		spriteBatch.Draw(
			BeamTexture.Value,
			screenCenter - new Vector2(0, itemFrameHeight * 0.5f + 56f * BeamScale),
			null,
			drawColor,
			0f,
			BeamTexture.Value.Size() * 0.5f,
			BeamScale,
			SpriteEffects.None,
			0f);

		spriteBatch.Draw(
			CenterGlowTexture.Value,
			glowPosition,
			null,
			drawColor,
			0f,
			CenterGlowTexture.Value.Size() * 0.5f,
			flashGlowScale,
			SpriteEffects.None,
			0f);

		float itemSizeFactor = Utils.Clamp((itemTexture.Width / 16f + itemFrameHeight / 16f) * 0.5f, 0.25f, 5f);
		float simpleGlowScale = (0.3f + pulse * 0.05f * itemSizeFactor) * GlowScale;

		spriteBatch.Draw(
			SimpleGlowTexture.Value,
			glowPosition,
			null,
			beamColor * fade * (0.5f + pulse * 0.5f),
			0f,
			SimpleGlowTexture.Value.Size() * 0.5f,
			simpleGlowScale,
			SpriteEffects.None,
			0f);

		data.FadeIn = Utils.Clamp(data.FadeIn + 0.025f / Math.Max((float)Main.frameRate / 60f, 0.017f), 0f, 1f);

		return base.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
	}

	public override bool GrabStyle(Item item, Player player)
	{
		if (item.whoAmI >= 0 && item.whoAmI < ItemFilterLootBeamSystem.DataByIndex.Length)
		{
			ref ItemFilterLootBeamData data = ref ItemFilterLootBeamSystem.DataByIndex[item.whoAmI];
			data.FadeIn = Utils.Clamp(data.FadeIn - 0.125f / Math.Max((float)Main.frameRate / 60f, 0.017f), 0f, 1f);
		}

		return base.GrabStyle(item, player);
	}

	public override void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
	{
		if (whoAmI < 0 || whoAmI >= ItemFilterLootBeamSystem.DataByIndex.Length)
		{
			return;
		}

		ref ItemFilterLootBeamData data = ref ItemFilterLootBeamSystem.DataByIndex[whoAmI];

		if (!data.HighlightedThisFrame)
		{
			return;
		}

		data.TimeSinceSpawn++;
		data.HighlightedThisFrame = false;
	}

	private static ref ItemFilterLootBeamData EnsureData(Item item, int whoAmI)
	{
		if (whoAmI >= ItemFilterLootBeamSystem.DataByIndex.Length)
		{
			Array.Resize(ref ItemFilterLootBeamSystem.DataByIndex, whoAmI + 1);
		}

		ref ItemFilterLootBeamData data = ref ItemFilterLootBeamSystem.DataByIndex[whoAmI];

		if (!data.Initialized || data.Type != item.type)
		{
			data = new ItemFilterLootBeamData
			{
				Type = item.type,
				Initialized = true
			};
		}

		return ref data;
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
