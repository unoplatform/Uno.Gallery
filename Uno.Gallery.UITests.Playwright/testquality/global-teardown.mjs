import fs from 'node:fs';
import path from 'node:path';
import { spawnSync } from 'node:child_process';

export default async function globalTeardown() {
  const rootDir = process.cwd();
  const autoUploadRaw = String(process.env.TQ_AUTO_UPLOAD ?? 'true').toLowerCase();
  const autoUploadEnabled = autoUploadRaw !== 'false' && autoUploadRaw !== '0' && autoUploadRaw !== 'no';
  const markerPath = path.join(rootDir, 'results', 'testquality', 'run-start-epoch.txt');

  if (!autoUploadEnabled) {
    console.log('[TestQuality] Auto upload disabled (TQ_AUTO_UPLOAD=false).');
    return;
  }

  const runStartEpoch = (() => {
    try {
      if (!fs.existsSync(markerPath)) {
        return 0;
      }

      const raw = fs.readFileSync(markerPath, 'utf8').trim();
      const value = Number.parseInt(raw, 10);
      return Number.isFinite(value) ? value : 0;
    } catch {
      return 0;
    }
  })();

  const candidateDirs = [
    path.join(rootDir, 'results', 'junit'),
    path.join(rootDir, 'results'),
  ];

  const xmlCandidates = candidateDirs
    .filter((dirPath) => fs.existsSync(dirPath))
    .flatMap((dirPath) => fs.readdirSync(dirPath)
      .filter((fileName) => fileName.toLowerCase().endsWith('.xml'))
      .map((fileName) => path.join(dirPath, fileName)))
    .map((filePath) => ({
      filePath,
      mtimeMs: fs.statSync(filePath).mtimeMs,
    }))
    .filter((item) => item.mtimeMs >= runStartEpoch)
    .sort((a, b) => b.mtimeMs - a.mtimeMs);

  const xmlPath = xmlCandidates.length > 0 ? xmlCandidates[0].filePath : null;

  if (!xmlPath) {
    console.log('[TestQuality] No fresh JUnit XML found for this run. Skipping upload.');
    return;
  }

  const uploaderPath = path.join(rootDir, 'testquality', 'upload-run-with-metadata.mjs');
  if (!fs.existsSync(uploaderPath)) {
    console.log('[TestQuality] Uploader script missing. Skipping upload.');
    return;
  }

  const uploadArgs = [uploaderPath, '--linked', '--xml', xmlPath];
  console.log(`[TestQuality] Uploading run from ${xmlPath}`);

  const result = spawnSync('node', uploadArgs, {
    cwd: rootDir,
    stdio: 'inherit',
    shell: false,
    env: process.env,
  });

  if (result.error) {
    throw new Error(`[TestQuality] Upload process error: ${result.error.message}`);
  }

  if (typeof result.status === 'number' && result.status !== 0) {
    throw new Error(`[TestQuality] Upload failed with exit code ${result.status}.`);
  }

  console.log('[TestQuality] Upload completed.');
}
