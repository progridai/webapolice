import { ENV } from '../../app/config/env';

export const APP_CONFIG = {
  appName: 'WebApólice',
  version: ENV.APP_VERSION,
  environment: ENV.MODE === 'development' ? 'Development' : ENV.MODE,
};
