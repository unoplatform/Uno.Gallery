import fs from 'node:fs';
import path from 'node:path';
import { spawnSync } from 'node:child_process';

class TestQualityUploadReporter {
  onEnd() {
    const autoUploadRaw = String(process.env.TQ_AUTO_UPLOAD ?? 'true').toLowerCase();
    const autoUploadEnabled = autoUploadRaw !== 'false' && autoUploadRaw !== '0' && autoUploadRaw !== 'no';

    if (!autoUploadEnabled) {
      console.log('[TestQuality] Auto upload disabled (TQ_AUTO_UPLOAD=false).');
      return;
    }

    const rootDir = process.cwd();
    const envXmlPathRaw = process.env.PLAYWRIGHT_JUNIT_OUTPUT_FILE || '';
    const envXmlPath = envXmlPathRaw
      ? (path.isAbsolute(envXmlPathRaw) ? envXmlPathRaw : path.resolve(rootDir, envXmlPathRaw))
      : null;
    const preferredXml = path.join(rootDir, 'results', 'junit', 'playwright-results.xml');
    const legacyXml = path.join(rootDir, 'results', 'playwright-results.xml');
    const xmlPath = (envXmlPath && fs.existsSync(envXmlPath))
      ? envXmlPath
      : (fs.existsSync(preferredXml)
      ? preferredXml
      : (fs.existsSync(legacyXml) ? legacyXml : null));

    if (!xmlPath) {
      console.log('[TestQuality] No JUnit XML found. Skipping upload.');
      return;
    }

    const uploaderPath = path.join(rootDir, 'testquality', 'upload-run-with-metadata.mjs');
    if (!fs.existsSync(uploaderPath)) {
      console.log('[TestQuality] Uploader script missing. Skipping upload.');
      return;
    }

    console.log(`[TestQuality] Uploading run from ${xmlPath}`);

    const result = spawnSync('node', [uploaderPath, '--linked', '--xml', xmlPath], {
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
}

export default TestQualityUploadReporter;
