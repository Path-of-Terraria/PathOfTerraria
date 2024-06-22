using PathOfTerraria.Content.Passives;
using PathOfTerraria.Core;
using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Core.Systems.TreeSystem;
using System.Collections.Generic;
using PathOfTerraria.Content.GUI.PassiveTree;
using PathOfTerraria.Content.GUI.SkillsTree;
using PathOfTerraria.Content.GUI.Utilities;
using PathOfTerraria.Core.Systems.ModPlayers;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI;

internal class TreeState : SmartUIState
{
	public UIDraggablePanel Panel;
	public UIImageButton CloseButton;
	public PassiveTreeInnerPanel PassiveTreeInner;
	public SkillsTreeInnerPanel SkillsTreeInner;

	public bool IsVisible;

	protected static TreePlayer TreeSystem => Main.LocalPlayer.GetModPlayer<TreePlayer>();

	protected const int PointsAndExitPadding = 10;

	protected const int DraggablePanelHeight = 32;

	protected const int TopPadding = -400;
	protected const int PanelHeight = 800 - DraggablePanelHeight;
	protected const int LeftPadding = -450;
	protected const int PanelWidth = 900;

	public Vector2 TopLeftTree;
	public Vector2 BotRightTree;

	public override bool Visible => IsVisible;
	public PlayerClass CurrentDisplayClass = PlayerClass.None;

	public void Toggle(PlayerClass newClass = PlayerClass.None)
	{
		if (newClass == PlayerClass.None || IsVisible)
		{
			IsVisible = false;
			return;
		}

		if (CurrentDisplayClass != newClass)
		{
			TopLeftTree = Vector2.Zero;
			BotRightTree = Vector2.Zero;
			CurrentDisplayClass = newClass;
			RemoveAllChildren();
			CreateMainPanel();
			AddPassiveTreeInnerPanel();
			AddSkillsTreeInnerPanel();
			AddCloseButton();

			TreeSystem.CreateTree();
			TreeSystem.ActiveNodes.ForEach(n =>
			{
				if (n is JewelSocket socket)
				{
					PassiveTreeInner.Append(new PassiveSocket(socket));
				}
				else
				{
					PassiveTreeInner.Append(new PassiveElement(n));
				}
			});
		}

		IsVisible = true;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Recalculate();
		base.Draw(spriteBatch);
		DrawPanelText(spriteBatch);
	}

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
	}

	protected void CreateMainPanel()
	{
		var localizedTexts = new (string key, LocalizedText text)[]
		{
			("PassiveTree", Language.GetText("Mods.PathOfTerraria.GUI.PassiveTreeTab")),
			("SkillTree", Language.GetText("Mods.PathOfTerraria.GUI.SkillTreeTab"))
		};
		Panel = new UIDraggablePanel(false, false, localizedTexts, DraggablePanelHeight);
		Panel.OnActiveTabChanged += HandleActiveTabChanged;
		Panel.Left.Set(LeftPadding, 0.5f);
		Panel.Top.Set(TopPadding, 0.5f);
		Panel.Width.Set(PanelWidth, 0);
		Panel.Height.Set(PanelHeight, 0);
		Append(Panel);
	}

	private void HandleActiveTabChanged()
	{
		switch (Panel.ActiveTab)
		{
			case "PassiveTree":
				PassiveTreeInner.Visible = true;
				SkillsTreeInner.Visible = false;
				break;
			case "SkillTree":
				PassiveTreeInner.Visible = false;
				SkillsTreeInner.Visible = true;
				break;
		}
	}

	protected void AddPassiveTreeInnerPanel()
	{
		PassiveTreeInner = new PassiveTreeInnerPanel();
		PassiveTreeInner.Left.Set(0, 0);
		PassiveTreeInner.Top.Set(DraggablePanelHeight, 0);
		PassiveTreeInner.Width.Set(0, 1f);
		PassiveTreeInner.Height.Set(-DraggablePanelHeight, 1f);
		Panel.Append(PassiveTreeInner);
	}
	
	protected void AddSkillsTreeInnerPanel()
	{
		SkillsTreeInner = new SkillsTreeInnerPanel();
		SkillsTreeInner.Left.Set(0, 0);
		SkillsTreeInner.Top.Set(DraggablePanelHeight, 0);
		SkillsTreeInner.Width.Set(0, 1f);
		SkillsTreeInner.Height.Set(-DraggablePanelHeight, 1f);
		Panel.Append(SkillsTreeInner);
	}

	protected void AddCloseButton()
	{
		CloseButton =
			new UIImageButton(ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/CloseButton"));
		CloseButton.Left.Set(-38 - PointsAndExitPadding, 1f);
		CloseButton.Top.Set(10, 0f);
		CloseButton.Width.Set(38, 0);
		CloseButton.Height.Set(38, 0);
		CloseButton.OnLeftClick += (a, b) =>
		{
			IsVisible = false;
			SoundEngine.PlaySound(SoundID.MenuClose, Main.LocalPlayer.Center);
		};
		CloseButton.SetVisibility(1, 1);
		Panel.Append(CloseButton);
	}

	protected void DrawPanelText(SpriteBatch spriteBatch)
	{
		Texture2D tex = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/PassiveFrameSmall").Value;
		TreePlayer treePlayer = Main.LocalPlayer.GetModPlayer<TreePlayer>();
		SkillPlayer skillPlayer = Main.LocalPlayer.GetModPlayer<SkillPlayer>();

		Vector2 pointsDrawPoin = new Vector2(PointsAndExitPadding, PointsAndExitPadding + DraggablePanelHeight) +
		                         tex.Size() / 2;

		int points = Panel.ActiveTab switch
		{
			"PassiveTree" => treePlayer.Points,
			"SkillTree" => skillPlayer.Points,
			_ => 0
		};
		spriteBatch.Draw(tex, GetRectangle().TopLeft() + pointsDrawPoin, null, Color.White, 0, tex.Size() / 2f, 1, 0,
			0);
		Utils.DrawBorderStringBig(spriteBatch, $"{points}", GetRectangle().TopLeft() + pointsDrawPoin,
			treePlayer.Points > 0 ? Color.Yellow : Color.Gray, 0.5f, 0.5f, 0.35f);
		Utils.DrawBorderStringBig(spriteBatch, "Points remaining",
			GetRectangle().TopLeft() + pointsDrawPoin + new Vector2(138, 0), Color.White, 0.6f, 0.5f, 0.35f);
	}

	public Rectangle GetRectangle()
	{
		return Panel.GetDimensions().ToRectangle();
	}
}