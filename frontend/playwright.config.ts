import { defineConfig, devices } from '@playwright/test';

const isCI = !!process.env['CI'];

// Tests de bout en bout : vrai backend .NET + ng serve (proxy /api → http://localhost:5029).
export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: isCI,
  retries: isCI ? 1 : 0,
  reporter: isCI ? [['github'], ['html', { open: 'never' }]] : 'list',
  use: {
    baseURL: 'http://localhost:4200',
    trace: 'retain-on-failure',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
  webServer: [
    {
      command: 'dotnet run --project ../backend/src/FizzBuzz.Api --launch-profile http',
      url: 'http://localhost:5029/health/ready',
      reuseExistingServer: !isCI,
      timeout: 180_000,
    },
    {
      command: 'npm start',
      url: 'http://localhost:4200',
      reuseExistingServer: !isCI,
      timeout: 180_000,
    },
  ],
});
