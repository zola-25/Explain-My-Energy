import { type Page } from '@playwright/test';
import { APP_LOAD_TIMEOUT, expect, test } from '../support/test';

// Each test loads its route directly, so also checks the app serves it

test('Home page', async ({ page }) => {
    await page.goto('/');

    await expectHeadings(page, 'App Setup Status', 'Weather Data Status', 'Analysis Options Status');
    await expect(page.getByRole('heading', { name: /^Explain My Energy/ }), 'the main header').toBeVisible();
    await expect(page.getByRole('button', { name: 'Welcome Screen' })).toBeVisible();
});

test('Setup page', async ({ page }) => {
    await page.goto('/Setup');

    await expectHeadings(page, 'Household Setup', 'Meter Setup', 'Weather Data Status');
});

for (const { meter, path } of [
    { meter: 'Electricity', path: '/Meter/Electricity' },
    { meter: 'Gas', path: '/HeatingMeter/Gas' },
]) {
    test(`${meter} analysis page`, async ({ page }) => {
        await page.goto(path);

        // The analysis is monthly by default
        await expectHeadings(page, 'Last Month', 'This Month', 'Next Month');
        await expect(page.getByTestId('energy-chart')).toBeVisible();
    });
}

test('Privacy Policy page', async ({ page }) => {
    await page.goto('/PrivacyPolicy');

    await expectHeadings(page, 'Privacy Policy for Explain My Energy');
});

test('Terms & Conditions page', async ({ page }) => {
    await page.goto('/TermsAndConditions');

    await expectHeadings(page, /^Terms and Conditions$/i);
});

test('Credits page', async ({ page }) => {
    await page.goto('/Credits');

    await expectHeadingInAnyFrame(page, 'Explain My Energy - Credits, Attributions & Licenses', { timeout: APP_LOAD_TIMEOUT });
});

test('the links to each route go to its page', async ({ page, baseURL }) => {
    // Starts away from the home page, as the welcome screen may cover it
    await page.goto('/Setup');
    await expectHeadings(page, 'Household Setup');

    // Each page shows a different heading from the one before, so it shows when the new page has rendered
    const steps = [
        { path: '/Meter/Electricity', heading: 'Last Month' },
        { path: '/PrivacyPolicy', heading: 'Privacy Policy for Explain My Energy' },
        { path: '/HeatingMeter/Gas', heading: 'Last Month' },
        { path: '/TermsAndConditions', heading: /^Terms and Conditions$/i },
        { path: '/Credits', heading: 'Explain My Energy - Credits, Attributions & Licenses' },
        { path: '/Setup', heading: 'Household Setup' },
        { path: '/', heading: 'App Setup Status' },
    ];
    for (const { path, heading } of steps) {
        await page.locator(`a[href="${path}"]`).click();

        await expect(page).toHaveURL(new URL(path, baseURL).href);
        await expectHeadingInAnyFrame(page, heading, { timeout: APP_LOAD_TIMEOUT });
    }
});

/** The first heading waits for the app to load */
async function expectHeadings(page: Page, ...names: (string | RegExp)[]): Promise<void> {
    for (const [index, name] of names.entries()) {
        await expect(page.getByRole('heading', { name, exact: true }))
            .toBeVisible({ timeout: index === 0 ? APP_LOAD_TIMEOUT : undefined });
    }
}

/** Also finds headings in a document embedded in the page, as the credits can be */
async function expectHeadingInAnyFrame(page: Page, name: string | RegExp, options?: { timeout?: number }): Promise<void> {
    await expect.poll(async () => {
        for (const frame of page.frames()) {
            try {
                if (await frame.getByRole('heading', { name, exact: true }).first().isVisible()) {
                    return true;
                }
            } catch {
                // The frame went away mid-navigation
            }
        }
        return false;
    }, { message: `Expected the heading ${name} to be visible`, ...options }).toBe(true);
}
