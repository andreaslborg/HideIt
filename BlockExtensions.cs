// Copyright (c) Hide It Package.
// MIT Licensed.

using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace HideIt;

/// <summary>
/// Extension methods for filtering hidden blocks from block collections.
/// </summary>
public static class BlockExtensions {
  /// <summary>
  /// The property alias used to determine if a block should be hidden.
  /// Defaults to "hideIt" and can be customized via the "HideIt:PropertyAlias" app setting.
  /// </summary>
  private static string _hideItPropertyAlias = HideItSettings.DefaultPropertyAlias;

  /// <summary>
  /// Sets the property alias used to determine if a block should be hidden.
  /// Called at composition time when a custom alias is configured.
  /// </summary>
  /// <param name="alias">The custom property alias.</param>
  internal static void SetPropertyAlias( string alias ) {
    if ( string.IsNullOrWhiteSpace( alias ) ) {
      return;
    }
    _hideItPropertyAlias = alias;
  }

  /// <summary>
  /// Filters out blocks that have hideIt set to true in their settings.
  /// </summary>
  /// <param name="blocks">The block list model to filter.</param>
  /// <returns>A new BlockListModel containing only visible blocks.</returns>
  public static BlockListModel WhereVisible( this BlockListModel? blocks ) {
    if ( blocks == null || blocks.Count == 0 ) {
      return BlockListModel.Empty;
    }

    List<BlockListItem>? visibleBlocks = null;
    for ( int index = 0; index < blocks.Count; index++ ) {
      BlockListItem block = blocks[ index ];
      if ( !IsHidden( block.Settings ) ) {
        visibleBlocks?.Add( block );
        continue;
      }

      if ( visibleBlocks is not null ) {
        continue;
      }

      visibleBlocks = new List<BlockListItem>( blocks.Count - 1 );
      for ( int visibleIndex = 0; visibleIndex < index; visibleIndex++ ) {
        visibleBlocks.Add( blocks[ visibleIndex ] );
      }
    }

    return visibleBlocks == null ? blocks : new BlockListModel( visibleBlocks );
  }

  /// <summary>
  /// Filters out blocks that have hideIt set to true in their settings, including nested blocks in areas.
  /// </summary>
  /// <param name="blocks">The block grid model to filter.</param>
  /// <returns>A new BlockGridModel containing only visible blocks.</returns>
  public static BlockGridModel WhereVisible( this BlockGridModel? blocks ) {
    if ( blocks == null || blocks.Count == 0 ) {
      return BlockGridModel.Empty;
    }

    List<BlockGridItem>? visibleBlocks = null;
    for ( int index = 0; index < blocks.Count; index++ ) {
      BlockGridItem block = blocks[ index ];
      if ( !IsHidden( block.Settings ) ) {
        visibleBlocks?.Add( FilterBlockGridItemAreas( block ) );
        continue;
      }

      if ( visibleBlocks is not null ) {
        continue;
      }

      visibleBlocks = new List<BlockGridItem>( blocks.Count - 1 );
      for ( int visibleIndex = 0; visibleIndex < index; visibleIndex++ ) {
        visibleBlocks.Add( FilterBlockGridItemAreas( blocks[ visibleIndex ] ) );
      }
    }

    return visibleBlocks == null ? blocks : new BlockGridModel( visibleBlocks, blocks.GridColumns );
  }

  /// <summary>
  /// Checks if a block item is hidden based on its settings.
  /// </summary>
  /// <param name="block">The block item to check.</param>
  /// <returns>True if the block is hidden, false otherwise.</returns>
  public static bool IsBlockHidden<TContent, TSettings>( this IBlockReference<TContent, TSettings> block )
      where TContent : IPublishedElement
      where TSettings : IPublishedElement {
    return IsHidden( block.Settings );
  }

  private static bool IsHidden( IPublishedElement? settings ) {
    IPublishedProperty? hideItProperty = settings?.GetProperty( _hideItPropertyAlias );
    if ( hideItProperty == null ) {
      return false;
    }

    object? value = hideItProperty.GetValue();
    return value switch {
      bool boolValue => boolValue,
      int intValue => intValue == 1,
      string stringValue => stringValue.Equals( "true", StringComparison.OrdinalIgnoreCase ) || stringValue == "1",
      _ => false
    };
  }

  private static BlockGridItem FilterBlockGridItemAreas( BlockGridItem item ) {
    // Filter areas recursively
    List<BlockGridArea> filteredAreas = item.Areas
        .Select( area => new BlockGridArea(
            area.Where( areaItem => !IsHidden( areaItem.Settings ) )
                .Select( FilterBlockGridItemAreas )
                .ToList(),
            area.Alias,
            area.RowSpan,
            area.ColumnSpan ) )
        .ToList();

    // Create a new item with filtered areas
    BlockGridItem newItem = new(
        item.ContentKey,
        item.Content,
        item.SettingsKey,
        item.Settings ) {
      RowSpan = item.RowSpan,
      ColumnSpan = item.ColumnSpan,
      AreaGridColumns = item.AreaGridColumns,
      GridColumns = item.GridColumns,
      Areas = filteredAreas
    };

    return newItem;
  }
}
