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
  #evaluationId = 0;

  constructor(host: UmbControllerHost, args: UmbConditionControllerArguments<HideItHasPropertyConditionConfig>) {
    super(host, args);

    this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
      if (!context) {
        this.#settingsElementTypeKey = undefined;
        this.#scheduleEvaluation();
        return;
      }

      this.observe(
        context.settingsElementTypeKey,
        (settingsElementTypeKey) => {
          this.#settingsElementTypeKey = settingsElementTypeKey;
          this.#scheduleEvaluation();
        },
        'observeSettingsElementTypeKeyForHideIt',
      );
    });

    this.consumeContext(UMB_BLOCK_MANAGER_CONTEXT, (manager) => {
      this.#manager = manager;

      if (!manager) {
        this.removeUmbControllerByAlias('observeBlockTypesForHideIt');
        this.#scheduleEvaluation();
        return;
      }

      // Re-evaluate when Umbraco registers structures for newly loaded block types.
      this.observe(
        manager.blockTypes,
        () => this.#scheduleEvaluation(),
        'observeBlockTypesForHideIt',
      );
    });
  }

  #scheduleEvaluation() {
    const evaluationId = ++this.#evaluationId;

    this.removeUmbControllerByAlias('observeHideItPropertyStructure');
    this.permitted = false;

    void this.#observeHideItProperty(evaluationId).catch((error: unknown) => {
      if (evaluationId !== this.#evaluationId) return;

      console.error('[HideIt] Error checking the settings property structure:', error);
    });
  }

  async #observeHideItProperty(evaluationId: number) {
    const manager = this.#manager;
    const settingsElementTypeKey = this.#settingsElementTypeKey;

    if (!manager || settingsElementTypeKey == null) {
      return;
    }

    await manager.contentTypesLoaded;
    if (!this.#isCurrentEvaluation(evaluationId, manager, settingsElementTypeKey)) return;

    const structure = manager.getStructure(settingsElementTypeKey);
    if (!structure) return;

    this.#aliasPromise ??= getHideItPropertyAlias(this);
    const alias = await this.#aliasPromise;
    if (!this.#isCurrentEvaluation(evaluationId, manager, settingsElementTypeKey)) return;

    const propertyObservable = await structure.propertyStructureByAlias(alias);
    if (!this.#isCurrentEvaluation(evaluationId, manager, settingsElementTypeKey)) return;

    this.observe(
      propertyObservable,
      (property) => {
        this.permitted = property !== undefined;
      },
      'observeHideItPropertyStructure',
    );
  }

  #isCurrentEvaluation(
    evaluationId: number,
    manager: typeof UMB_BLOCK_MANAGER_CONTEXT.TYPE,
    settingsElementTypeKey: string,
  ) {
    return (
      evaluationId === this.#evaluationId &&
      manager === this.#manager &&
      settingsElementTypeKey === this.#settingsElementTypeKey
    );
  }
}

export default HideItHasPropertyCondition;
