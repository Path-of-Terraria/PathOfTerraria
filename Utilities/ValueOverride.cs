using System.Runtime.CompilerServices;

namespace PathOfTerraria.Utilities;

/// <summary> Temporarily overrides a given reference. Usage: <code>using var _ = ValueOverride.Create(ref value, newValue);</code> </summary>
internal static class ValueOverride
{
	public static ValueOverride<T> Create<T>(ref T reference, T value)
	{
		return new(ref reference, value);
	}
}

/// <inheritdoc cref="ValueOverride"/>
internal ref struct ValueOverride<T>
{
	private T oldValue;
	private ref T reference;

	public ValueOverride(ref T reference, T value)
	{
		this.reference = ref reference;
		oldValue = reference;
		reference = value;
	}

	public void Dispose()
	{
		if (!Unsafe.IsNullRef(ref reference)) {
			reference = oldValue;
			oldValue = default!;
			reference = ref Unsafe.NullRef<T>();
		}
	}
}
