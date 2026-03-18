import { readFileSync } from 'fs';
const h = readFileSync('playwright-report/index.html', 'utf8');
// The Playwright HTML report embeds data as a base64-encoded blob
const m = h.match(/window\.__pw_report_data\s*=\s*"([^"]+)"/);
if (m) {
  const d = JSON.parse(Buffer.from(m[1], 'base64'));
  console.log('stats:', JSON.stringify(d.stats, null, 2));
  const suites = d.suites || [];
  const tests = suites.flatMap(s => s.suites?.flatMap(ss => ss.tests || []) || s.tests || []);
  tests.forEach(t => {
    const status = t.results?.[t.results.length-1]?.status ?? 'unknown';
    console.log(`  [${status}] ${t.title}`);
  });
} else {
  // Try alternative format
  const m2 = h.match(/window\.__pw_report_data\s*=\s*'([^']+)'/);
  if (m2) {
    const d = JSON.parse(Buffer.from(m2[1], 'base64'));
    console.log('stats:', JSON.stringify(d.stats, null, 2));
  } else {
    console.log('Could not find report data. Report size:', h.length);
    // Print a snippet to understand the format
    const idx = h.indexOf('pw_report');
    if (idx > -1) console.log('snippet:', h.substring(idx, idx+200));
  }
}
