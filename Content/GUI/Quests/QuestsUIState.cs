using System.Collections.Generic;
using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Core.Systems.Questing;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.Quests;

public class QuestsUIState : DraggableSmartUi
{
	private QuestDetailsPanel _questDetails;
	public const float SELECTED_OPACITY = 0.25f;
	public const float HOVERED_OPACITY = 0.1f;
	
	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
	}

	public override int DepthPriority => 2;

	public void Toggle()
	{
		if (IsVisible)
		{
			IsVisible = false;
			_questDetails.Remove();
			return;
		}

		if (!HasChild(_questDetails))
		{
			RemoveAllChildren();

			_questDetails = new QuestDetailsPanel
			{
				Width = StyleDimension.FromPixels(1200),
				Height = StyleDimension.FromPixels(900),
				HAlign = 0.5f,
				VAlign = 0.5f
			};

			Append(_questDetails);

			CloseButton = new UIImageButton(ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/CloseButton"));
			CloseButton.Left.Set(-450, 1f);
			CloseButton.Top.Set(80, 0f);
			CloseButton.Width.Set(38, 0);
			CloseButton.Height.Set(38, 0);
			CloseButton.OnLeftClick += (a, b) =>
			{
				IsVisible = false;
				SoundEngine.PlaySound(SoundID.MenuClose, Main.LocalPlayer.Center);
			};
			CloseButton.SetVisibility(1, 1);

			Append(CloseButton);
		}

		IsVisible = true;
		DrawQuests();
	}
	
	private void DrawQuests()
	{
		int offset = 0;
		QuestModPlayer player = Main.LocalPlayer.GetModPlayer<QuestModPlayer>();
		foreach (Quest quest in player.GetAllQuests())
		{
			UISelectableQuest selectableQuest = new(quest, this);
			selectableQuest.Left.Set(200, 0);
			selectableQuest.Top.Set(200 + offset, 0);
			selectableQuest.Width = StyleDimension.FromPixels(200);
			_questDetails.Append(selectableQuest);
			offset += 22;
		}
	}
	
	public void SelectQuest(Quest quest)
	{
		_questDetails.ViewedQuest = quest;
	}
}