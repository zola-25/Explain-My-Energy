import { expect, test } from '@playwright/test';
import { mkdirSync, rmSync, writeFileSync } from 'node:fs';
import { answerDocsWithNotFound, fixBrowserTime, recordedApiHosts, weatherFixturesDir, type RecordedResponse, type WeatherRecording } from '../support/mocks';
import { APP_LOAD_TIMEOUT } from '../support/timeouts';

/**
 * Records the real postcodes.io and Open-Meteo responses the demo app requests at a fixed current time,
 * for the tests to replay. Defaults to midday (UTC) today; override with E2E_FIXED_UTC_NOW, e.g.
 *   E2E_FIXED_UTC_NOW=2026-10-27T12:00:00Z npm run record-weather
 */
test('record weather API responses', async ({ page }) => {
    const fixedUtcNow = process.env.E2E_FIXED_UTC_NOW ?? `${new Date().toISOString().slice(0, 10)}T12:00:00Z`;
    expect(Date.parse(fixedUtcNow), `E2E_FIXED_UTC_NOW is not a valid date: ${fixedUtcNow}`).not.toBeNaN();

    const recorded: RecordedResponse[] = [];
    const bodies = new Map<string, Buffer>();

    await page.route(url => recordedApiHosts.includes(url.hostname), async route => {
        const response = await route.fetch();
        expect(response.ok(), `${route.request().url()} responded ${response.status()}`).toBe(true);

        const url = route.request().url();
        const file = `${new URL(url).hostname}.json`;
        recorded.push({ url, file });
        bodies.set(file, await response.body());

        await route.fulfill({ response });
    });
    await answerDocsWithNotFound(page);
    await fixBrowserTime(page, fixedUtcNow);

    await page.goto('/Setup');
    await expect(page.getByRole('heading', { name: 'Weather Data Status', exact: true })).toBeVisible({ timeout: APP_LOAD_TIMEOUT });
    await expect.poll(() => recorded.length, { timeout: 30_000 }).toBe(recordedApiHosts.length);

    rmSync(weatherFixturesDir, { recursive: true, force: true });
    mkdirSync(weatherFixturesDir, { recursive: true });
    for (const [file, body] of bodies) {
        writeFileSync(new URL(file, weatherFixturesDir), body);
    }
    const recording: WeatherRecording = { fixedUtcNow, responses: recorded };
    writeFileSync(new URL('recording.json', weatherFixturesDir), JSON.stringify(recording, null, 4) + '\n');

    console.log(`Recorded ${recorded.length} responses for ${fixedUtcNow}`);
});
