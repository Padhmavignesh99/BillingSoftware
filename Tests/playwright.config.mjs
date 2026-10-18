import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './tests',
  timeout: 30000,
  retries: 0,
  workers: 1,
  fullyParallel: false,
  reporter: [
    ['list'],
    ['json', { outputFile: 'playwright-report/results.json' }],
    ['html', { outputFolder: 'playwright-report', open: 'never' }]
  ],
  use: {
    baseURL: process.env.API_BASE_URL || 'http://localhost:5224',
    extraHTTPHeaders: { 'Content-Type': 'application/json' },
    ignoreHTTPSErrors: true,
  },
});
