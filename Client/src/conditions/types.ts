import type { UmbConditionConfigBase } from '@umbraco-cms/backoffice/extension-api';

export type HideItHasPropertyConditionConfig = UmbConditionConfigBase<'HideIt.Condition.HasHideItProperty'>;

declare global {
  interface UmbExtensionConditionConfigMap {
    hideItHasProperty: HideItHasPropertyConditionConfig;
  }
}
