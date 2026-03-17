using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.ModPlayers.SkillPlayers;
using PathOfTerraria.Common.UI.Utilities;
using PathOfTerraria.Content.SkillPassives;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.SkillsTree;

internal class SkillTreeInnerPanel : AllocatableInnerPanel
{
	public override string TabName => "SelectedSkillTree";

	private readonly Skill _skill;

	private int _resetTimer = 0;

	public SkillTreeInnerPanel(Skill skill) : base()
	{
		_skill = skill;
		Width = Height = StyleDimension.Fill;
	}

	public override void SafeUpdate(GameTime gameTime)
	{
		base.SafeUpdate(gameTime);

		_resetTimer--;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		AvailablePassivePointsText.DrawResettablePoints(spriteBatch, _skill.Tree.Points, GetRectangle().TopLeft() + new Vector2(35, 35), ref _resetTimer, () =>
		{
			foreach (SkillNode node in _skill.Tree.Nodes)
			{
				while (node is not Anchor && node.Level > 0)
				{
					node.OnDeallocate(Main.LocalPlayer);
				}
			}

			Main.LocalPlayer.GetModPlayer<SkillCombatPlayer>().SaveData([]);
		});

		base.Draw(spriteBatch);
	}
}