import { manifests as conditionManifests } from "./conditions/manifest.js";
import { manifests as actionManifests } from "./actions/manifest.js";

export const manifests: Array<UmbExtensionManifest> = [
  ...conditionManifests,
  ...actionManifests,
];
