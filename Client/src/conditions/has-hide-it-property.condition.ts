import type { HideItHasPropertyConditionConfig } from './types.js';
import { getHideItPropertyAlias } from '../hide-it-config.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_BLOCK_ENTRY_CONTEXT } from '@umbraco-cms/backoffice/block';

/**
 * Condition that checks if the block's settings element type has a property with the Hide It alias.
 * The alias defaults to "hideIt" and can be customized via the "HideIt:PropertyAlias" app setting.
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

      const alias = await getHideItPropertyAlias(this);

      // Check if the block has settings and the settings has a property with the Hide It alias
      const settingsValuesObservable = await context.settingsValues();

      this.observe(
        settingsValuesObservable,
        (settingsValues) => {
          // If settingsValues exists and has the alias as a key, the property exists
          const hasHideIt = settingsValues !== undefined && alias in settingsValues;
          this.permitted = hasHideIt;
        },
        'observeSettingsForHideIt',
      );
    });
  }
}

export default HideItHasPropertyCondition;
