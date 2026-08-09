# Hide It - Umbraco Block Visibility Toggle

A simple Umbraco 17+ package that allows content editors to toggle block visibility directly from the block action bar.

## Features

- **Toggle Button**: Eye icon in the block action bar to hide/show blocks
- **Visual Feedback**: Hidden blocks show reduced opacity in the backoffice
- **Automatic Filtering**: Hidden blocks are automatically excluded from frontend rendering
- **Nested Blocks**: BlockGrid areas are filtered recursively

## Installation

```bash
dotnet add package HideIt
```

Or add a project reference during development.

## Usage

1. Install the package
2. Add a **True/False** property with alias `hideIt` to your block's **Settings** element type
3. The toggle button will automatically appear on blocks with this property
4. **That's it!** Hidden blocks are automatically filtered on the frontend

## How It Works

### Backoffice
- A toggle icon appears when a block's settings element type has a `hideIt` property
- Eye icon = visible, Eye-off icon = hidden
- Clicking toggles the value and dims the block content

### Frontend
Hidden blocks are **automatically filtered** - no code changes needed in your views!

The package replaces Umbraco's built-in block property value converters to filter out blocks where `hideIt` is true.

### Optional: Manual Filtering

If you need more control, extension methods are also available:

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

- Umbraco 17.5.0 or higher
- .NET 10.0

## License

MIT
