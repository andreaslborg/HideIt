import type { HideItHasPropertyConditionConfig } from './types.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_BLOCK_ENTRY_CONTEXT } from '@umbraco-cms/backoffice/block';

/**
 * Condition that checks if the block's settings element type has a property with alias "hideIt".
 * This enables the Hide It action to only show on blocks that have opted into the feature.
 */
export class HideItHasPropertyCondition
  extends UmbConditionBase<HideItHasPropertyConditionConfig>
  implements UmbExtensionCondition
{
  constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<HideItHasPropertyConditionConfig>) {
    super(host, args);

    this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, async (context) => {
      if (!context) {
        this.permitted = false;
        return;
      }

      // Check if the block has settings and the settings has a "hideIt" property
      const settingsValuesObservable = await context.settingsValues();
      
      this.observe(
        settingsValuesObservable,
        (settingsValues) => {
          // If settingsValues exists and has a "hideIt" key, the property exists
          const hasHideIt = settingsValues !== undefined && 'hideIt' in settingsValues;
          this.permitted = hasHideIt;
        },
        'observeSettingsForHideIt',
      );
    });
  }
}

export default HideItHasPropertyCondition;
