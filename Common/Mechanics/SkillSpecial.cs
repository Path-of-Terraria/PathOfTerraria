using PathOfTerraria.Common.Systems.Skills;
using Terraria.Localization;

namespace PathOfTerraria.Common.Mechanics;

public abstract class SkillSpecial(SkillTree tree) : Allocatable(tree)
{
	public override string TexturePath => $"{PoTMod.ModName}/Assets/SkillSpecials/" + Name;
	public override string Tooltip => Language.GetTextValue("Mods.PathOfTerraria.SkillSpecials." + Name + ".Description");

	public override void Draw(SpriteBatch spriteBatch, Vector2 position)
	{
		Texture2D texture = Texture.Value;
		spriteBatch.Draw(texture, position, null, Color.White, 0, texture.Size() / 2, 1, default, 0);

		if (Tree.Specialization == this)
		{
			float pulse = (float)Math.Sin(Main.timeForVisualEffects / 50f) * .25f + .5f;

			Texture2D glow = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/GlowAlpha").Value;
			var glowColor = new Color(255, 230, 150) { A = 0 };
			spriteBatch.Draw(glow, position, null, glowColor * pulse, 0, glow.Size() / 2, 1, default, 0);

			Texture2D outline = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/SpecialFrame_Outline").Value;
			spriteBatch.Draw(outline, position, null, Color.White * pulse, 0, outline.Size() / 2, 1, default, 0);
		}
	}

	public override void OnAllocate(Player player)
	{
		Tree.Specialization = this;
	}

	public override void OnDeallocate(Player player)
	{
		Tree.Specialization = null;
	}

	/// <summary> Whether this skill specialization can be used. </summary>
	public override bool CanAllocate(Player player)
	{
		return Tree.Specialization is null;
	}

	/// <summary> Whether this skill specialization can be refunded. </summary>
	public override bool CanDeallocate(Player player)
	{
		return Tree.Specialization == this;
	}
}