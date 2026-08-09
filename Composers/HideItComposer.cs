// Copyright (c) Hide It Package.
// MIT Licensed.

using HideIt.ValueConverters;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace HideIt.Composers;

/// <summary>
/// Composer that registers Hide It services.
/// Replaces the built-in block property value converters to automatically filter hidden blocks.
/// </summary>
public class HideItComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // Replace the built-in converters with our HideIt-aware versions
        // This makes hidden block filtering "magic" - no code changes needed in views
        builder.PropertyValueConverters()
            .Replace<BlockListPropertyValueConverter, HideItBlockListPropertyValueConverter>()
            .Replace<BlockGridPropertyValueConverter, HideItBlockGridPropertyValueConverter>();
    }
}
