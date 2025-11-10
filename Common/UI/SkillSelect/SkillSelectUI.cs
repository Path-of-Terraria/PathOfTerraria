using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.UI.Components;
using PathOfTerraria.Common.UI.Guide;
using PathOfTerraria.Common.UI.SkillsTree;
using PathOfTerraria.Core.UI;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.SkillSelect;

internal class SkillSelectUI(Point16 topAnchor, int slotToChange) : UIState
{
	public const string Identifier = "Skill Select";

	internal readonly int SlotToChange = slotToChange;

	private readonly Point16 _topAnchor = topAnchor;

	public override void OnInitialize()
	{
		Asset<Texture2D> back = ModContent.Request<Texture2D>("PathOfTerraria/Assets/UI/TooltipBackgroundPlain");
		UIPanel panel = new(back, back, 32, 32)
		{
			BackgroundColor = Color.White,
			BorderColor = Color.White,
			PaddingLeft = 0,
			PaddingRight = 0
		};
		panel.SetDimensions((0, _topAnchor.X), (0, _topAnchor.Y), (0, 280), (0, 300));
		panel.AddComponent(new UIBlockMouse());
		Append(panel);

		UIGrid grid = new()
		{
			Width = StyleDimension.FromPixelsAndPercent(-24, 1),
			Height = StyleDimension.FromPixelsAndPercent(0, 1),
			HAlign = 1f,
			ListPadding = 8
		};
		panel.Append(grid);

		UIScrollbar bar = new();
		bar.SetDimensions((1, -30), (0, 14), (0, 18), (1, 0));
		grid.SetScrollbar(bar);
		panel.Append(bar);

		List<SkillSelectionElement> elements = [];

		foreach (Skill skill in ModContent.GetContent<Skill>())
		{
			Skill clone = skill.Clone();

			var element = new SkillSelectionElement(clone, (parent, self) =>
			{
				if (Main.LocalPlayer.GetModPlayer<SkillCombatPlayer>().TryAddSkill(clone, false, SlotToChange))
				{
					OnDeactivate();

					Main.LocalPlayer.GetModPlayer<TutorialPlayer>().TutorialChecks.Add(TutorialCheck.SelectedSkill);
				}
			});

			elements.Add(element);
		}

		grid.AddRange(elements);
	}

	public override void OnDeactivate()
	{
		RemoveAllChildren();
	}
}
