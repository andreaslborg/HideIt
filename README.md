# Hide It - Umbraco Block Visibility Toggle

A simple Umbraco 17+ package that allows content editors to toggle block visibility directly from the block action bar.

## Features

- **Toggle Button**: Shows in the block action bar (first position) with `icon-ban`
- **Visual Feedback**: Hidden blocks show reduced opacity in the backoffice
- **Extension Methods**: Filter hidden blocks with `.WhereVisible()` in views
- **Nested Blocks**: BlockGrid areas are filtered recursively

## Installation

```bash
dotnet add package Our.Umbraco.HideIt
```

## Usage

1. Install the package
2. Add a **True/False** property with alias `hideIt` to your block's **Settings** element type
3. The toggle button will automatically appear on blocks with this property
4. Use `.WhereVisible()` in your views to filter hidden blocks

## How It Works

### Backoffice
- A block action appears when a block's settings element type has a `hideIt` property
- Clicking toggles the value and applies visual feedback (reduced opacity)
- The action bar remains fully visible for easy toggling

### Frontend (Views)

Use the `.WhereVisible()` extension method to filter hidden blocks:

```razor
@using HideIt

@* Block List *@
@foreach (var block in Model.ContentRows.WhereVisible())
{
    <partial name="@block.Content.ContentType.Alias" model="block" />
}

@* Block Grid *@
@await Html.GetBlockGridHtmlAsync(Model.ContentGrid.WhereVisible())
```

## Extension Methods

```csharp
using HideIt;

// Filter block list
var visibleBlocks = Model.Blocks.WhereVisible();

// Filter block grid (including nested areas)
var visibleGrid = Model.Grid.WhereVisible();

// Check individual blocks
if (!block.IsBlockHidden())
{
    // Render block
}
```

## Requirements

- Umbraco 17.0.0 or higher
- .NET 10.0

## License

MIT
