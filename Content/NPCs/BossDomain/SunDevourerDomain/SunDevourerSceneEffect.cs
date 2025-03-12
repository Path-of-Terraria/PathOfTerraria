using PathOfTerraria.Core.Graphics.Shaders;
using ReLogic.Content;

namespace PathOfTerraria.Content.NPCs.BossDomain.SunDevourerDomain;

public sealed class SunDevourerSceneEffect : ModSceneEffect
{
	public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

	public override void SpecialVisuals(Player player, bool isActive) 
	{
		base.SpecialVisuals(player, isActive);
		
		if (isActive)
		{
			var shader = VignetteLoader.Vignette.Value;
			
			shader.Parameters["uOpacity"].SetValue(1f);
			shader.Parameters["strength"].SetValue(0.8f);
			shader.Parameters["curvature"].SetValue(0.5f);
			shader.Parameters["innerRadius"].SetValue(0.5f);
			shader.Parameters["outerRadius"].SetValue(1.2f);
		}

		player.ManageSpecialBiomeVisuals($"{PoTMod.ModName}:Vignette", isActive);
	}

	public override bool IsSceneEffectActive(Player player)
	{
		return NPC.AnyNPCs(ModContent.NPCType<SunDevourerNPC>());
	}
}