using ReLogic.Content;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Common.Mechanics;

/// <summary> Boilerplate for an object that can be allocated to, like <see cref="SkillPassive"/> and <see cref="Passive"/>.<para/>
/// Use <see cref="ILevel"/> if the object can gain levels. </summary>
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

	private Vector2 _size;
	public Vector2 Size
	{
		get
		{
			if (_size != Vector2.Zero)
			{
				return _size;
			}

			return _size = Texture.Size();
		}
	}

	/// <summary> Shorthand for Type name. </summary>
	public virtual string Name => GetType().Name;

	public virtual string TexturePath { get; }
	/// <summary> Name to be used in ALL display situations. </summary>
	public abstract string DisplayName { get; }
	/// <summary> Tooltip to be used in ALL display situations. </summary>
	public abstract string DisplayTooltip { get; }

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