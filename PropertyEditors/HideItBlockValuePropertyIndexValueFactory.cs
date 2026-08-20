// Copyright (c) Hide It Package.
// MIT Licensed.

using System.Text;
using System.Text.Json;
using HideIt.PropertyEditors.Models;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Extensions;

namespace HideIt.PropertyEditors;

/// <summary>
/// Index value factory for block editors that removes hidden block content before it is added to search indexes.
/// </summary>
internal sealed class HideItBlockValuePropertyIndexValueFactory : JsonPropertyIndexValueFactoryBase<HideItIndexValueFactoryBlockValue>, IBlockValuePropertyIndexValueFactory {
  private readonly string _hidePropertyAlias;
  private readonly PropertyEditorCollection _propertyEditorCollection;

  public HideItBlockValuePropertyIndexValueFactory(
    PropertyEditorCollection propertyEditorCollection,
    IJsonSerializer jsonSerializer,
    IOptionsMonitor<IndexingSettings> indexingSettings,
    IOptions<HideItSettings> hideItSettings )
    : base( jsonSerializer, indexingSettings ) {
    _propertyEditorCollection = propertyEditorCollection;
    _hidePropertyAlias = HideItSettings.NormalizePropertyAlias( hideItSettings.Value.PropertyAlias );
  }

  protected override IEnumerable<IndexValue> Handle(
    HideItIndexValueFactoryBlockValue blockValue,
    IProperty property,
    string? culture,
    string? segment,
    bool published,
    IEnumerable<string> availableCultures,
    IDictionary<Guid, IContentType> contentTypeDictionary ) {
    string[] availableCultureArray = availableCultures as string[] ?? availableCultures.ToArray();
    List<IndexValue> result = [];
    HideItIndexValueFactoryBlockValue visibleBlockValue = FilterHiddenBlocks( blockValue );

    int index = 0;
    foreach ( RawDataItem rawData in GetDataItems( visibleBlockValue.ContentData, visibleBlockValue.Expose, published ) ) {
      if ( !contentTypeDictionary.TryGetValue( rawData.ContentTypeKey, out IContentType? contentType ) ) {
        continue;
      }

      Dictionary<string, IPropertyType> propertyTypeDictionary = contentType
        .CompositionPropertyTypes
        .Select( propertyType => {
          if ( culture is not null ) {
            propertyType.Variations |= ContentVariation.Culture;
          }

          if ( segment is not null ) {
            propertyType.Variations |= ContentVariation.Segment;
          }

          return propertyType;
        } )
        .ToDictionary( propertyType => propertyType.Alias );

      result.AddRange( GetNestedResults(
        $"{property.Alias}.items[{index}]",
        culture,
        segment,
        published,
        propertyTypeDictionary,
        rawData,
        availableCultureArray,
        contentTypeDictionary ) );

      index++;
    }

    return RenameKeysToEnsureRawSegmentsIsAPrefix( result );
  }

  protected override IEnumerable<IndexValue> HandleResume(
    List<IndexValue> indexedContent,
    IProperty property,
    string? culture,
    string? segment,
    bool published ) {
    string?[] indexedCultures = indexedContent
      .DistinctBy( value => value.Culture )
      .Select( value => value.Culture )
      .WhereNotNull()
      .ToArray();

    IEnumerable<string?> cultures = indexedCultures.Length > 0 ? indexedCultures : [culture];

    return cultures.Select( currentCulture => new IndexValue {
      Culture = currentCulture,
      FieldName = property.Alias,
      Values = [GetResumeFromAllContent( indexedContent, currentCulture )]
    } );
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

  private static IEnumerable<RawDataItem> GetDataItems( IList<BlockItemData> contentData, IList<BlockItemVariation> expose, bool published ) {
    if ( !published ) {
      return contentData.Select( ToRawData );
    }

    List<RawDataItem> indexData = [];
    foreach ( BlockItemData blockItemData in contentData ) {
      string?[] exposedCultures = expose
        .Where( variation => variation.ContentKey == blockItemData.Key )
        .Select( variation => variation.Culture )
        .ToArray();

      if ( exposedCultures.Length == 0 ) {
        continue;
      }

      string?[] blockItemCultures = blockItemData.Values.Select( value => value.Culture ).ToArray();
      if ( exposedCultures.Contains( null ) || exposedCultures.ContainsAll( blockItemCultures ) ) {
        indexData.Add( ToRawData( blockItemData ) );
        continue;
      }

      indexData.Add( ToRawData(
        blockItemData.ContentTypeKey,
        blockItemData.Values.Where( value => value.Culture is null || exposedCultures.Contains( value.Culture ) ) ) );
    }

    return indexData;
  }

  private static List<IndexValue> RenameKeysToEnsureRawSegmentsIsAPrefix( List<IndexValue> indexContent ) {
    foreach ( IndexValue indexValue in indexContent ) {
      if ( indexValue.FieldName.Substring( 1 ).Contains( UmbracoExamineFieldNames.RawFieldPrefix, StringComparison.Ordinal ) ) {
        indexValue.FieldName = UmbracoExamineFieldNames.RawFieldPrefix + indexValue.FieldName.Replace( UmbracoExamineFieldNames.RawFieldPrefix, string.Empty, StringComparison.Ordinal );
      }
    }

    return indexContent;
  }

  private static string GetResumeFromAllContent( List<IndexValue> indexedContent, string? culture ) {
    StringBuilder stringBuilder = new();
    foreach ( IndexValue indexValue in indexedContent.Where( value => value.Culture == culture || value.Culture is null ) ) {
      if ( indexValue.FieldName.Contains( UmbracoExamineFieldNames.RawFieldPrefix, StringComparison.Ordinal ) ) {
        continue;
      }

      foreach ( object? value in indexValue.Values ) {
        if ( value is not null ) {
          stringBuilder.AppendLine( value.ToString() );
        }
      }
    }

    return stringBuilder.ToString();
  }

  private IEnumerable<IndexValue> GetNestedResults(
    string keyPrefix,
    string? culture,
    string? segment,
    bool published,
    Dictionary<string, IPropertyType> propertyTypeDictionary,
    RawDataItem rawData,
    IReadOnlyList<string> availableCultures,
    IDictionary<Guid, IContentType> contentTypeDictionary ) {
    List<RawPropertyData> rawProperties = rawData.Properties.ToList();
    string?[] rawDataCultures = rawProperties.Select( property => property.Culture ).Distinct().WhereNotNull().ToArray();
    bool hasRawDataCultures = rawDataCultures.Length > 0;

    foreach ( RawPropertyData rawPropertyData in rawProperties ) {
      if ( !propertyTypeDictionary.TryGetValue( rawPropertyData.Alias, out IPropertyType? propertyType ) ) {
        continue;
      }

      IDataEditor? editor = _propertyEditorCollection[propertyType.PropertyEditorAlias];
      if ( editor is null ) {
        continue;
      }

      string? propertyCulture = rawPropertyData.Culture ?? culture;
      if ( !propertyType.VariesByCulture() && propertyCulture is not null ) {
        continue;
      }

      Property subProperty = new( propertyType );
      IEnumerable<IndexValue> indexValues = [];

      if ( propertyType.VariesByCulture() && propertyCulture is null ) {
        foreach ( string availableCulture in availableCultures ) {
          subProperty.SetValue( rawPropertyData.Value, availableCulture, segment );
          if ( published ) {
            subProperty.PublishValues( availableCulture, segment ?? "*" );
          }

          indexValues = editor.PropertyIndexValueFactory.GetIndexValues( subProperty, availableCulture, segment, published, availableCultures, contentTypeDictionary );
        }
      } else {
        subProperty.SetValue( rawPropertyData.Value, propertyCulture, segment );
        if ( published ) {
          subProperty.PublishValues( propertyCulture ?? "*", segment ?? "*" );
        }

        indexValues = editor.PropertyIndexValueFactory.GetIndexValues( subProperty, propertyCulture, segment, published, availableCultures, contentTypeDictionary );
      }

      foreach ( IndexValue indexValue in indexValues ) {
        indexValue.FieldName = $"{keyPrefix}.{indexValue.FieldName}";

        if ( indexValue.Culture is null && hasRawDataCultures ) {
          foreach ( string? rawDataCulture in rawDataCultures ) {
            yield return new IndexValue {
              Culture = rawDataCulture,
              FieldName = indexValue.FieldName,
              Values = indexValue.Values
            };
          }
        } else {
          indexValue.Culture = hasRawDataCultures ? indexValue.Culture : null;
          yield return indexValue;
        }
      }
    }
  }

  private static RawDataItem ToRawData( BlockItemData blockItemData ) => ToRawData( blockItemData.ContentTypeKey, blockItemData.Values );

  private static RawDataItem ToRawData( Guid contentTypeKey, IEnumerable<BlockPropertyValue> values ) {
    return new RawDataItem {
      ContentTypeKey = contentTypeKey,
      Properties = values.Select( value => new RawPropertyData {
        Alias = value.Alias,
        Culture = value.Culture,
        Value = value.Value
      } )
    };
  }

  private sealed class RawDataItem {
    public required Guid ContentTypeKey { get; init; }
    public required IEnumerable<RawPropertyData> Properties { get; init; }
  }

  private sealed class RawPropertyData {
    public required string Alias { get; init; }
    public required object? Value { get; init; }
    public required string? Culture { get; init; }
  }
}