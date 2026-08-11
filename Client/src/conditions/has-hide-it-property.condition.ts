import type { HideItHasPropertyConditionConfig } from './types.js';
import { getHideItPropertyAlias } from '../hide-it-config.js';
import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbConditionControllerArguments, UmbExtensionCondition } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_BLOCK_ENTRY_CONTEXT, UMB_BLOCK_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/block';

/**
 * Condition that checks if the block's settings element type has a property with the Hide It alias.
 * The alias defaults to "hideIt" and can be customized via the "HideIt:PropertyAlias" app setting.
 * This enables the Hide It action to only show on blocks that have opted into the feature.
 */
export class HideItHasPropertyCondition
  extends UmbConditionBase<HideItHasPropertyConditionConfig>
  implements UmbExtensionCondition
{
  #manager?: typeof UMB_BLOCK_MANAGER_CONTEXT.TYPE;
  #settingsElementTypeKey?: string | null;
  #aliasPromise?: Promise<string>;

  constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<HideItHasPropertyConditionConfig>) {
    super(host, args);

    this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
      if (!context) {
        this.permitted = false;
        return;
      }

      this.observe(
        context.settingsElementTypeKey,
        (settingsElementTypeKey) => {
          this.#settingsElementTypeKey = settingsElementTypeKey;
          void this.#observeHideItProperty();
        },
        'observeSettingsElementTypeKeyForHideIt',
      );
    });

    this.consumeContext(UMB_BLOCK_MANAGER_CONTEXT, (manager) => {
      this.#manager = manager;
      void this.#observeHideItProperty();
    });
  }

  async #observeHideItProperty() {
    const manager = this.#manager;
    const settingsElementTypeKey = this.#settingsElementTypeKey;

    if (!manager || settingsElementTypeKey == null) {
      this.permitted = false;
      return;
    }

    const structure = manager.getStructure(settingsElementTypeKey);
    if (!structure) {
      this.permitted = false;
      return;
    }

    this.#aliasPromise ??= getHideItPropertyAlias(this);
    const alias = await this.#aliasPromise;
    const propertyObservable = await structure.propertyStructureByAlias(alias);

    this.observe(
      propertyObservable,
      (property) => {
        this.permitted = property !== undefined;
      },
      'observeHideItPropertyStructure',
    );
  }
}

export default HideItHasPropertyCondition;
