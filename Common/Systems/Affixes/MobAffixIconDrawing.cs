using PathOfTerraria.Common.Systems.MobSystem;
using System.Collections.Generic;

namespace PathOfTerraria.Common.Systems.Affixes;

internal class MobAffixIconDrawing : GlobalNPC
{
	public override bool InstancePerEntity => true;

	private float _hoverStrength = 0;

	public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		List<MobAffix> affixes = npc.GetGlobalNPC<ArpgNPC>().Affixes;

		if (affixes.Count == 0)
		{
			return;
		}

		_hoverStrength = MathHelper.Lerp(_hoverStrength, npc.Hitbox.Contains(Main.MouseWorld.ToPoint()) ? 1 : 0, 0.1f);

		float offset = affixes.Count / 2f - 0.5f;
		float scale = MathHelper.Lerp(1f, 1.5f, _hoverStrength);
		float opacity = MathHelper.Lerp(0.6f, 1f, _hoverStrength);

		for (int i = 0; i < affixes.Count; i++)
		{
			MobAffix affix = affixes[i];

			Vector2 position = npc.Center - screenPos + new Vector2((i * 20 - offset * 20) * scale, -npc.height / 2 - 12);
			spriteBatch.Draw(affix.Icon.Value, position, null, drawColor * opacity, 0f, affix.Icon.Size() / 2f, scale, SpriteEffects.None, 0);
		}
	}
}
