using ReLogic.Content;

namespace PathOfTerraria.Content.NPCs.BossDomain.SunDevourerDomain;

public sealed class SunDevourerSceneEffect : ModSceneEffect
{
	/// <summary>
	///		Gets or sets the vignette shader asset.
	/// </summary>
	public static Asset<Effect> VignetteShader { get; private set; }
	
	public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

	public override void Load()
	{
		base.Load();

		VignetteShader = ModContent.Request<Effect>($"{PoTMod.ModName}/Assets/Effects/Vignette", AssetRequestMode.ImmediateLoad);
	}

	public override void SpecialVisuals(Player player, bool isActive) 
	{
		base.SpecialVisuals(player, isActive);
		
		if (isActive)
		{
			var shader = VignetteShader.Value;
			
			shader.Parameters["uOpacity"].SetValue(1f);
			shader.Parameters["strength"].SetValue(0.8f);
			shader.Parameters["curvature"].SetValue(0.5f);
			shader.Parameters["innerRadius"].SetValue(0.5f);
			shader.Parameters["outerRadius"].SetValue(1.2f);
		}

		player.ManageSpecialBiomeVisuals($"{PoTMod.ModName}:{nameof(VignetteShader)}", isActive);
	}

	public override bool IsSceneEffectActive(Player player)
	{
		return NPC.AnyNPCs(ModContent.NPCType<SunDevourerNPC>());
	}
}