using SubworldLibrary;

#nullable enable

namespace PathOfTerraria.Core.Subworlds;

/// <summary> Unified interface used for synchronizing data between subworlds. </summary>
internal interface ISubworldSync : ICopyWorldData
{
    void ICopyWorldData.CopyMainWorldData()
    {
        ExportMainWorldData();
    }
    void ICopyWorldData.ReadCopiedMainWorldData()
    {
        ImportMainWorldData();
    }
    
    /// <summary> Called by default implementations of <see cref="ExportMainWorldData"/> and <see cref="ExportSubworldData"/>. </summary>
    void ExportSharedData() { }
    /// <summary> Called by default implementations of <see cref="ImportMainWorldData"/> and <see cref="ImportSubworldData"/>. </summary>
    void ImportSharedData() { }

    /// <inheritdoc cref="ICopyWorldData.CopyMainWorldData"/>
    void ExportMainWorldData() { ExportSharedData(); }
    /// <inheritdoc cref="ICopyWorldData.ReadCopiedMainWorldData"/>
    void ImportMainWorldData() { ImportSharedData(); }

    /// <inheritdoc cref="Subworld.CopySubworldData"/>
    void ExportSubworldData(Subworld subworld) { ExportSharedData(); }
    /// <inheritdoc cref="Subworld.ReadCopiedSubworldData"/>
    void ImportSubworldData(Subworld subworld) { ImportSharedData(); }
}

internal sealed class SubworldHooks : ModSystem
{
    private static Action<Subworld>? exportSubworldData;
    private static Action<Subworld>? importSubworldData;

    public static void ExportSubworldData(Subworld subworld) { exportSubworldData?.Invoke(subworld); }
    public static void ImportSubworldData(Subworld subworld) { importSubworldData?.Invoke(subworld); }
    
	public override void PostSetupContent()
	{
        exportSubworldData = null;
        importSubworldData = null;
    
		foreach (ISubworldSync impl in ModContent.GetContent<ISubworldSync>())
        {
            exportSubworldData += impl.ExportSubworldData;
            importSubworldData += impl.ImportSubworldData;
        }
	}
	public override void Unload()
	{
		exportSubworldData = null;
		importSubworldData = null;
	}
}
