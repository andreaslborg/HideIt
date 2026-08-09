import type { MetaBlockActionDefaultKind } from '@umbraco-cms/backoffice/block';
import { UmbBlockActionBase, UMB_BLOCK_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/block';
import { UMB_BLOCK_ENTRY_CONTEXT } from '@umbraco-cms/backoffice/block';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbBlockActionArgs } from '@umbraco-cms/backoffice/block';

/**
 * Block action that toggles the "hideIt" property value in the block's settings.
 */
export class HideItToggleAction extends UmbBlockActionBase<MetaBlockActionDefaultKind> {
  #context?: typeof UMB_BLOCK_ENTRY_CONTEXT.TYPE;
  #manager?: typeof UMB_BLOCK_MANAGER_CONTEXT.TYPE;

  constructor(host: UmbControllerHost, args: UmbBlockActionArgs<MetaBlockActionDefaultKind>) {
    super(host, args);

    this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
      this.#context = context;
    });
    
    this.consumeContext(UMB_BLOCK_MANAGER_CONTEXT, (manager) => {
      this.#manager = manager;
    });
  }

  override async execute() {
    if (!this.#context || !this.#manager) {
      return;
    }

    // Get current settings
    const settings = this.#context.getSettings();
    if (!settings) {
      return;
    }

    // Find the current hideIt value in the values array
    const values = settings.values ?? [];
    const hideItIndex = values.findIndex(v => v.alias === 'hideIt');
    const currentValue = hideItIndex >= 0 ? values[hideItIndex].value === true : false;
    
    // Create updated values array
    const newValues = [...values];
    if (hideItIndex >= 0) {
      // Update existing value
      newValues[hideItIndex] = { ...newValues[hideItIndex], value: !currentValue };
    } else {
      // Add new value (shouldn't happen if property exists, but just in case)
      newValues.push({ 
        alias: 'hideIt', 
        value: !currentValue,
        culture: null,
        segment: null,
        editorAlias: 'Umbraco.TrueFalse',
      });
    }
    
    // Create updated settings object and update via manager
    const updatedSettings = {
      ...settings,
      values: newValues,
    };
    
    this.#manager.setOneSettings(updatedSettings);
  }
}

export { HideItToggleAction as api };
