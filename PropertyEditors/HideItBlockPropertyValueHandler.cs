// Copyright (c) Hide It Package.
// MIT Licensed.

using System.Text.Json;
using HideIt.PropertyEditors.Models;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Core.Search.Indexing.Collection;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace HideIt.PropertyEditors;

/// <summary>
/// Property value handler for block editors (block list, block grid and single block) that removes hidden block
/// content before delegating to the built-in handler, so hidden blocks never reach search indexes.
/// </summary>
/// <remarks>
/// This intentionally does not implement <see cref="ICorePropertyValueHandler"/>, so it is prioritized over the
/// built-in block handlers by <see cref="PropertyValueHandlerCollectionExtensions.GetPropertyValueHandler"/>.
/// </remarks>
[UsedImplicitly]
internal sealed class HideItBlockPropertyValueHandler : IPropertyValueHandler {
  private static readonly string[] _blockEditorAliases = ["Umbraco.BlockList", "Umbraco.BlockGrid", "Umbraco.SingleBlock"];

  private readonly PropertyValueHandlerCollection _propertyValueHandlerCollection;
  private readonly IJsonSerializer _jsonSerializer;
  private readonly string _hidePropertyAlias;

  public HideItBlockPropertyValueHandler(
    PropertyValueHandlerCollection propertyValueHandlerCollection,
    IJsonSerializer jsonSerializer,
    IOptions<HideItSettings> hideItSettings ) {
    _propertyValueHandlerCollection = propertyValueHandlerCollection;
    _jsonSerializer = jsonSerializer;
    _hidePropertyAlias = HideItSettings.NormalizePropertyAlias( hideItSettings.Value.PropertyAlias );
  }

  public bool CanHandle( IPropertyType propertyType ) => _blockEditorAliases.Contains( propertyType.PropertyEditorAlias );

  public IEnumerable<IndexField> GetIndexFields(
    IProperty property,
    string? culture,
    string? segment,
    bool published,
    IContentBase contentContext ) {
    IPropertyValueHandler? builtInHandler = _propertyValueHandlerCollection
      .FirstOrDefault( handler => handler is ICorePropertyValueHandler && handler.CanHandle( property.PropertyType ) );

    if ( builtInHandler is null ) {
      return [];
    }

    if ( property.GetValue( culture, segment, published ) is not string rawValue || !rawValue.DetectIsJson() ) {
      return builtInHandler.GetIndexFields( property, culture, segment, published, contentContext );
    }

    HideItIndexValueFactoryBlockValue? blockValue = _jsonSerializer.Deserialize<HideItIndexValueFactoryBlockValue>( rawValue );
    if ( blockValue is null ) {
      return builtInHandler.GetIndexFields( property, culture, segment, published, contentContext );
    }

    HideItIndexValueFactoryBlockValue filteredBlockValue = FilterHiddenBlocks( blockValue );
    if ( ReferenceEquals( filteredBlockValue, blockValue ) ) {
      return builtInHandler.GetIndexFields( property, culture, segment, published, contentContext );
    }

    Property filteredProperty = new( property.PropertyType );
    filteredProperty.SetValue( _jsonSerializer.Serialize( filteredBlockValue ), culture, segment );
    if ( published ) {
      filteredProperty.PublishValues( culture ?? "*", segment ?? "*" );
    }

    return builtInHandler.GetIndexFields( filteredProperty, culture, segment, published, contentContext );
  }

  private HideItIndexValueFactoryBlockValue FilterHiddenBlocks( HideItIndexValueFactoryBlockValue blockValue ) {
    if ( blockValue.ContentData.Count == 0 || blockValue.SettingsData.Count == 0 || blockValue.Layout.Count == 0 ) {
      return blockValue;
    }

    HashSet<Guid> hiddenContentKeys = GetHiddenContentKeys( blockValue );
    if ( hiddenContentKeys.Count == 0 ) {
      return blockValue;
    }

    return new HideItIndexValueFactoryBlockValue {
      Layout = blockValue.Layout,
      SettingsData = blockValue.SettingsData,
      ContentData = blockValue.ContentData.Where( content => !hiddenContentKeys.Contains( content.Key ) ).ToList(),
      Expose = blockValue.Expose.Where( expose => !hiddenContentKeys.Contains( expose.ContentKey ) ).ToList()
    };
  }

  private HashSet<Guid> GetHiddenContentKeys( HideItIndexValueFactoryBlockValue blockValue ) {
    Dictionary<Guid, BlockItemData> settingsByKey = blockValue.SettingsData.ToDictionary( setting => setting.Key );
    HashSet<Guid> hiddenContentKeys = [];

    foreach ( List<HideItLayoutItem> layoutItems in blockValue.Layout.Values ) {
      foreach ( HideItLayoutItem layoutItem in layoutItems ) {
        CollectHiddenContentKeys( layoutItem, settingsByKey, hiddenContentKeys );
      }
    }

    return hiddenContentKeys;
  }

  private void CollectHiddenContentKeys(
    HideItLayoutItem layoutItem,
    IReadOnlyDictionary<Guid, BlockItemData> settingsByKey,
    ISet<Guid> hiddenContentKeys ) {
    Guid? contentKey = layoutItem.ContentKey ?? ParseGuidFromUdi( layoutItem.ContentUdi );
    Guid? settingsKey = layoutItem.SettingsKey ?? ParseGuidFromUdi( layoutItem.SettingsUdi );

    if ( contentKey.HasValue && settingsKey.HasValue && settingsByKey.TryGetValue( settingsKey.Value, out BlockItemData? settingsData ) && IsHiddenSettings( settingsData ) ) {
      hiddenContentKeys.Add( contentKey.Value );
    }

    foreach ( HideItLayoutArea area in layoutItem.Areas ) {
      foreach ( HideItLayoutItem areaItem in area.Items ) {
        CollectHiddenContentKeys( areaItem, settingsByKey, hiddenContentKeys );
      }
    }
  }

  private bool IsHiddenSettings( BlockItemData settingsData ) {
    BlockPropertyValue? hideValue = settingsData.Values.FirstOrDefault( propertyValue => propertyValue.Alias.Equals( _hidePropertyAlias, StringComparison.OrdinalIgnoreCase ) );
    return hideValue is not null && IsHiddenValue( hideValue.Value );
  }

  private static bool IsHiddenValue( object? value ) {
    return value switch {
      bool boolValue => boolValue,
      byte byteValue => byteValue == 1,
      short shortValue => shortValue == 1,
      int intValue => intValue == 1,
      long longValue => longValue == 1,
      string stringValue => IsTrueString( stringValue ),
      JsonElement jsonElement => IsHiddenJsonElement( jsonElement ),
      _ => IsTrueString( value?.ToString() )
    };
  }

  private static bool IsHiddenJsonElement( JsonElement jsonElement ) {
    return jsonElement.ValueKind switch {
      JsonValueKind.True => true,
      JsonValueKind.False => false,
      JsonValueKind.Number when jsonElement.TryGetInt64( out long intValue ) => intValue == 1,
      JsonValueKind.String => IsTrueString( jsonElement.GetString() ),
      _ => false
    };
  }

  private static bool IsTrueString( string? value ) {
    if ( string.IsNullOrWhiteSpace( value ) ) {
      return false;
    }

    string normalizedValue = value.Trim();
    return normalizedValue.Equals( "true", StringComparison.OrdinalIgnoreCase ) || normalizedValue == "1";
  }

  private static Guid? ParseGuidFromUdi( string? udi ) {
    if ( string.IsNullOrWhiteSpace( udi ) ) {
      return null;
    }

    string value = udi.Trim();
    int separatorIndex = value.LastIndexOf( '/' );
    if ( separatorIndex >= 0 && separatorIndex + 1 < value.Length ) {
      value = value[( separatorIndex + 1 )..];
    }

    if ( Guid.TryParse( value, out Guid guid ) || Guid.TryParseExact( value, "N", out guid ) ) {
      return guid;
    }

    return null;
  }
}
