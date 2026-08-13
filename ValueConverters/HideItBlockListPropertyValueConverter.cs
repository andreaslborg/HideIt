// Copyright (c) Hide It Package.
// MIT Licensed.

using HideIt.Filters;
using JetBrains.Annotations;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace HideIt.ValueConverters;

/// <summary>
/// Extends the built-in BlockListPropertyValueConverter to automatically filter out hidden blocks.
/// </summary>
[UsedImplicitly]
public class HideItBlockListPropertyValueConverter : BlockListPropertyValueConverter {
  private readonly IReadOnlyList<IHideItBlockListFilter> _filters;

  /// <inheritdoc />
  public HideItBlockListPropertyValueConverter(
      IProfilingLogger proflog,
      BlockEditorConverter blockConverter,
      IContentTypeService contentTypeService,
      IApiElementBuilder apiElementBuilder,
      IJsonSerializer jsonSerializer,
      BlockListPropertyValueConstructorCache constructorCache,
      IVariationContextAccessor variationContextAccessor,
      BlockEditorVarianceHandler blockEditorVarianceHandler,
      ILanguageService languageService,
      IPropertyRenderingContextAccessor propertyRenderingContextAccessor,
      IEnumerable<IHideItBlockListFilter> filters )
      : base( proflog, blockConverter, contentTypeService, apiElementBuilder, jsonSerializer, constructorCache, variationContextAccessor, blockEditorVarianceHandler, languageService, propertyRenderingContextAccessor ) {
    _filters = filters.ToArray();
  }

  /// <inheritdoc />
  public override object? ConvertIntermediateToObject( IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview ) {
    object? result = base.ConvertIntermediateToObject( owner, propertyType, referenceCacheLevel, inter, preview );

    return result switch {
      // Filter hidden blocks from BlockListModel and apply registered extensions
      BlockListModel blockList => ApplyFilters( blockList ),
      // Filter hidden single block items
      BlockListItem blockItem when blockItem.IsBlockHidden() => null,
      _ => result
    };
  }

  private BlockListModel ApplyFilters( BlockListModel model ) {
    BlockListModel currentModel = model;
    foreach ( IHideItBlockListFilter filter in _filters ) {
      currentModel = filter.Filter( currentModel ) ?? throw new InvalidOperationException( $"HideIt block list filter '{filter.GetType().FullName}' returned null." );
    }
    return currentModel;
  }
}