using PathOfTerraria.Core.Graphics.Filters;
using ReLogic.Content;

namespace PathOfTerraria.Content.NPCs.BossDomain.SunDevourerDomain;

public sealed class SunDevourerSceneEffect : ModSceneEffect
{
	/// <summary>
	///		Gets or sets the strength of the vignette effect.
	/// </summary>
	public float Strength { get; private set; }
	
	public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

	public override void SpecialVisuals(Player player, bool isActive) 
	{
		base.SpecialVisuals(player, isActive);

		Strength = MathHelper.SmoothStep(Strength, isActive ? 1f : 0f, 0.25f);
		
		if (isActive)
		{
			var shader = VignetteFilter.Vignette.Value;
			
			shader.Parameters["uOpacity"].SetValue(1f * Strength);
			shader.Parameters["strength"].SetValue(0.8f * Strength);
			shader.Parameters["curvature"].SetValue(0.5f * Strength);
			shader.Parameters["innerRadius"].SetValue(0.5f * Strength);
			shader.Parameters["outerRadius"].SetValue(1.2f * Strength);
		}

		player.ManageSpecialBiomeVisuals($"{PoTMod.ModName}:Vignette", isActive);
	}

	public override bool IsSceneEffectActive(Player player)
	{
		return NPC.AnyNPCs(ModContent.NPCType<SunDevourerNPC>());
	}
}