using System.Runtime.CompilerServices;

#nullable enable

namespace PathOfTerraria.Utilities.Xna;

internal record struct SpriteBatchArgs
(
	SpriteSortMode SortMode,
	BlendState BlendState,
	SamplerState SamplerState,
	DepthStencilState DepthStencilState,
	RasterizerState RasterizerState,
	Effect? Effect,
	Matrix Matrix
);

/// <summary> Utility that begins a SpriteBatch, ending it when disposed. Use this via the using keyword. </summary>
internal readonly ref struct SpriteBatchScope
{
	private readonly SpriteBatch spriteBatch;

	public SpriteBatchScope(SpriteBatch sb, in SpriteBatchArgs args)
	{
		spriteBatch = sb;
		sb.Begin(args);
	}
	public readonly void Dispose()
	{
		spriteBatch.End();
	}
}
/// <summary> Like <see cref="SpriteBatchScope"/>, but able to restore prior SpriteBatch parameters. </summary>
internal readonly ref struct SpriteBatchOverride
{
	private readonly SpriteBatch spriteBatch;
	private readonly SpriteBatchArgs? argsToRestore;

	public SpriteBatchOverride(SpriteBatch sb, in SpriteBatchArgs args)
	{
		spriteBatch = sb;

		if (sb.IsActive())
		{
			argsToRestore = sb.GetArguments();
			sb.End();
		}

		sb.Begin(args);
	}

	public readonly void Dispose()
	{
		spriteBatch.End();

		if (argsToRestore.HasValue)
		{
			spriteBatch.Begin(argsToRestore.Value);
		}
	}
}

internal static class SpriteBatchUtils
{
	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "beginCalled")]
	public extern static ref readonly bool IsActive(this SpriteBatch sb);
	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "sortMode")]
	public extern static ref readonly SpriteSortMode GetSortMode(this SpriteBatch sb);
	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "blendState")]
	public extern static ref readonly BlendState GetBlendState(this SpriteBatch sb);
	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "samplerState")]
	public extern static ref readonly SamplerState GetSamplerState(this SpriteBatch sb);
	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "depthStencilState")]
	public extern static ref readonly DepthStencilState GetDepthStencilState(this SpriteBatch sb);
	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "rasterizerState")]
	public extern static ref readonly RasterizerState GetRasterizerState(this SpriteBatch sb);
	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "customEffect")]
	public extern static ref readonly Effect? GetEffect(this SpriteBatch sb);
	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "transformMatrix")]
	public extern static ref readonly Matrix GetMatrix(this SpriteBatch sb);

	public static void Begin(this SpriteBatch sb, in SpriteBatchArgs args)
	{
		sb.Begin(args.SortMode, args.BlendState, args.SamplerState, args.DepthStencilState, args.RasterizerState, args.Effect, args.Matrix);
	}
	
	public static SpriteBatchScope Scope(this SpriteBatch sb, in SpriteBatchArgs args)
	{
		return new(sb, in args);
	}
	
	public static SpriteBatchOverride Override(this SpriteBatch sb, in SpriteBatchArgs args)
	{
		return new(sb, in args);
	}

	public static SpriteBatchArgs GetArguments(this SpriteBatch sb)
	{
		return new()
		{
			SortMode = sb.GetSortMode(),
			BlendState = sb.GetBlendState(),
			SamplerState = sb.GetSamplerState(),
			DepthStencilState = sb.GetDepthStencilState(),
			RasterizerState = sb.GetRasterizerState(),
			Effect = sb.GetEffect(),
			Matrix = sb.GetMatrix()
		};
	}
}
