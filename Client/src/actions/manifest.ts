export const manifests: Array<UmbExtensionManifest> = [
  {
    type: 'blockAction',
    kind: 'default',
    alias: 'HideIt.BlockAction.Toggle',
    name: 'Hide It Toggle',
    weight: 1000, // High weight = appears first in action bar
    api: () => import('./hide-it-toggle.action.js'),
    element: () => import('../elements/hide-it-block-action.element.js'),
    meta: {
      icon: 'icon-eye',
      label: 'Toggle visibility',
    },
    conditions: [
      {
        alias: 'HideIt.Condition.HasHideItProperty',
      },
    ],
  },
];
