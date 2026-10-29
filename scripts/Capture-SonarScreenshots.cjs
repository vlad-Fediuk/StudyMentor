// Requires playwright in NODE_PATH and Chrome. Credentials are process environment variables only.
const { chromium } = require('playwright');
const fs = require('node:fs/promises');
const path = require('node:path');

(async () => {
  const label = process.argv[2] || 'baseline';
  const project = process.env.SONAR_PROJECT || 'study-mentor';
  if (!/^[a-z-]+$/.test(label)) throw new Error('Invalid screenshot label.');
  if (!process.env.SONAR_PASSWORD) throw new Error('Set SONAR_PASSWORD first.');
  const browser = await chromium.launch({ executablePath: 'C:/Program Files/Google/Chrome/Application/chrome.exe', headless: true });
  try {
    const context = await browser.newContext({ viewport: { width: 1440, height: 1200 } });
    const base = 'http://127.0.0.1:9000';
    const login = await context.request.post(`${base}/api/authentication/login`, {
      form: { login: 'admin', password: process.env.SONAR_PASSWORD }
    });
    if (!login.ok()) throw new Error(`Local login failed: ${login.status()}`);
    const page = await context.newPage();
    const output = path.resolve(__dirname, '../doc/module-3/screenshots/sonar');
    await fs.mkdir(output, { recursive: true });
    await page.goto(`${base}/dashboard?id=${encodeURIComponent(project)}`, { waitUntil: 'networkidle', timeout: 120000 });
    for (let i = 0; i < 3; i++) {
      const dismiss = page.getByRole('button', { name: 'Dismiss', exact: true });
      if (!await dismiss.count()) break;
      await dismiss.first().click();
    }
    await page.getByText('Overall Code', { exact: true }).click();
    await page.waitForLoadState('networkidle');
    await page.screenshot({ path: path.join(output, `${label}-dashboard.png`), fullPage: true });
    await page.goto(`${base}/project/issues?id=${encodeURIComponent(project)}&resolved=false`, { waitUntil: 'networkidle', timeout: 120000 });
    await page.screenshot({ path: path.join(output, `${label}-issues.png`), fullPage: true });
    console.log(`Saved ${label} dashboard and issues screenshots.`);
  } finally { await browser.close(); }
})().catch(error => { console.error(error.message); process.exitCode = 1; });
