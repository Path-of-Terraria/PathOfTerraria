using System.IO;
using Terraria.GameContent;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.NPCs.Overhealth;

internal class OverhealthNPC : GlobalNPC
{
	public override bool InstancePerEntity => true;

	private int _overhealth = 0;
	private int _maxOverhealth = 0;

	public void SetOverhealth(int value)
	{
		if (_overhealth < value)
		{
			_overhealth = value;
		}

		_maxOverhealth = _overhealth;
	}

	public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
	{
		modifiers.ModifyHitInfo += (ref NPC.HitInfo info) => HitOverhealth(npc, ref info);
	}

	public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
	{
		modifiers.ModifyHitInfo += (ref NPC.HitInfo info) => HitOverhealth(npc, ref info);
	}

	private void HitOverhealth(NPC npc, ref NPC.HitInfo info)
	{
		if (_overhealth <= 0)
		{
			return;
		}

		int delta = Math.Min(_overhealth, info.Damage - 1);
		_overhealth -= delta;
		info.Damage -= delta;

		CombatText.NewText(npc.Hitbox, Color.Purple, delta);
	}

	public override bool? DrawHealthBar(NPC npc, byte hbPosition, ref float scale, ref Vector2 position)
	{
		if (_overhealth <= 0)
		{
			return null;
		}

		Vector2 basePos = position;
		npc.ModNPC?.DrawHealthBar(hbPosition, ref scale, ref basePos);

		Texture2D hb1 = TextureAssets.Hb1.Value;
		Texture2D hb2 = TextureAssets.Hb2.Value;
		Vector2 drawPos = basePos + new Vector2(0, 16 * scale) - Main.screenPosition;
		Color color = Color.MediumPurple;

		// Back bar
		Main.spriteBatch.Draw(hb2, drawPos, null, Color.White, 0f, hb1.Size() / 2f, scale, SpriteEffects.None, 0);

		// Actual "health"
		Rectangle source = new(0, 0, (int)(hb1.Width * (_overhealth / (float)_maxOverhealth)), hb1.Height);
		Main.spriteBatch.Draw(hb1, drawPos, source, color, 0f, hb1.Size() / 2f, scale, SpriteEffects.None, 0);
		return null;
	}
}
