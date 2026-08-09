// Copyright (c) Hide It Package.
// MIT Licensed.

using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.PropertyEditors;

namespace HideIt.ValueConverters;

/// <summary>
/// Extends the built-in BlockListPropertyValueConverter to automatically filter out hidden blocks.
/// </summary>
public class HideItBlockListPropertyValueConverter : BlockListPropertyValueConverter
{
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
        IPropertyRenderingContextAccessor propertyRenderingContextAccessor)
        : base(proflog, blockConverter, contentTypeService, apiElementBuilder, jsonSerializer, constructorCache, variationContextAccessor, blockEditorVarianceHandler, languageService, propertyRenderingContextAccessor)
    {
    }

    /// <inheritdoc />
    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        var result = base.ConvertIntermediateToObject(owner, propertyType, referenceCacheLevel, inter, preview);

        // Filter hidden blocks from BlockListModel
        if (result is BlockListModel blockList)
        {
            return blockList.WhereVisible();
        }

        // Filter hidden single block items
        if (result is BlockListItem blockItem && blockItem.IsBlockHidden())
        {
            return null;
        }

        return result;
    }
}
