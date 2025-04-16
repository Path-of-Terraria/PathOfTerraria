using PathOfTerraria.Common.Systems.Skills;
using ReLogic.Content;

namespace PathOfTerraria.Common.Mechanics;

public abstract class Allocatable(SkillTree tree)
{
	private Asset<Texture2D> _texture;
	public Asset<Texture2D> Texture //Cache the texture to avoid constant requests
	{
		get
		{
			if (_texture != null)
			{
				return _texture;
			}

			return _texture = ModContent.Request<Texture2D>(TexturePath, AssetRequestMode.ImmediateLoad);
		}
	}

	public virtual string TexturePath { get; }
	public virtual string Name => GetType().Name;
	public abstract string Tooltip { get; }

	public SkillTree Tree = tree;

	/// <summary> Should check <see cref="CanAllocate"/> before being called. </summary>
	public virtual void OnAllocate(Player player)
	{
		if (Tree.Points > 0)
		{
			Tree.Points--;
		}
	}

	/// <summary> Should check <see cref="CanDeallocate"/> before being called. </summary>
	public virtual void OnDeallocate(Player player)
	{
		Tree.Points++;
	}

	public virtual bool CanAllocate(Player player)
	{
		return Tree.Points > 0;
	}

	public virtual bool CanDeallocate(Player player)
	{
		return true;
	}

	public virtual void Draw(SpriteBatch spriteBatch, Vector2 position) { }
}