using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.ModPlayers;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.SkillsTree;

internal class SkillSelectionElement : UIElement
{
	private readonly Skill _skill;
	private readonly SkillSelectionPanel _parentPanel;

	public SkillSelectionElement(Skill skill, int index, SkillSelectionPanel parentPanel)
	{
		_skill = skill;
		_parentPanel = parentPanel;
		Width.Set(skill.Size.X, 0);
		Height.Set(skill.Size.Y, 0);
		Top.Set(150, 0);
		Left.Set(25 + 75 * index, 0);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);

		Texture2D tex = ModContent.Request<Texture2D>(_skill.Texture).Value;

		if (tex == null)
		{
			return;
		}

		Vector2 position = GetDimensions().Position() + new Vector2(Width.Pixels / 2, Height.Pixels / 2);
		spriteBatch.Draw(tex, position, null, _skill.CanEquipSkill(Main.LocalPlayer) ? Color.White : Color.Gray, 0f, tex.Size() / 2f, 1f, SpriteEffects.None, 0f);
	}
	
	public override void LeftClick(UIMouseEvent evt)
	{
		SkillPlayer skillPlayer = Main.LocalPlayer.GetModPlayer<SkillPlayer>();
		Main.NewText("Clicked on " + _skill.Name);
		skillPlayer.TryAddSkill(_skill);
		_parentPanel.SelectedSkill = _skill;
		_parentPanel.DrawSkillTree();
	}

	public override void RightClick(UIMouseEvent evt)
	{
		Main.NewText("Right Clicked on " + _skill.Name);
		SkillPlayer skillPlayer = Main.LocalPlayer.GetModPlayer<SkillPlayer>();
		skillPlayer.TryRemoveSkill(_skill);
	}
}