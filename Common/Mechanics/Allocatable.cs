using PathOfTerraria.Common.Systems.Skills;
using ReLogic.Content;
using System.Linq;

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

	public virtual string TexturePath { get; }
	public virtual string Name => GetType().Name;
	public abstract string Tooltip { get; }

	internal static Skill ViewedSkill;
	/// <summary> The tree of the currently viewed skill (<see cref="ViewedSkill"/>).</summary>
	public static SkillTree Tree => ViewedSkill.Tree;

	/// <summary> Should check <see cref="CanAllocate"/> before being called. </summary>
	public virtual void OnAllocate(Player player)
	{
		Tree.Points--;
	}

	/// <summary> Should check <see cref="CanDeallocate"/> before being called. </summary>
	public virtual void OnDeallocate(Player player)
	{
		Tree.Points++;
	}

	public virtual bool CanAllocate(Player player)
	{
		return Tree.Points > 0 && Tree.Edges.Any(e => e.Contains(this) && (e.Other(this) is not SkillPassive p || p.Level > 0));
	}

	public virtual bool CanDeallocate(Player player)
	{
		return true;
	}

	public virtual void Draw(SpriteBatch spriteBatch, Vector2 position) { }
}