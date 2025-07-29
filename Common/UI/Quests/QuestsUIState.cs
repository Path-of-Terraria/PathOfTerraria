using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.UI.Guide;
using PathOfTerraria.Core.UI.SmartUI;
using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Quests;

public class QuestsUIState : CloseableSmartUi, IMutuallyExclusiveUI
{
	private QuestDetailsPanel _questDetails;
	private QuestDetailsPanel _completedQuestDetails;

	public const float SelectedOpacity = 0.25f;
	public const float HoveredOpacity = 0.1f;

	private UIImageButton _closeButton;
	private UIImage _bookBackground;

	protected override int TopPadding => 0;
	protected override int PanelHeight => 1000;
	protected override int LeftPadding => 0;
	protected override int PanelWidth => 750;
	protected override bool IsCentered => true;
	public override int DepthPriority => 3;

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

	public override void SafeUpdate(GameTime gameTime)
	{
		if (!Main.playerInventory)
		{
			Toggle();
		}
	}

	public void Toggle()
	{
		if (IsVisible)
		{
			IsVisible = false;
			_questDetails.Remove();
			return;
		}

		Main.LocalPlayer.GetModPlayer<TutorialPlayer>().TutorialChecks.Add(TutorialCheck.OpenedQuestBook);
		ModContent.GetInstance<SmartUiLoader>().ClearMutuallyExclusive<QuestsUIState>();

		RemoveAllChildren();
		base.CreateMainPanel(false, new Point(970, 715), false, true);
		Quest quest = Main.LocalPlayer.GetModPlayer<QuestModPlayer>().QuestsByName.FirstOrDefault(x => x.Value.Active).Value;

		_bookBackground = new(ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/QuestBookBackground"))
		{
			Left = new(-14, 0),
			Top = new(-18, 0)
		};

		Panel.Append(_bookBackground);

		_questDetails = new QuestDetailsPanel
		{
			Width = StyleDimension.FromPercent(1),
			Height = StyleDimension.FromPercent(0.5f),
			ViewedQuestName = quest is not null ? quest.FullName : ""
		};

		_questDetails.PopulateQuestSteps();
		Panel.Append(_questDetails);

		//Separate completed quests from others
		_completedQuestDetails = new QuestDetailsPanel
		{
			Width = StyleDimension.FromPercent(1),
			Height = StyleDimension.FromPercent(0.5f),
			Top = StyleDimension.FromPercent(0.5f)
		};

		//_completedQuestDetails.PopulateQuestSteps();
		Panel.Append(_completedQuestDetails);

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
		_questDetails.RemoveAllChildren();

		int offset = 0;
		int completedOffset = 0;

		QuestModPlayer player = Main.LocalPlayer.GetModPlayer<QuestModPlayer>();

		foreach (Quest quest in player.QuestsByName.Values.OrderByDescending(x => x.FullName == player.PinnedQuest))
		{
			if (quest.Completed) // Gray out completed quests
			{
				UIText text = new(quest.DisplayName.Value, 0.7f)
				{
					TextColor = Color.Gray,
					ShadowColor = Color.Transparent,
					Height = StyleDimension.FromPixels(22),
					Left = StyleDimension.FromPixelsAndPercent(26, 0.15f),
					Top = StyleDimension.FromPixels(22 + completedOffset)
				};

				_completedQuestDetails.Append(text);
				completedOffset += 22;
			}
			else if (quest.Active) // Properly display active quests
			{
				UISelectableQuest selectableQuest = new(quest, _questDetails);
				selectableQuest.Left.Set(0, 0.15f);
				selectableQuest.Top.Set(100 + offset, 0);
				selectableQuest.OnLeftClick += (_, _) => SelectQuest(quest.FullName);
				selectableQuest.OnRightClick += (_, _) => PinQuest(quest.FullName);
				_questDetails.Append(selectableQuest);
				offset += 22;
			}

			if (quest.FullName == player.PinnedQuest)
			{
				Asset<Texture2D> icon = TextureAssets.Cursors[3];
				UIImage star = new(icon)
				{
					Left = new(0, 0.15f),
					Top = new((quest.Completed ? 22 + completedOffset : 100 + offset) - icon.Height(), 0)
				};

				_questDetails.Append(star);
			}
		}

		if (player.PinnedQuest != null) //Automatically select the pinned quest
		{
			SelectQuest(player.PinnedQuest);
		}
	}

	public void PinQuest(string questName)
	{
		QuestModPlayer player = Main.LocalPlayer.GetModPlayer<QuestModPlayer>();
		player.PinnedQuest = (player.PinnedQuest == questName) ? null : questName;

		AppendQuests();
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