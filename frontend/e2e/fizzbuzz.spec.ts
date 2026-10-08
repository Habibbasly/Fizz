import { Page, expect, test } from '@playwright/test';

async function fill(page: Page, values: Partial<Record<'int1' | 'int2' | 'limit' | 'str1' | 'str2', string>>) {
  for (const [name, value] of Object.entries(values)) {
    await page.getByLabel(name, { exact: true }).fill(value);
  }
}

test.beforeEach(async ({ page }) => {
  await page.goto('/');
});

test('génère la séquence FizzBuzz par défaut via le backend', async ({ page }) => {
  await page.getByRole('button', { name: 'Générer' }).click();

  const values = page.locator('li .value');
  await expect(values).toHaveCount(15);
  await expect(values.nth(2)).toHaveText('Fizz');
  await expect(values.nth(4)).toHaveText('Buzz');
  await expect(values.nth(14)).toHaveText('FizzBuzz');
});

test('utilise les paramètres saisis', async ({ page }) => {
  await fill(page, { int1: '2', str1: 'Foo', int2: '7', str2: 'Bar', limit: '14' });
  await page.getByRole('button', { name: 'Générer' }).click();

  const values = page.locator('li .value');
  await expect(values).toHaveCount(14);
  await expect(values.nth(1)).toHaveText('Foo');
  await expect(values.nth(6)).toHaveText('Bar');
  await expect(values.nth(13)).toHaveText('FooBar');
});

test('affiche l’erreur renvoyée par l’API pour une limite trop grande', async ({ page }) => {
  await fill(page, { limit: '20000' });
  await page.getByRole('button', { name: 'Générer' }).click();

  await expect(page.locator('.error')).toContainText('ne peut pas dépasser 10000');
  await expect(page.locator('li')).toHaveCount(0);
});

test('affiche l’erreur renvoyée par l’API pour un diviseur nul', async ({ page }) => {
  await fill(page, { int1: '0' });
  await page.getByRole('button', { name: 'Générer' }).click();

  await expect(page.locator('.error')).toContainText('strictement positif');
});
