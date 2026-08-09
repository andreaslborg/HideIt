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
/// Extends the built-in BlockGridPropertyValueConverter to automatically filter out hidden blocks.
/// </summary>
public class HideItBlockGridPropertyValueConverter : BlockGridPropertyValueConverter
{
    /// <inheritdoc />
    public HideItBlockGridPropertyValueConverter(
        IProfilingLogger proflog,
        BlockEditorConverter blockConverter,
        IJsonSerializer jsonSerializer,
        IApiElementBuilder apiElementBuilder,
        BlockGridPropertyValueConstructorCache constructorCache,
        IVariationContextAccessor variationContextAccessor,
        BlockEditorVarianceHandler blockEditorVarianceHandler,
        ILanguageService languageService,
        IPropertyRenderingContextAccessor propertyRenderingContextAccessor)
        : base(proflog, blockConverter, jsonSerializer, apiElementBuilder, constructorCache, variationContextAccessor, blockEditorVarianceHandler, languageService, propertyRenderingContextAccessor)
    {
    }

    /// <inheritdoc />
    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        var result = base.ConvertIntermediateToObject(owner, propertyType, referenceCacheLevel, inter, preview);

        // Filter hidden blocks from BlockGridModel
        if (result is BlockGridModel blockGrid)
        {
            return blockGrid.WhereVisible();
        }

        return result;
    }
}
