import type { Page, Route } from '@playwright/test';
import { readFileSync } from 'node:fs';

export const weatherFixturesDir = new URL('../fixtures/weather/', import.meta.url);

/** Hosts whose responses are recorded by `npm run record-weather` and replayed in the tests */
export const recordedApiHosts = [
    'api.postcodes.io',
    'archive-api.open-meteo.com',
    'api.open-meteo.com',
    'climate-api.open-meteo.com',
];

/** The query parameters that pick the data a recorded response holds. Others, and their order, may differ */
const matchedQueryParameters = ['start_date', 'end_date'];

/** Doc snippet tooltips aren't tested; answering 404 keeps the app from waiting on retries */
const docsHost = 'docs.explainmyenergy.net';

export interface RecordedResponse {
    url: string;
    file: string;
}

export interface WeatherRecording {
    /** The fixed current time the app ran with when the responses were recorded */
    fixedUtcNow: string;
    responses: RecordedResponse[];
}

/** Freezes the browser's clock, and so the current time in the app */
export async function fixBrowserTime(page: Page, fixedUtcNow: string): Promise<void> {
    await page.clock.setFixedTime(new Date(fixedUtcNow));
}

export async function answerDocsWithNotFound(page: Page): Promise<void> {
    await page.route(url => url.hostname === docsHost, route => route.fulfill({ status: 404, body: '' }));
}

function matchesRecording(requested: URL, recorded: URL): boolean {
    return requested.origin === recorded.origin
        && requested.pathname === recorded.pathname
        && matchedQueryParameters.every(name => requested.searchParams.get(name) === recorded.searchParams.get(name));
}

/**
 * Fixes the app's current time at the recorded time, and replays the recorded weather and postcode responses.
 * Any other request leaving the app's origin is aborted and added to `unmockedRequests`.
 */
export async function installApiMocks(page: Page, appOrigin: string, unmockedRequests: string[]): Promise<void> {
    const recording: WeatherRecording = JSON.parse(
        readFileSync(new URL('recording.json', weatherFixturesDir), 'utf8'));

    await fixBrowserTime(page, recording.fixedUtcNow);

    await page.route(url => url.origin !== appOrigin, async (route: Route) => {
        const request = route.request();
        const requestedUrl = new URL(request.url());
        const recorded = recording.responses.find(r => matchesRecording(requestedUrl, new URL(r.url)));
        if (request.method() === 'GET' && recorded) {
            await route.fulfill({
                status: 200,
                contentType: 'application/json',
                headers: { 'access-control-allow-origin': '*' },
                body: readFileSync(new URL(recorded.file, weatherFixturesDir)),
            });
            return;
        }
        unmockedRequests.push(`${request.method()} ${request.url()}`);
        await route.abort('blockedbyclient');
    });

    // Registered later, so takes precedence over the catch-all above
    await answerDocsWithNotFound(page);
}
