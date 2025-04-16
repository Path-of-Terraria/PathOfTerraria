namespace PathOfTerraria.Common.Mechanics;

public abstract class SkillAugment
{
	public virtual string Name => GetType().Name;
	public string Texture => $"{PoTMod.ModName}/Assets/SkillAugments/" + Name;

	public virtual void Draw(SpriteBatch spriteBatch, Vector2 position)
	{
		Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
		spriteBatch.Draw(texture, position - texture.Size() / 2, Color.White);
	}

	public virtual bool CanAllocate(Player player)
	{
		return true;
	}

	public virtual bool CanDeallocate(Player player)
	{
		return true;
	}
}