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
	//private QuestDetailsPanel _questDetails;
	//private QuestDetailsPanel _completedQuestDetails;

	public const float SelectedOpacity = 0.25f;
	public const float HoveredOpacity = 0.1f;

	public static string ViewedQuest { get; private set; }

	private UIImageButton _closeButton;

	private UIList _questDetails;
	private UIList _questList;
	private UIList _completedQuestList;

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

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);

		QuestModPlayer player = Main.LocalPlayer.GetModPlayer<QuestModPlayer>();
		if (player.GetQuestCount() != 0 && !string.IsNullOrEmpty(ViewedQuest))
		{
			string name = player.QuestsByName[ViewedQuest].DisplayName.Value;
			Utils.DrawBorderStringBig(spriteBatch, name, GetDimensions().Center() + new Vector2(175, -285), Color.White, 0.5f, 0.5f, 0.35f);
		}
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
		ViewedQuest = null;

		if (IsVisible)
		{
			IsVisible = false;
			_questDetails.Remove();
			return;
		}

		Main.LocalPlayer.GetModPlayer<TutorialPlayer>().TutorialChecks.Add(TutorialCheck.OpenedQuestBook);
		ModContent.GetInstance<SmartUiLoader>().ClearMutuallyExclusive<QuestsUIState>();

		RemoveAllChildren();
		base.CreateMainPanel(false, new Point(750, 600), false, true);
		Quest quest = Main.LocalPlayer.GetModPlayer<QuestModPlayer>().QuestsByName.FirstOrDefault(x => x.Value.Active).Value;

		Panel.Append(new UIImage(ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/QuestBookBackground"))
		{
			Left = new(0, -0.175f),
			Top = new(0, -0.075f)
		});

		Panel.Append(_questDetails = new()
		{
			Left = StyleDimension.FromPercent(0.55f),
			Top = StyleDimension.FromPercent(0.1f),
			Width = StyleDimension.FromPercent(0.45f),
			Height = StyleDimension.FromPercent(0.9f)
		});
		_questDetails.SetScrollbar(new());

		Panel.Append(_questList = new()
		{
			Left = StyleDimension.FromPercent(0.05f),
			Top = StyleDimension.FromPercent(0.1f),
			Width = StyleDimension.FromPercent(0.5f),
			Height = StyleDimension.FromPercent(0.4f)
		});
		_questList.SetScrollbar(new());

		//Separate completed quests from others
		Panel.Append(_completedQuestList = new()
		{
			Left = StyleDimension.FromPercent(0.05f),
			Top = StyleDimension.FromPercent(0.5f),
			Width = StyleDimension.FromPercent(0.5f),
			Height = StyleDimension.FromPercent(0.5f)
		});
		_completedQuestList.SetScrollbar(new());

		_closeButton = new UIImageButton(ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/CloseButton"))
		{
			Left = new(-50, 1f),
			Top = new(10, 0f),
			Width = new(38, 0),
			Height = new(38, 0)
		};
		_closeButton.OnLeftClick += (a, b) =>
		{
			IsVisible = false;
			SoundEngine.PlaySound(SoundID.MenuClose, Main.LocalPlayer.Center);
		};
		_closeButton.SetVisibility(1, 1);

		Panel.Append(_closeButton);

		IsVisible = true;
		Visible = true;
		PopulateQuests();
		Recalculate();
	}

	private void PopulateQuestSteps(Quest quest)
	{
		_questDetails.Clear();

		for (int i = 0; i < quest.QuestSteps.Count; i++)
		{
			QuestStep step = quest.QuestSteps[i];
			if (step.NoUI)
			{
				continue;
			}

			_questDetails.Add(new UISelectableQuestStep(i, quest)
			{
				Width = StyleDimension.Fill,
				Height = StyleDimension.FromPixels(step.LineCount * 22)
			});
		}		
	}

	private void PopulateQuests()
	{
		_questList.Clear();
		_completedQuestList.Clear();

		QuestModPlayer player = Main.LocalPlayer.GetModPlayer<QuestModPlayer>();

		foreach (Quest quest in player.QuestsByName.Values.OrderByDescending(x => x.FullName == player.PinnedQuest))
		{
			if (quest.Completed) // Gray out completed quests
			{
				_completedQuestList.Add(new UIText(quest.DisplayName.Value, 0.7f)
				{
					TextColor = Color.Gray,
					ShadowColor = Color.Transparent,
					Left = StyleDimension.FromPixelsAndPercent(26, 0),
					Height = new(22, 0)
				});
			}
			else if (quest.Active)
			{
				UISelectableQuest selectable = new(quest);
				selectable.OnLeftClick += (a, b) => SelectQuest(quest.FullName);
				selectable.OnRightClick += (a, b) => PinQuest(quest.FullName);

				_questList.Add(selectable);

				if (quest.FullName == player.PinnedQuest)
				{
					Asset<Texture2D> icon = TextureAssets.Cursors[3];
					UIImage star = new(icon)
					{
						Left = new(-icon.Width() / 2, 0),
						Top = new(-icon.Height() / 2, 0)
					};

					selectable.Append(star);
				}
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

		PopulateQuests();
		PopulateQuestSteps(Main.LocalPlayer.GetModPlayer<QuestModPlayer>().QuestsByName[questName]);
	}

	public void SelectQuest(string questName)
	{
		ViewedQuest = questName;
		
		PopulateQuestSteps(Main.LocalPlayer.GetModPlayer<QuestModPlayer>().QuestsByName[questName]);
		Recalculate();
	}
}