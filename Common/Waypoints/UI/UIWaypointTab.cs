using PathOfTerraria.Common.UI.Elements;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Common.Waypoints.UI;

public sealed class UIWaypointTab : UIElement
{
	/// <summary>
	///     The margin of this element in pixels.
	/// </summary>
	public const float Margin = 16f;

	/// <summary>
	///		The width of this element in pixels.
	/// </summary>
	public const float FullWidth = UIWaypointList.FullWidth - 18f;
	
	/// <summary>
	///		The height of this element in pixels.
	/// </summary>
	public const float FullHeight = 48f;

	/// <summary>
	///     Whether this element is selected or not.
	/// </summary>
	public bool Selected { get; set; }

	/// <summary>
	///     The index of this element in a <see cref="UIList" />.
	/// </summary>
	public readonly int Index;

	/// <summary>
	///		The display name of this tab.
	/// </summary>
	public readonly LocalizedText Name;

	/// <summary>
	///		The icon of this tab.
	/// </summary>
	public readonly Asset<Texture2D> Icon;
	
	public UIWaypointTab(Asset<Texture2D> icon, LocalizedText name, int index)
	{
		Icon = icon;
		Name = name;
		Index = index;
	}

	public override void OnInitialize()
	{
		base.OnInitialize();
		
		Width.Set(FullWidth, 0f);
		Height.Set(FullHeight, 0f);

		Append(BuildPanel());
		Append(BuildIcon());
		Append(BuildText());
		Append(BuildSeparator());
	}

	private UIPanel BuildPanel()
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

		panel.OnUpdate += UpdatePanel;
		
		return panel;
	}

	private UIImage BuildIcon()
	{
		var icon = new UIImage(Icon)
		{
			VAlign = 0.5f,
			OverrideSamplerState = SamplerState.PointClamp
		};

		icon.Left.Set(Margin, 0f);

		return icon;
	}

	private UIText BuildText()
	{
		var text = new UIText(Name, 0.8f)
		{
			VAlign = 0.5f,
			Left = { Pixels = Margin + Icon.Width() + (32f - Icon.Width()) + Margin }
		};

		return text;
	}

	private UIImage BuildSeparator()
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

	private void UpdatePanel(UIElement element)
	{
		if (element is not UIPanel panel)
		{
			return;
		}
		
		panel.BorderColor = Color.Lerp(panel.BorderColor, Selected ? Color.White : new Color(68, 97, 175), 0.3f) * 0.8f;
	}
}