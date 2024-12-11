using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Systems.Questing;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Quests;

public class QuestsUIState : CloseableSmartUi
{
	private QuestDetailsPanel _questDetails;
	public const float SelectedOpacity = 0.25f;
	public const float HoveredOpacity = 0.1f;
	private UIImageButton _closeButton;

	public override int TopPadding => 0;
	public override int PanelHeight => 1000;
	public override int LeftPadding => 0;
	public override int PanelWidth => 750;
	public override bool IsCentered => true;
	
	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
	}
	
	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);
#if DEBUG
		GUIDebuggingTools.DrawGuiBorder(spriteBatch, this, Color.Green);
#endif
	}

	public override int DepthPriority => 3;

	public void Toggle()
	{
		if (IsVisible)
		{
			IsVisible = false;
			_questDetails.Remove();
			return;
		}
		
		RemoveAllChildren();
		base.CreateMainPanel(false, new Point(970, 715), false, true);
		Quest quest = Main.LocalPlayer.GetModPlayer<QuestModPlayer>().QuestsByName.FirstOrDefault(x => x.Value.Active).Value;

		_questDetails = new QuestDetailsPanel
		{
			Width = StyleDimension.FromPercent(1),
			Height = StyleDimension.FromPercent(1),
			ViewedQuestName = quest is not null ? quest.FullName : "",
		};

		_questDetails.PopulateQuestSteps();
		Panel.Append(_questDetails);

		_closeButton = new UIImageButton(ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/CloseButton"));
		_closeButton.Left.Set(0, 0.83f);
		_closeButton.Top.Set(40, 0f);
		_closeButton.Width.Set(38, 0);
		_closeButton.Height.Set(38, 0);
		_closeButton.OnLeftClick += (a, b) =>
		{
			IsVisible = false;
			SoundEngine.PlaySound(SoundID.MenuClose, Main.LocalPlayer.Center);
		};
		_closeButton.SetVisibility(1, 1);
		Panel.Append(_closeButton);

		IsVisible = true;
		Visible = true;
		AppendQuests();
		Recalculate();
	}
	
	private void AppendQuests()
	{
		int offset = 0;
		QuestModPlayer player = Main.LocalPlayer.GetModPlayer<QuestModPlayer>();

		foreach (Quest quest in player.QuestsByName.Values)
		{
			if (quest.Completed) // Gray out completed quests
			{
				UIText text = new(quest.DisplayName.Value, 0.7f)
				{
					TextColor = Color.Gray,
					ShadowColor = Color.Transparent,
					Height = StyleDimension.FromPixels(22),
					Left = StyleDimension.FromPixelsAndPercent(26, 0.15f),
					Top = StyleDimension.FromPixels(126 + offset),
				};

				_questDetails.Append(text);
				offset += 22;
			}
			else if (quest.Active) // Properly display active quests
			{
				UISelectableQuest selectableQuest = new(quest, _questDetails);
				selectableQuest.Left.Set(0, 0.15f);
				selectableQuest.Top.Set(120 + offset, 0);
				selectableQuest.OnLeftClick += (_, _) => SelectQuest(quest.FullName);
				_questDetails.Append(selectableQuest);
				offset += 22;
			}
		}
	}

	public void SelectQuest(string questName)
	{
		_questDetails.ViewedQuestName = questName;

		// Clear steps to repopulate later
		for (int i = 0; i < _questDetails.Children.Count(); ++i)
		{
			if (_questDetails.Children.ElementAt(i) is UISelectableQuestStep)
			{
				_questDetails.RemoveChild(_questDetails.Children.ElementAt(i));
				i--;
			}
		}

		_questDetails.PopulateQuestSteps();
		Recalculate();
	}
}