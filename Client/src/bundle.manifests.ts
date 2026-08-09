import { manifests as conditionManifests } from "./conditions/manifest.js";
import { manifests as actionManifests } from "./actions/manifest.js";

// Inject global styles for visual feedback
const styles = `
/* Hide It - Visual feedback for hidden blocks */
umb-block-list-entry[data-hideit-hidden],
umb-block-grid-entry[data-hideit-hidden],
umb-block-rte-entry[data-hideit-hidden],
[data-hideit-hidden] {
  opacity: 0.4;
}

umb-block-list-entry[data-hideit-hidden] uui-action-bar,
umb-block-grid-entry[data-hideit-hidden] uui-action-bar,
umb-block-rte-entry[data-hideit-hidden] uui-action-bar,
[data-hideit-hidden] uui-action-bar {
  opacity: 1;
}
`;

// Inject styles into the document
const styleSheet = document.createElement('style');
styleSheet.textContent = styles;
document.head.appendChild(styleSheet);

export const manifests: Array<UmbExtensionManifest> = [
  ...conditionManifests,
  ...actionManifests,
];
