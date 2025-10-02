using System.ComponentModel;
using Newtonsoft.Json;
using ReLogic.Reflection;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Utilities.Terraria;

[TypeConverter(typeof(ToFromStringConverter<MusicDefinition>))]
internal class MusicDefinition : EntityDefinition
{
	public static readonly Func<TagCompound, MusicDefinition> DESERIALIZER = Load;

	private static IdDictionary FixedDictionary
	{
		get
		{
			if (!MusicID.Search.ContainsId(1)) { MusicID.Search = IdDictionary.Create<MusicID, short>(); }
			return MusicID.Search;
		}
	}

	[JsonIgnore] public override int Type => Name == "None" ? 0 : (FixedDictionary.TryGetId(Mod != "Terraria" ? $"{Mod}/{Name}" : Name, out int id) ? id : -1);
	[JsonIgnore] public override bool IsUnloaded => Type <= -1;
	[JsonIgnore] public override string DisplayName => Type <= 0 ? "None" : (IsUnloaded ? Language.GetTextValue("Mods.ModLoader.Unloaded") : FixedDictionary.GetName(Type));

	public MusicDefinition() : base() { }
	public MusicDefinition(int type) : base(FixedDictionary.TryGetName(type, out string name) ? name : "None") { }
	public MusicDefinition(string key) : base(key) { }
	public MusicDefinition(string mod, string name) : base(mod, name) { }

	public static MusicDefinition FromString(string s) { return new(s); }
	public static MusicDefinition Load(TagCompound tag) { return new(tag.GetString("mod"), tag.GetString("name")); }
}
