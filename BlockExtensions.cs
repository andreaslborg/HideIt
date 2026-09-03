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
      BlockListItem block = blocks[index];
      if ( !IsHidden( block.Settings ) ) {
        visibleBlocks?.Add( block );
        continue;
      }

      if ( visibleBlocks is not null ) {
        continue;
      }

      visibleBlocks = new List<BlockListItem>( blocks.Count - 1 );
      for ( int visibleIndex = 0; visibleIndex < index; visibleIndex++ ) {
        visibleBlocks.Add( blocks[visibleIndex] );
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
      BlockGridItem block = blocks[index];

      if ( IsHidden( block.Settings ) ) {
        if ( visibleBlocks is null ) {
          visibleBlocks = new List<BlockGridItem>( blocks.Count - 1 );
          for ( int previousIndex = 0; previousIndex < index; previousIndex++ ) {
            visibleBlocks.Add( blocks[previousIndex] );
          }
        }

        continue;
      }

      // Nested blocks in areas must always be checked for hidden items, regardless of
      // whether this top-level block is visible, so areaChanged can force allocation.
      BlockGridItem filteredBlock = FilterBlockGridItemAreas( block, out bool areaChanged );
      if ( areaChanged && visibleBlocks is null ) {
        visibleBlocks = new List<BlockGridItem>( blocks.Count );
        for ( int previousIndex = 0; previousIndex < index; previousIndex++ ) {
          visibleBlocks.Add( blocks[previousIndex] );
        }
      }

      visibleBlocks?.Add( filteredBlock );
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

  /// <summary>
  /// Recursively filters hidden blocks out of an item's nested areas.
  /// </summary>
  /// <param name="item">The block grid item whose areas should be filtered.</param>
  /// <param name="changed">True if any nested block was removed or altered; otherwise false.</param>
  /// <returns>The original item if nothing changed, or a new item with filtered areas.</returns>
  private static BlockGridItem FilterBlockGridItemAreas( BlockGridItem item, out bool changed ) {
    changed = false;

    // Areas is typed as IEnumerable<BlockGridArea>, but is backed by an indexable
    // collection (an empty array by default). Avoid materializing a new list unless
    // it genuinely isn't already indexable.
    IReadOnlyList<BlockGridArea> areas = item.Areas as IReadOnlyList<BlockGridArea> ?? item.Areas.ToList();
    if ( areas.Count == 0 ) {
      return item;
    }

    List<BlockGridArea>? filteredAreas = null;
    for ( int index = 0; index < areas.Count; index++ ) {
      BlockGridArea area = areas[index];
      BlockGridArea filteredArea = FilterBlockGridArea( area, out bool areaChanged );

      if ( areaChanged && filteredAreas is null ) {
        filteredAreas = new List<BlockGridArea>( areas.Count );
        for ( int previousIndex = 0; previousIndex < index; previousIndex++ ) {
          filteredAreas.Add( areas[previousIndex] );
        }
      }

      filteredAreas?.Add( filteredArea );
    }

    if ( filteredAreas is null ) {
      return item;
    }

    changed = true;
    return new BlockGridItem(
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
  }

  /// <summary>
  /// Filters hidden blocks out of a single area, recursing into each item's own nested areas.
  /// </summary>
  /// <param name="area">The area to filter.</param>
  /// <param name="changed">True if any item was removed or altered; otherwise false.</param>
  /// <returns>The original area if nothing changed, or a new area with filtered items.</returns>
  private static BlockGridArea FilterBlockGridArea( BlockGridArea area, out bool changed ) {
    changed = false;
    if ( area.Count == 0 ) {
      return area;
    }

    List<BlockGridItem>? filteredItems = null;
    for ( int index = 0; index < area.Count; index++ ) {
      BlockGridItem areaItem = area[index];

      if ( IsHidden( areaItem.Settings ) ) {
        if ( filteredItems is null ) {
          filteredItems = new List<BlockGridItem>( area.Count - 1 );
          for ( int previousIndex = 0; previousIndex < index; previousIndex++ ) {
            filteredItems.Add( area[previousIndex] );
          }
        }

        continue;
      }

      BlockGridItem filteredItem = FilterBlockGridItemAreas( areaItem, out bool itemChanged );
      if ( itemChanged && filteredItems is null ) {
        filteredItems = new List<BlockGridItem>( area.Count );
        for ( int previousIndex = 0; previousIndex < index; previousIndex++ ) {
          filteredItems.Add( area[previousIndex] );
        }
      }

      filteredItems?.Add( filteredItem );
    }

    if ( filteredItems is null ) {
      return area;
    }

    changed = true;
    return new BlockGridArea( filteredItems, area.Alias, area.RowSpan, area.ColumnSpan );
  }
}