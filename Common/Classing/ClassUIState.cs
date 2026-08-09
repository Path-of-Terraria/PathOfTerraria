using PathOfTerraria.Common.Looting.VirtualBagUI;
using PathOfTerraria.Common.UI.Utilities;
using PathOfTerraria.Core.UI;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.Classing;

internal class ClassUIState(Action resetAction, Player player) : UIState
{
	const int PanelWidth = 400;
	const int PaddedPanelWidth = 400 - (12 * 2);
	const int StandardPanelHeight = 288;
	const float DefaultTextScale = 0.8f;

	public static readonly Rectangle SeperatorFrame = new(0, 0, PaddedPanelWidth, 1);
	public static readonly Color BackColor = Color.Black * 0.6f;

	private readonly Action resetAction = resetAction;
	private readonly Player player = player;

	public override void OnInitialize()
	{
		const string TexturePath = "PathOfTerraria/Assets/UI/SquarePanel";

		UIPanel panel = this.AddElement(new UIPanel(ModContent.Request<Texture2D>(TexturePath + "Background"), ModContent.Request<Texture2D>(TexturePath + "Outline")), x =>
		{
			x.SetDimensions(default, default, (0, PanelWidth), (0, StandardPanelHeight));
			x.HAlign = 0.5f;
			x.VAlign = 0f;

			if (Main.gameMenu) // When in the menu, position under the logo, otherwise position under the tutorial UI
			{
				x.Top = StyleDimension.FromPixelsAndPercent(-170, 0.5f);
			}
			else
			{
				x.Top = StyleDimension.FromPixelsAndPercent(-120, 0.5f);
			}
		});

		BuildPage(StarterClass.Melee, panel);
	}

	private void BuildPage(StarterClass classes, UIPanel panel)
	{
		if (Main.gameMenu)
		{
			panel.AddElement(new UICharacter(player, true, false, 1.5f), x =>
			{
				x.HAlign = 0.5f;
				x.Top = StyleDimension.FromPixels(-60);
			});
		}

		UIText title = panel.AddElement(new UIText(classes.Localize(), 0.8f, true) { Top = StyleDimension.FromPixels(6) });
		title.AddElement(new UIItemIcon(ContentSamples.ItemsByType[classes switch
		{
			StarterClass.Melee => ItemID.BreakerBlade,
			StarterClass.Ranged => ItemID.DemonBow,
			StarterClass.Magic => ItemID.RainbowRod,
			StarterClass.Summon or _ => ItemID.SlimeStaff,
		}], false), self =>
		{
			self.HAlign = 1f;
			self.Left = StyleDimension.FromPixels(40);
		});

		panel.AddElement(new UIImageFramed(TextureAssets.MagicPixel, SeperatorFrame), x => x.Top = StyleDimension.FromPixels(48));

		panel.AddElement(new UIImageButton(ModContent.Request<Texture2D>("PathOfTerraria/Assets/UI/LeftArrow", AssetRequestMode.ImmediateLoad)), self =>
		{
			self.Top = StyleDimension.FromPixels(54);
			self.OnLeftClick += (_, _) => SwitchClass(classes - 1, panel);
		});

		panel.AddElement(new UIImageButton(ModContent.Request<Texture2D>("PathOfTerraria/Assets/UI/RightArrow", AssetRequestMode.ImmediateLoad)), self =>
		{
			self.Top = StyleDimension.FromPixels(54);
			self.OnLeftClick += (_, _) => SwitchClass(classes + 1, panel);
			self.HAlign = 1f;
		});

		panel.AddElement(new UIButton<string>(Language.GetText("Mods.PathOfTerraria.UI.Confirm").Value), self =>
		{
			self.Width = StyleDimension.FromPixels(120);
			self.Height = StyleDimension.FromPixels(24);
			self.Top = StyleDimension.FromPixels(54);
			self.HAlign = 0.5f;

			self.OnLeftClick += (_, _) =>
			{
				player.GetModPlayer<ClassingPlayer>().Class = classes;
				resetAction();
			};
		});

		if (Main.gameMenu)
		{
			var close = new UIImageButton(ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/CloseButton")) { HAlign = 1f };
			close.SetVisibility(1, 0.6f);
			close.SetDimensions((0, 0), (0, 0), (0, 38), (0, 38));
			close.OnLeftClick += Close;
			panel.Append(close);
		}

		string desc = Language.GetTextValue("Mods.PathOfTerraria.UI.ClassPages." + classes + ".Description");
		panel.AddElement(new UISimpleWrappableText(desc, DefaultTextScale)
		{
			Wrappable = true,
			Top = StyleDimension.FromPixels(84),
			Width = StyleDimension.FromPixels(PaddedPanelWidth),
			BorderColour = BackColor,
			Border = true,
		});

		float yOffset = 84 + ChatManager.GetStringSize(FontAssets.MouseText.Value, desc, new Vector2(DefaultTextScale), PaddedPanelWidth).Y;
		GenerateSection(classes, panel, ref yOffset, "PreferredWeapons", "Weapons");
		//GenerateSection(classes, panel, ref yOffset, "Awakenings", "Awakenings");
		GenerateSection(classes, panel, ref yOffset, "Skill", "Skill", Language.GetTextValue("Mods.PathOfTerraria.UI.ClassPages." + classes + ".SkillTitle"));

		panel.Height = StyleDimension.FromPixels(yOffset + 24);
		Recalculate();
	}

	public override void OnDeactivate()
	{
		RemoveAllChildren();
	}

	private static float GenerateSection(StarterClass classes, UIPanel panel, ref float yOffset, string title, string info, string addTitle = "")
	{
		string awakeningsTitle = Language.GetTextValue("Mods.PathOfTerraria.UI.ClassPages." + title) + addTitle;
		panel.AddElement(new UISimpleWrappableText(awakeningsTitle, 1f)
		{
			Wrappable = true,
			Top = StyleDimension.FromPixels(yOffset),
			Width = StyleDimension.FromPixels(PaddedPanelWidth),
			BorderColour = BackColor,
			Border = true,
		});

		yOffset += 24;
		yOffset = (int)yOffset;
		float currentYOffset = yOffset;
		panel.AddElement(new UIImageFramed(TextureAssets.MagicPixel, SeperatorFrame), x => x.Top = StyleDimension.FromPixels(currentYOffset));

		yOffset += 6;
		string infoText = Language.GetTextValue("Mods.PathOfTerraria.UI.ClassPages." + classes + "." + info);
		UISimpleWrappableText uiInfo = panel.AddElement(new UISimpleWrappableText(infoText, DefaultTextScale)
		{
			Wrappable = true,
			Top = StyleDimension.FromPixels(yOffset),
			Width = StyleDimension.FromPixels(PaddedPanelWidth),
			BorderColour = BackColor,
			Border = true,
		});

		string wrappedInfoText = UISimpleWrappableText.WrapText(FontAssets.MouseText.Value, infoText, PaddedPanelWidth, DefaultTextScale);
		Vector2 stringSize = ChatManager.GetStringSize(FontAssets.MouseText.Value, wrappedInfoText, new Vector2(DefaultTextScale));
		yOffset += stringSize.Y;
		
		return yOffset;
	}

	private void SwitchClass(StarterClass newClass, UIPanel panel)
	{
		if (newClass == StarterClass.None)
		{
			newClass = StarterClass.Summon;
		}
		else if (newClass > StarterClass.Summon)
		{
			newClass = StarterClass.Melee;
		}

		panel.RemoveAllChildren();
		BuildPage(newClass, panel);
	}

	private void Close(UIMouseEvent evt, UIElement listeningElement)
	{
		SoundEngine.PlaySound(SoundID.MenuClose, Main.LocalPlayer.Center);
		resetAction.Invoke();
	}
}
