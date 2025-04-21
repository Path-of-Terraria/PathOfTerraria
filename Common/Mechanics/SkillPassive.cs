using PathOfTerraria.Common.Systems.Skills;
using Terraria.Localization;

namespace PathOfTerraria.Common.Mechanics;

public abstract class SkillPassive(SkillTree tree) : SkillNode(tree)
{
	public override string TexturePath => $"{PoTMod.ModName}/Assets/SkillPassives/" + Name;
	public override string DisplayName => Language.GetTextValue("Mods.PathOfTerraria.SkillPassives." + Name + ".Name");
	public override string Tooltip => Language.GetTextValue("Mods.PathOfTerraria.SkillPassives." + Name + ".Tooltip");

	public int Level;
	public int MaxLevel;

	public override void Draw(SpriteBatch spriteBatch, Vector2 position)
	{
		Texture2D texture = Texture.Value;
		Color color = Color.Gray;

		if (CanAllocate(Main.LocalPlayer))
		{
			color = Color.Lerp(Color.Gray, Color.White, (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.5f + 0.5f);
		}

		if (Level > 0)
		{
			color = Color.White;
		}

		spriteBatch.Draw(texture, position - texture.Size() / 2, color);
	}

	/// <summary> The effects of this skill passive. </summary>
	public virtual void PassiveEffects(ref SkillBuff buff) { }

	public override void OnAllocate(Player player)
	{
		Level++;
		Tree.Points--;
	}

	public override void OnDeallocate(Player player)
	{
		Level--;
		Tree.Points++;
	}

	public override bool CanAllocate(Player player)
	{
		return Tree.Points > 0;
	}

	public override bool CanDeallocate(Player player)
	{
		return Level > 0;
	}
}