import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import type { UmbClassInterface } from '@umbraco-cms/backoffice/class-api';

/**
 * The default property alias used when no custom alias is configured.
 */
export const DEFAULT_HIDE_IT_ALIAS = 'hideIt';

let aliasPromise: Promise<string> | undefined;

/**
 * Resolves the property alias used to hide blocks.
 * The alias is fetched once from the Hide It configuration endpoint and cached,
 * falling back to the default "hideIt" alias if the request fails.
 */
export function getHideItPropertyAlias(host: UmbClassInterface): Promise<string> {
  aliasPromise ??= fetchPropertyAlias(host).catch(() => DEFAULT_HIDE_IT_ALIAS);
  return aliasPromise;
}

async function fetchPropertyAlias(host: UmbClassInterface): Promise<string> {
  const authContext = await host.getContext(UMB_AUTH_CONTEXT);
  if (!authContext) {
    return DEFAULT_HIDE_IT_ALIAS;
  }

  const config = authContext.getOpenApiConfiguration();
  const token = await config.token();

  const response = await fetch(`${config.base}/umbraco/management/api/v1/hideit/configuration`, {
    headers: { Authorization: `Bearer ${token}` },
  });

  if (!response.ok) {
    return DEFAULT_HIDE_IT_ALIAS;
  }

  const data = (await response.json()) as { propertyAlias?: unknown };
  const alias = typeof data.propertyAlias === 'string' ? data.propertyAlias.trim() : '';
  return alias.length > 0 ? alias : DEFAULT_HIDE_IT_ALIAS;
}
