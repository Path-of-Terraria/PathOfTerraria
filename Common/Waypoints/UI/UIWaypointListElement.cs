using PathOfTerraria.Common.NPCs.QuestMarkers;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.Questing;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Common.Waypoints.UI;

public sealed class UIWaypointListElement(Asset<Texture2D> icon, LocalizedText name, int index, string location) : UIElement
{
	private static Asset<Texture2D> Markers = null;

	public const float FullWidth = UIWaypointMenu.FullWidth - 18f;
	public const float FullHeight = 48f;
	public const float ElementMargin = 16f;

	public bool CanClick => ModContent.GetInstance<PersistentDataSystem>().ObelisksByLocation.Contains(Location);

	public readonly Asset<Texture2D> Icon = icon;
	public readonly int Index = index;
	public readonly string Location = location;

	public readonly LocalizedText Name = name;

	public bool Selected;

	private UIPanel panel;
	private UIScalingText text;
	private UIImage icon;

	public override void OnInitialize()
	{
		base.OnInitialize();

		Width.Set(FullWidth, 0f);
		Height.Set(FullHeight, 0f);

		Append(panel = BuildPanel());
		Append(icon = BuildIcon());
		Append(text = BuildText());
		Append(BuildSeparator());

		Markers ??= ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/WaypointMarkers");
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (!CanClick)
		{
			panel.BorderColor = Color.Lerp(panel.BorderColor, new Color(10, 15, 35), 0.3f) * 0.8f;
			text.Scale = MathHelper.SmoothStep(text.Scale, 0.7f, 0.3f);
			icon.Color = Color.Lerp(icon.Color, Color.Gray, 0.3f);
			return;
		}

		panel.BorderColor = Color.Lerp(panel.BorderColor, Selected ? Color.White : new Color(68, 97, 175), 0.3f) * 0.8f;
		icon.ImageScale = MathHelper.SmoothStep(icon.ImageScale, Selected ? 1.25f : 1f, 0.3f);
		icon.Color = Color.Lerp(icon.Color, Color.White, 0.3f);
		text.Scale = MathHelper.SmoothStep(text.Scale, Selected ? 0.9f : 0.7f, 0.3f);
	}

	private UIImage BuildIcon()
	{
		var icon = new UIImage(Icon)
		{
			VAlign = 0.5f,
			Left = { Pixels = ElementMargin },
			OverrideSamplerState = SamplerState.PointClamp
		};

		return icon;
	}

	protected override void DrawChildren(SpriteBatch spriteBatch)
	{
		base.DrawChildren(spriteBatch);
		QuestModPlayer questPlayer = Main.LocalPlayer.GetModPlayer<QuestModPlayer>();

		bool hasMarker = questPlayer.MarkerTypeByLocation.TryGetValue(Location, out QuestMarkerType loc) && loc != QuestMarkerType.None;
		bool available = QuestUnlockManager.LocationHasQuest(Location);

		if (available && !hasMarker)
		{
			hasMarker = true;
			loc = QuestMarkerType.HasQuest;
		}

		// Display quest marker if we have something that merits it
		if (hasMarker)
		{
			var source = new Rectangle(0, 38 * (int)loc, 36, 36);
			spriteBatch.Draw(Markers.Value, icon.GetDimensions().Position(), source, Color.White, 0f, Vector2.Zero, icon.ImageScale, SpriteEffects.None, 0);
		}
	}

	private UIScalingText BuildText()
	{
		var text = new UIScalingText(Name, 0.7f)
		{
			VAlign = 0.5f,
			Left = { Pixels = ElementMargin + Icon.Width() + (32f - Icon.Width()) + ElementMargin }
		};

		return text;
	}

	private static UIPanel BuildPanel()
	{
		var panel = new UIPanel(
			ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Waypoints/PanelBackground"),
			ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Waypoints/PanelBorder"),
			13
		)
		{
			BackgroundColor = new Color(68, 97, 175) * 0.8f,
			BorderColor = new Color(68, 97, 175) * 0.8f,
			OverrideSamplerState = SamplerState.PointClamp,
			Width = { Pixels = FullWidth },
			Height = { Pixels = FullHeight }
		};

		return panel;
	}

	private static UIImage BuildSeparator()
	{
		var separator = new UIImage(TextureAssets.MagicPixel)
		{
			Color = Color.White * 0.8f,
			ScaleToFit = true,
			HAlign = 0.5f,
			VAlign = 1f,
			Width = { Pixels = FullWidth },
			Height = { Pixels = 2f },
			OverrideSamplerState = SamplerState.PointClamp
		};

		return separator;
	}
}