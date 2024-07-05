using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Core.Systems.Questing;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.Quests;

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
		GUIDebuggingTools.DrawGuiBorder(spriteBatch, GetDimensions(), Color.Green);
#endif
	}

	public override int DepthPriority => 2;

	public void Toggle()
	{
		if (IsVisible)
		{
			IsVisible = false;
			_questDetails.Remove();
			Visible = false;
			return;
		}
		
		base.CreateMainPanel(false, null, false);
		RemoveAllChildren();
		List<Quest> quests = Main.LocalPlayer.GetModPlayer<QuestModPlayer>().GetAllQuests();
		if (quests.Count > 0)
		{
			_questDetails = new QuestDetailsPanel
			{
				Width = StyleDimension.FromPercent(1),
				Height = StyleDimension.FromPercent(1),
				ViewedQuest = quests.First()
			};
			if (_questDetails.ViewedQuest != null)
			{
				Append(_questDetails);
				_questDetails.PopulateQuestSteps();
			}    
		}

		_closeButton = new UIImageButton(ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/CloseButton"));
		_closeButton.Left.Set(GetCloseButtonLeft(), 0.73f);
		_closeButton.Top.Set(GetCloseButtonTop(), 0f);
		_closeButton.Width.Set(38, 0);
		_closeButton.Height.Set(38, 0);
		_closeButton.OnLeftClick += (a, b) =>
		{
			IsVisible = false;
			SoundEngine.PlaySound(SoundID.MenuClose, Main.LocalPlayer.Center);
		};
		_closeButton.SetVisibility(1, 1);
		Append(_questDetails);
		Append(_closeButton);

		IsVisible = true;
		Visible = true;
		DrawQuests();
	}
	
	private void DrawQuests()
	{
		int offset = 0;
		QuestModPlayer player = Main.LocalPlayer.GetModPlayer<QuestModPlayer>();
		foreach (Quest quest in player.GetAllQuests())
		{
			UISelectableQuest selectableQuest = new(quest, this);
			selectableQuest.Left.Set(GetQuestNameLeft(), 0);
			selectableQuest.Top.Set(GetQuestNameTop() + offset, 0);
			_questDetails.Append(selectableQuest);
			offset += 22;
		}
	}

	private static float GetQuestNameLeft()
	{
		float screenWidth = Main.screenWidth / 1.12f;
		return screenWidth switch
		{
			//4k or 4k Wide
			>= 2160 => 360,
			//1440p+
			>= 1440 => 250,
			_ => 150
		};
	}
	
	private static float GetQuestNameTop()
	{
		float screenWidth = Main.screenWidth / 1.12f;
		return screenWidth switch
		{
			//4k or 4k Wide
			>= 2160 => 325,
			//1440p+
			>= 1440 => 250,
			_ => 130
		};
	}
	
	private static float GetCloseButtonTop()
	{
		float screenWidth = Main.screenWidth / 1.12f;
		return screenWidth switch
		{
			//4k or 4k Wide
			>= 2160 => 330,
			//1440p+
			>= 1440 => 235,
			_ => 135
		};
	}
	
	private static float GetCloseButtonLeft()
	{
		float screenWidth = Main.screenWidth / 1.12f;
		return screenWidth switch
		{
			//4k or 4k Wide
			>= 2160 => 200,
			//1440p+
			>= 1440 => 100,
			_ => 0
		};
	}

	public void SelectQuest(Quest quest)
	{
		_questDetails.ViewedQuest = quest;
	}
}