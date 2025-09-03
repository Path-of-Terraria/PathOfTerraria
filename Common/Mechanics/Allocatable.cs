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
	public virtual Vector2 Size
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

	public Vector2 TreePos;
	public int Level;
	public int MaxLevel = 1;

	/// <summary> Shorthand for checking <see cref="Level"/> > 0.</summary>
	public bool Allocated => Level > 0;

	/// <summary> The amount of neighbouring nodes that must be allocated before this node becomes available itself. </summary>
	public int RequiredAllocatedEdges { get; set; } = 1;

	/// <summary> Shorthand for Type name. </summary>
	public virtual string Name => GetType().Name;

	public virtual string TexturePath { get; }
	/// <summary> Name to be used in ALL display situations. </summary>
	public abstract string DisplayName { get; }
	/// <summary> Tooltip to be used in <b>ALL</b> display situations. </summary>
	public abstract string DisplayTooltip { get; }

	/// <summary> Should check <see cref="CanAllocate"/> before being called. </summary>
	public virtual void OnAllocate(Player player)
	{
		Level = Math.Min(Level + 1, MaxLevel);
	}

	/// <summary> Should check <see cref="CanDeallocate"/> before being called. </summary>
	public virtual void OnDeallocate(Player player)
	{
		Level = Math.Max(Level - 1, 0);
	}

	public virtual bool CanAllocate(Player player)
	{
		return Level < MaxLevel;
	}

	public virtual bool CanDeallocate(Player player)
	{
		return Allocated;
	}

	public virtual void Draw(SpriteBatch spriteBatch, Vector2 position) { }
}