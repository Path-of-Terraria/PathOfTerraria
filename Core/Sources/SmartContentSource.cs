using ReLogic.Content;
using ReLogic.Content.Sources;
using System.Collections.Generic;
using System.IO;

namespace PathOfTerraria.Core.Sources;

/// <summary>
///		A wrapper around an <see cref="IContentSource"/> instance which additional
///		APIs for finer control.
/// </summary>
internal sealed class SmartContentSource(IContentSource source) : IContentSource
{
	IContentValidator IContentSource.ContentValidator
	{
		get => source.ContentValidator;
		set => source.ContentValidator = value;
	}

	RejectedAssetCollection IContentSource.Rejections => source.Rejections;

	/// <summary>
	///		A map of directory redirects, with keys as the source directories
	///		and values as the target directories.
	/// </summary>
	private readonly Dictionary<string, string> directoryRedirects = [];

	/// <summary>
	///		Adds a directory redirect from <paramref name="fromDir"/> to
	///		<paramref name="toDir"/>.
	/// </summary>
	/// <param name="fromDir">The directory to redirect from.</param>
	/// <param name="toDir">The directory to redirect to.</param>
	public void AddDirectoryRedirect(string fromDir, string toDir)
	{
		directoryRedirects.Add(fromDir, toDir);
	}

	/// <summary>
	///		Rewrites a path to account for modifications made by this content
	///		source, such as directory redirections.
	/// </summary>
	/// <param name="path">The path to rewrite.</param>
	/// <returns>The rewritten path.</returns>
	private string RewritePath(string path)
	{
		// In the future, we may also use a hash map to reduce duplicate
		// calculations if a notable performance degradation is observed, as
		// this is used in frequently-called codepaths.

		foreach ((string from, string to) in directoryRedirects)
		{
			if (path.StartsWith(from))
			{
				// Since we only rewrite directories now, we can return early
				// here.
				return path.Replace(from, to);
			}
		}

		return path;
	}

	public IEnumerable<string> EnumerateAssets()
	{
		throw new NotImplementedException();
	}

	public string GetExtension(string assetName)
	{
		throw new NotImplementedException();
	}

	public Stream OpenStream(string fullAssetName)
	{
		throw new NotImplementedException();
	}
}
