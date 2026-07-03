// app-config.ts

export const APP_CONFIG = {
  appName: 'WebApólice',
  version: '0.1.0',
  environment: import.meta.env.MODE === 'development' ? 'Development' : import.meta.env.MODE,
};
