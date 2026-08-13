// Copyright (c) Hide It Package.
// MIT Licensed.

using Umbraco.Cms.Core.Models.Blocks;

namespace HideIt.Filters;

/// <summary>
/// Extensibility point for post-processing Block List values after Hide It's built-in visibility filtering.
/// </summary>
public interface IHideItBlockListFilter {
  /// <summary>
  /// Applies additional filtering or transformations to a Block List model.
  /// </summary>
  /// <param name="model">The current Block List model.</param>
  /// <returns>The filtered Block List model.</returns>
  BlockListModel Filter( BlockListModel model );
}

/// <summary>
/// Extensibility point for post-processing Block Grid values after Hide It's built-in visibility filtering.
/// </summary>
public interface IHideItBlockGridFilter {
  /// <summary>
  /// Applies additional filtering or transformations to a Block Grid model.
  /// </summary>
  /// <param name="model">The current Block Grid model.</param>
  /// <returns>The filtered Block Grid model.</returns>
  BlockGridModel Filter( BlockGridModel model );
}

internal sealed class HideItVisibilityBlockListFilter : IHideItBlockListFilter {
  public BlockListModel Filter( BlockListModel model ) => model.WhereVisible();
}

internal sealed class HideItVisibilityBlockGridFilter : IHideItBlockGridFilter {
  public BlockGridModel Filter( BlockGridModel model ) => model.WhereVisible();
}
