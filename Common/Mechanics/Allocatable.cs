using ReLogic.Content;

namespace PathOfTerraria.Common.Mechanics;

public abstract class Allocatable
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

	/// <summary> Shorthand for the Type name. </summary>
	public string Name => GetType().Name;

	public virtual string TexturePath { get; }
	public abstract string DisplayName { get; }
	public abstract string Tooltip { get; }

	/// <summary> Should check <see cref="CanAllocate"/> before being called. </summary>
	public virtual void OnAllocate(Player player) { }

	/// <summary> Should check <see cref="CanDeallocate"/> before being called. </summary>
	public virtual void OnDeallocate(Player player) { }

	public virtual bool CanAllocate(Player player)
	{
		return true;
	}

	public virtual bool CanDeallocate(Player player)
	{
		return true;
	}

	public virtual void Draw(SpriteBatch spriteBatch, Vector2 position) { }
}