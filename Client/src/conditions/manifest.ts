import HideItHasPropertyCondition from './has-hide-it-property.condition.js';

export const manifests: Array<UmbExtensionManifest> = [
  {
    type: 'condition',
    name: 'Hide It Has Property Condition',
    alias: 'HideIt.Condition.HasHideItProperty',
    api: HideItHasPropertyCondition,
  },
];
