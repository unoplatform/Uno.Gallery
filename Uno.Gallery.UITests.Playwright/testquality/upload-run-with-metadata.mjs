import fs from 'node:fs';
import path from 'node:path';
import { spawnSync } from 'node:child_process';

const rootDir = process.cwd();

function findUp(startDir, fileName) {
  let current = startDir;
  while (true) {
    const candidate = path.join(current, fileName);
    if (fs.existsSync(candidate)) {
      return candidate;
    }

    const parent = path.dirname(current);
    if (parent === current) {
      return null;
    }

    current = parent;
  }
}

function readUnoSdkVersion() {
  const globalJsonPath = findUp(rootDir, 'global.json');
  if (!globalJsonPath) {
    return null;
  }

  try {
    const parsed = JSON.parse(fs.readFileSync(globalJsonPath, 'utf8'));
    return parsed?.['msbuild-sdks']?.['Uno.Sdk'] || null;
  } catch {
    return null;
  }
}

function getArgValue(name) {
  const prefix = `${name}=`;
  const byEquals = process.argv.find((arg) => arg.startsWith(prefix));
  if (byEquals) {
    return byEquals.slice(prefix.length);
  }

  const index = process.argv.indexOf(name);
  if (index >= 0 && index + 1 < process.argv.length) {
    return process.argv[index + 1];
  }

  return null;
}

function hasArg(name) {
  return process.argv.includes(name);
}

function resolveDefaultXmlPattern() {
  const envXmlPathRaw = process.env.PLAYWRIGHT_JUNIT_OUTPUT_FILE || '';
  if (envXmlPathRaw) {
    const envXmlPath = path.isAbsolute(envXmlPathRaw)
      ? envXmlPathRaw
      : path.resolve(rootDir, envXmlPathRaw);
    if (fs.existsSync(envXmlPath)) {
      return envXmlPath;
    }
  }

  const preferred = path.join(rootDir, 'results', 'junit', 'playwright-results.xml');
  if (fs.existsSync(preferred)) {
    return preferred;
  }

  const legacy = path.join(rootDir, 'results', 'playwright-results.xml');
  if (fs.existsSync(legacy)) {
    return legacy;
  }

  return preferred;
}

const isLinked = hasArg('--linked') || !hasArg('--unlinked');
const isDryRun = hasArg('--dry-run') || String(process.env.TQ_DRY_RUN || '').toLowerCase() === 'true';
const xmlPattern = getArgValue('--xml') || resolveDefaultXmlPattern();
const projectId = process.env.TQ_PROJECT_ID || '34730';
const planId = process.env.TQ_PLAN_ID || '49557';
const delimiter = process.env.TQ_DELIMITER || ' › ';
const forceDelimiter = hasArg('--force-delimiter') || String(process.env.TQ_FORCE_DELIMITER || '').toLowerCase() === 'true';
const noDelimiter = hasArg('--no-delimiter') || String(process.env.TQ_NO_DELIMITER || '').toLowerCase() === 'true';
const runResultOutputDir = process.env.TQ_RUN_RESULT_OUTPUT_DIR || path.join(rootDir, 'test-results');
const runDate = process.env.TQ_RUN_DATE || new Date().toISOString().slice(0, 10);
const runUtc = process.env.TQ_RUN_UTC || new Date().toISOString().slice(11, 16);
const explicitRunName = process.env.TQ_RUN_NAME || getArgValue('--run-name');
const unoSdkVersion = process.env.TQ_UNO_SDK_VERSION || readUnoSdkVersion();

function normalizeXmlForStableMatching(inputPath) {
  if (!fs.existsSync(inputPath)) {
    return { xmlPath: inputPath, hasAttachmentMarkers: false, attachmentPaths: [] };
  }

  const inputDir = path.dirname(inputPath);
  const raw = fs.readFileSync(inputPath, 'utf8');
  const withStableTitles = raw
    // Normalize dash variants to ASCII hyphen to prevent duplicate case keys.
    .replace(/[\u2012\u2013\u2014\u2212]/g, '-')
    .replace(/â€“|â€”|ΓÇô|ΓÇö/g, '-')
    // Normalize known mojibake of the Playwright delimiter.
    .replace(/â€º|ΓÇ║/g, '›');

  let hasAttachmentMarkers = false;
  const attachmentPaths = [];
  const withNormalizedAttachments = withStableTitles.replace(/\[\[ATTACHMENT\|([^\]]+)\]\]/g, (_full, rawPath) => {
    hasAttachmentMarkers = true;

    const trimmed = String(rawPath || '').trim();
    if (!trimmed) {
      return _full;
    }

    const normalizedPath = trimmed.replace(/\\/g, path.sep).replace(/\//g, path.sep);
    const candidateFromXmlDir = path.resolve(inputDir, normalizedPath);
    const absolute = fs.existsSync(candidateFromXmlDir)
      ? candidateFromXmlDir
      : path.resolve(rootDir, normalizedPath);

    if (!fs.existsSync(absolute)) {
      return _full;
    }

    const relativeToOutputDir = path.relative(runResultOutputDir, absolute).replace(/\\/g, '/');
    if (relativeToOutputDir.startsWith('..')) {
      return _full;
    }

    // Keep all attachment types referenced by failing testcases
    // (images, traces, videos, markdown contexts, etc.).
    attachmentPaths.push(relativeToOutputDir);

    return `[[ATTACHMENT|${relativeToOutputDir}]]`;
  });

  if (withNormalizedAttachments === raw) {
    return { xmlPath: inputPath, hasAttachmentMarkers, attachmentPaths };
  }

  const normalizedDir = path.join(rootDir, 'results', 'testquality', 'normalized');
  fs.mkdirSync(normalizedDir, { recursive: true });
  const outPath = path.join(normalizedDir, `${path.basename(inputPath, path.extname(inputPath))}.normalized.xml`);
  fs.writeFileSync(outPath, withNormalizedAttachments, 'utf8');
  return { xmlPath: outPath, hasAttachmentMarkers, attachmentPaths };
}

const normalizedXml = xmlPattern.includes('*')
  ? { xmlPath: xmlPattern, hasAttachmentMarkers: false, attachmentPaths: [] }
  : normalizeXmlForStableMatching(path.resolve(rootDir, xmlPattern));
const xmlInput = normalizedXml.xmlPath;
const hasAttachmentMarkers = normalizedXml.hasAttachmentMarkers;
const attachmentPaths = normalizedXml.attachmentPaths;
const sdkVersionForName = unoSdkVersion || 'unknown';
const runName = explicitRunName || `Gallery - Wasm [Uno.Sdk ${sdkVersionForName}] [${runDate}] [${runUtc}]`;

function prepareAttachmentUploadDir(relativePaths, options = {}) {
  if (!Array.isArray(relativePaths) || relativePaths.length === 0) {
    return { outDir: null, copiedPaths: [], totalCandidates: 0 };
  }

  const {
    outputFolder = 'attachments',
    includePath = () => true,
  } = options;

  const outDir = path.join(rootDir, 'results', 'testquality', outputFolder);
  fs.rmSync(outDir, { recursive: true, force: true });
  fs.mkdirSync(outDir, { recursive: true });

  const copiedPaths = [];

  for (const relativePath of relativePaths) {
    if (!includePath(relativePath)) {
      continue;
    }

    const sourcePath = path.join(runResultOutputDir, relativePath);
    if (!fs.existsSync(sourcePath)) {
      continue;
    }

    const destinationPath = path.join(outDir, relativePath);
    fs.mkdirSync(path.dirname(destinationPath), { recursive: true });
    fs.copyFileSync(sourcePath, destinationPath);
    copiedPaths.push(relativePath);
  }

  return { outDir, copiedPaths, totalCandidates: relativePaths.length };
}

const mediaExtensions = new Set([
  '.png',
  '.jpg',
  '.jpeg',
  '.gif',
  '.bmp',
  '.webp',
  '.svg',
  '.mp4',
  '.webm',
  '.mov',
  '.m4v',
]);

function isImageOrVideoAttachment(relativePath) {
  const ext = path.extname(relativePath || '').toLowerCase();
  return mediaExtensions.has(ext);
}

const markdownExtensions = new Set([
  '.md',
]);

function isMarkdownAttachment(relativePath) {
  const ext = path.extname(relativePath || '').toLowerCase();
  return markdownExtensions.has(ext);
}

function isPrimaryAttachment(relativePath) {
  return isImageOrVideoAttachment(relativePath) || isMarkdownAttachment(relativePath);
}

const primaryAttachments = hasAttachmentMarkers && fs.existsSync(runResultOutputDir)
  ? prepareAttachmentUploadDir(attachmentPaths, {
    outputFolder: 'attachments-primary',
    includePath: isPrimaryAttachment,
  })
  : { outDir: null, copiedPaths: [], totalCandidates: attachmentPaths.length };

const mediaAttachments = hasAttachmentMarkers && fs.existsSync(runResultOutputDir)
  ? prepareAttachmentUploadDir(attachmentPaths, {
    outputFolder: 'attachments-media',
    includePath: isImageOrVideoAttachment,
  })
  : { outDir: null, copiedPaths: [], totalCandidates: attachmentPaths.length };

function shouldUseDelimiter(xmlPath) {
  if (!isLinked || noDelimiter) {
    return false;
  }

  if (forceDelimiter) {
    return true;
  }

  if (!xmlPath || xmlPath.includes('*') || !fs.existsSync(xmlPath)) {
    return true;
  }

  try {
    const raw = fs.readFileSync(xmlPath, 'utf8');
    const firstTestcase = raw.match(/<testcase\b[^>]*\sname="([^"]+)"/);
    if (!firstTestcase || !firstTestcase[1]) {
      return true;
    }

    // When test names are already "Suite › Test", passing delimiter can split again
    // and lead to duplicate case creation in some TestQuality setups.
    return !firstTestcase[1].includes(delimiter);
  } catch {
    return true;
  }
}

const useDelimiter = shouldUseDelimiter(xmlInput);

function buildUploadArgs(attachmentUploadDir) {
  const args = [
    '@testquality/cli',
    'upload_test_run',
    `--project_id=${projectId}`,
    `--run_name=${runName}`,
  ];

  if (isLinked) {
    args.push(`--plan_id=${planId}`);
    if (useDelimiter) {
      args.push(`--delimiter=${delimiter}`);
    }
  }

  if (attachmentUploadDir) {
    args.push(`--run_result_output_dir=${attachmentUploadDir}`);
  }

  args.push(xmlInput);
  return args;
}

const quote = (value) => {
  const v = String(value);
  if (v.length === 0) {
    return '""';
  }
  if (!/[\s"^&|<>]/.test(v)) {
    return v;
  }
  return `"${v.replace(/"/g, '\\"')}"`;
};

function runUploadAttempt(attachmentUploadDir, label) {
  const args = buildUploadArgs(attachmentUploadDir);
  const displayCommand = `npx ${args.map((a) => (a.includes(' ') ? `"${a}"` : a)).join(' ')}`;
  console.log(`Upload command (${label}): ${displayCommand}`);
  if (isLinked) {
    console.log(`Linked mode: delimiter ${useDelimiter ? `enabled (${delimiter})` : 'disabled (already suite-qualified names detected)'}`);
  }
  if (attachmentUploadDir) {
    console.log(`Attachments (${label}): run_result_output_dir=${attachmentUploadDir}`);
  }

  if (isDryRun) {
    console.log(`[DRY RUN] Skipping upload execution (${label}).`);
    return { status: 0, hadKnownUploadError: false };
  }

  const commandLine = `npx ${args.map((a) => quote(a)).join(' ')}`;

  const result = spawnSync(commandLine, [], {
    cwd: rootDir,
    stdio: 'pipe',
    shell: true,
    env: process.env,
    encoding: 'utf8',
  });

  const stdout = String(result.stdout || '');
  const stderr = String(result.stderr || '');
  if (stdout) {
    process.stdout.write(stdout);
  }
  if (stderr) {
    process.stderr.write(stderr);
  }

  const combinedOutput = `${stdout}\n${stderr}`;
  // Keep retry detection strict; generic words like "error" can appear in
  // attachment file names (e.g., error-context.md) and cause false retries.
  const knownUploadErrorPattern = /request entity too large|the given data was invalid|the file field is required when none of files are present|the files field is required when none of file are present|\berror\s+cli\b/i;
  const hadKnownUploadError = knownUploadErrorPattern.test(combinedOutput);

  if (result.error) {
    console.error(`Upload process error (${label}): ${result.error.message}`);
    return { status: 1, hadKnownUploadError: true };
  }

  return {
    status: typeof result.status === 'number' ? result.status : 1,
    hadKnownUploadError,
  };
}

if (primaryAttachments.outDir) {
  console.log(`Primary attachments (images/videos/md): ${primaryAttachments.copiedPaths.length}/${primaryAttachments.totalCandidates}`);
}
if (mediaAttachments.outDir) {
  console.log(`Images/videos eligible for retry: ${mediaAttachments.copiedPaths.length}`);
}

const firstAttempt = runUploadAttempt(primaryAttachments.outDir, 'images-videos-md');

const shouldRetryWithMediaOnly =
  (firstAttempt.status !== 0 || firstAttempt.hadKnownUploadError)
  && primaryAttachments.outDir
  && mediaAttachments.outDir
  && mediaAttachments.copiedPaths.length > 0
  && mediaAttachments.copiedPaths.length < primaryAttachments.copiedPaths.length;

if (!shouldRetryWithMediaOnly) {
  process.exit(firstAttempt.status);
}

console.warn('Upload failed with images/videos/md attachment set. Retrying with images/videos only.');
const retryAttempt = runUploadAttempt(mediaAttachments.outDir, 'images-and-videos-only');
process.exit(retryAttempt.status);
