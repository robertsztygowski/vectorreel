import { defineConfig } from '@playwright/test';

// E2E against the docker compose e2e profile (TESTING.md):
//   web  http://localhost:3000   api  http://localhost:8080   postgres  localhost:5432
// Bring the stack up first: scripts/e2e.sh up   (or: docker compose --profile e2e up -d --build)
export default defineConfig({
  testDir: './specs',
  timeout: 120_000,
  expect: { timeout: 10_000 },
  fullyParallel: true,
  retries: 0,
  reporter: [['list'], ['html', { open: 'never' }]],
  outputDir: './test-results',
  use: {
    baseURL: process.env.E2E_WEB_BASE ?? 'http://localhost:3000',
    // Failure artifacts: a model goes from red test to root cause without re-running.
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },
  projects: [{ name: 'chromium', use: { browserName: 'chromium' } }],
});
