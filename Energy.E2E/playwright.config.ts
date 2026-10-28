import { defineConfig, devices } from '@playwright/test';

// The tests expect the app to be running in demo mode on port 5000: see README.md
export default defineConfig({
    // The app takes 30s+ to load and analyse the demo data in the browser, more when tests run in parallel
    timeout: 300_000,
    expect: { timeout: 15_000 },
    fullyParallel: true,
    forbidOnly: !!process.env.CI,
    retries: process.env.CI ? 1 : 0,
    reporter: process.env.CI ? [['list'], ['html', { open: 'never' }]] : [['list'], ['html', { open: 'on-failure' }]],

    use: {
        baseURL: 'http://localhost:5000',
        // Keeps the browser's date, and so the weather data the app requests, the same wherever the tests run
        timezoneId: 'UTC',
        locale: 'en-GB',
        trace: 'retain-on-failure',
        screenshot: 'only-on-failure',
    },

    projects: [
        {
            name: 'chromium',
            testDir: './tests',
            use: { ...devices['Desktop Chrome'] },
        },
        // Maintenance tool, run explicitly: see README.md
        {
            name: 'record-weather',
            testDir: './tools',
            testMatch: 'record-weather.ts',
            use: { ...devices['Desktop Chrome'] },
        },
    ],
});
