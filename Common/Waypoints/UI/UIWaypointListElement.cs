using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Common.Waypoints.UI;

public sealed class UIWaypointListElement(Asset<Texture2D> icon, LocalizedText name, int index) : UIElement
{
	public const float FullWidth = UIWaypointMenu.FullWidth - 18f;

	public const float FullHeight = 48f;

	public const float ElementMargin = 16f;

	public readonly Asset<Texture2D> Icon = icon;

	public readonly int Index = index;

	public readonly LocalizedText Name = name;
	private UIImage icon;

	private UIPanel panel;

	public bool Selected;
	private UIScalingText text;

	public override void OnInitialize()
	{
		base.OnInitialize();

		Width.Set(FullWidth, 0f);
		Height.Set(FullHeight, 0f);

		Append(panel = BuildPanel());
		Append(icon = BuildIcon());
		Append(text = BuildText());
		Append(BuildSeparator());
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		panel.BorderColor = Color.Lerp(panel.BorderColor, Selected ? Color.White : new Color(68, 97, 175), 0.3f) * 0.8f;
		icon.ImageScale = MathHelper.SmoothStep(icon.ImageScale, Selected ? 1.25f : 1f, 0.3f);
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