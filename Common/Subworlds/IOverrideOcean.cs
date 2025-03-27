namespace PathOfTerraria.Common.Subworlds;

/// <summary>
/// Allows <see cref="MappingWorld"/>s to override the Ocean biome when desired.
/// </summary>
internal interface IOverrideOcean
{
	/// <summary>
	/// Runs when the Ocean is active and needs to be overriden.<br/>
	/// <see cref="DisableOceanSystem"/> manages this, 
	/// running this method in both <see cref="DisableOceanSystem.OverrideOceanPostUpdateBiomes(On_Player.orig_UpdateBiomes, Player)"/>,<br/>and in
	/// <see cref="DisableOceanSystem.OveriddeOceanPostMusic(Terraria.Audio.On_LegacyAudioSystem.orig_UpdateMisc, Terraria.Audio.LegacyAudioSystem)"/>.<br/>
	/// This makes sure the game is consistent with the music and background.
	/// </summary>
	public void OverrideOcean();
}
