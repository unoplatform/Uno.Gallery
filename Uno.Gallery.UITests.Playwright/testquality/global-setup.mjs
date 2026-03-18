import fs from 'node:fs';
import path from 'node:path';

export default async function globalSetup() {
  const rootDir = process.cwd();
  const markerDir = path.join(rootDir, 'results', 'testquality');
  const markerPath = path.join(markerDir, 'run-start-epoch.txt');

  fs.mkdirSync(markerDir, { recursive: true });
  fs.writeFileSync(markerPath, String(Date.now()), 'utf8');

  // Remove stale canonical report files so teardown cannot accidentally upload
  // a previous run if Playwright fails before writing a fresh JUnit report.
  const staleCandidates = [
    path.join(rootDir, 'results', 'junit', 'playwright-results.xml'),
    path.join(rootDir, 'results', 'playwright-results.xml'),
  ];

  for (const filePath of staleCandidates) {
    if (fs.existsSync(filePath)) {
      fs.rmSync(filePath, { force: true });
    }
  }

  console.log(`[TestQuality] Run marker written: ${markerPath}`);
}
