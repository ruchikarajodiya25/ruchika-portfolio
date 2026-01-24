import { test, expect } from '@playwright/test';

test.describe('Authentication Flow', () => {
  test('should login successfully', async ({ page }) => {
    await page.goto('http://localhost:5173/login');
    
    // Fill in login form
    await page.fill('input[type="email"]', 'owner@demo.com');
    await page.fill('input[type="password"]', 'Demo@123');
    await page.click('button[type="submit"]');
    
    // Wait for redirect to dashboard
    await page.waitForURL('**/dashboard');
    await expect(page).toHaveURL(/.*dashboard/);
  });
});

test.describe('Customer Management', () => {
  test.beforeEach(async ({ page }) => {
    // Login first
    await page.goto('http://localhost:5173/login');
    await page.fill('input[type="email"]', 'owner@demo.com');
    await page.fill('input[type="password"]', 'Demo@123');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/dashboard');
  });

  test('should create a new customer', async ({ page }) => {
    await page.goto('http://localhost:5173/customers');
    
    // Click add customer button
    await page.click('text=Add Customer');
    
    // Fill customer form
    await page.fill('input[name="firstName"]', 'John');
    await page.fill('input[name="lastName"]', 'Doe');
    await page.fill('input[name="email"]', 'john.doe@example.com');
    await page.fill('input[name="phone"]', '555-1234');
    
    // Submit form
    await page.click('button[type="submit"]');
    
    // Verify customer appears in list
    await expect(page.locator('text=John Doe')).toBeVisible();
  });

  test('should search for customers', async ({ page }) => {
    await page.goto('http://localhost:5173/customers');
    
    // Search for customer
    await page.fill('input[placeholder*="Search"]', 'John');
    
    // Wait for results
    await page.waitForTimeout(500);
    
    // Verify search results
    const results = page.locator('tbody tr');
    await expect(results.first()).toBeVisible();
  });
});

test.describe('Appointment Booking', () => {
  test.beforeEach(async ({ page }) => {
    // Login first
    await page.goto('http://localhost:5173/login');
    await page.fill('input[type="email"]', 'owner@demo.com');
    await page.fill('input[type="password"]', 'Demo@123');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/dashboard');
  });

  test('should book an appointment', async ({ page }) => {
    await page.goto('http://localhost:5173/appointments');
    
    // Click book appointment button
    await page.click('text=Book Appointment');
    
    // Fill appointment form
    // Note: Adjust selectors based on actual form structure
    await page.selectOption('select[name="customerId"]', { index: 1 });
    await page.selectOption('select[name="serviceId"]', { index: 1 });
    
    // Select date/time (adjust based on actual date picker)
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    const dateStr = tomorrow.toISOString().split('T')[0];
    await page.fill('input[type="date"]', dateStr);
    
    // Submit
    await page.click('button[type="submit"]');
    
    // Verify appointment appears
    await expect(page.locator('text=Appointment')).toBeVisible();
  });
});
