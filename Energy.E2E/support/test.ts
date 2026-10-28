import { test as base, expect } from '@playwright/test';
import { installApiMocks } from './mocks';

/**
 * Every page runs the app with a fixed current time and recorded weather data,
 * and the test fails if the app made an external request that has no recorded response.
 */
export const test = base.extend({
    page: async ({ page, baseURL }, use) => {
        const unmockedRequests: string[] = [];
        await installApiMocks(page, new URL(baseURL!).origin, unmockedRequests);

        await use(page);

        expect(unmockedRequests,
            'External requests without a recorded response. If the app\'s weather requests changed, ' +
            're-record them with `npm run record-weather`').toEqual([]);
    },
});

export { expect };
export { APP_LOAD_TIMEOUT } from './timeouts';
