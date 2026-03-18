import fs from 'node:fs';
import path from 'node:path';
import { spawnSync } from 'node:child_process';

const rootDir = process.cwd();
const mapPath = path.join(rootDir, 'testquality-folder-spec-map.json');
const preferredInputXmlPath = path.join(rootDir, 'results', 'junit', 'playwright-results.xml');
const legacyInputXmlPath = path.join(rootDir, 'results', 'playwright-results.xml');
const inputXmlPath = process.argv[2]
  || (fs.existsSync(preferredInputXmlPath) ? preferredInputXmlPath : (fs.existsSync(legacyInputXmlPath) ? legacyInputXmlPath : preferredInputXmlPath));
const outputDir = path.join(rootDir, 'results', 'testquality', 'by-folder');
const inputXmlDir = path.dirname(inputXmlPath);

const projectId = process.env.TQ_PROJECT_ID || '34730';
const planId = process.env.TQ_PLAN_ID || '49557';
const delimiter = process.env.TQ_DELIMITER || ' › ';
const runResultOutputDir = process.env.TQ_RUN_RESULT_OUTPUT_DIR || path.join(rootDir, 'test-results');
const runDate = process.env.TQ_RUN_DATE || new Date().toISOString().slice(0, 10);
const runUtc = process.env.TQ_RUN_UTC || new Date().toISOString().slice(11, 16);
const explicitRunName = process.env.TQ_RUN_NAME || null;
const isDryRun = (process.env.TQ_DRY_RUN || '').toLowerCase() === '1'
  || (process.env.TQ_DRY_RUN || '').toLowerCase() === 'true';

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

const unoSdkVersion = process.env.TQ_UNO_SDK_VERSION || readUnoSdkVersion();

if (!fs.existsSync(mapPath)) {
  console.error(`Mapping file not found: ${mapPath}`);
  process.exit(1);
}

if (!fs.existsSync(inputXmlPath)) {
  console.error(`JUnit XML not found: ${inputXmlPath}`);
  process.exit(1);
}

const mapping = JSON.parse(fs.readFileSync(mapPath, 'utf8'));
const xml = fs.readFileSync(inputXmlPath, 'utf8');

if (!fs.existsSync(outputDir)) {
  fs.mkdirSync(outputDir, { recursive: true });
}

function attr(source, name) {
  const rx = new RegExp(`${name}="([^"]*)"`);
  const match = source.match(rx);
  return match ? match[1] : '';
}

function suiteBlocks(xmlText) {
  const rx = /<testsuite\b[^>]*>[\s\S]*?<\/testsuite>/g;
  return xmlText.match(rx) || [];
}

function uploadFile(xmlFile, runName) {
  if (isDryRun) {
    console.log(`[DRY RUN] npx @testquality/cli upload_test_run --project_id=${projectId} --plan_id=${planId} --run_name=${runName} --delimiter=${delimiter} --run_result_output_dir=${runResultOutputDir} ${xmlFile}`);
    return true;
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

  const commandLine = [
    'npx',
    '@testquality/cli',
    'upload_test_run',
    `--project_id=${quote(projectId)}`,
    `--plan_id=${quote(planId)}`,
    `--run_name=${quote(runName)}`,
    `--delimiter=${quote(delimiter)}`,
    `--run_result_output_dir=${quote(runResultOutputDir)}`,
    quote(xmlFile),
  ].join(' ');

  const result = spawnSync(commandLine, [], {
    cwd: rootDir,
    stdio: 'pipe',
    shell: true,
    encoding: 'utf8',
  });

  if (result.stdout) {
    process.stdout.write(result.stdout);
  }
  if (result.stderr) {
    process.stderr.write(result.stderr);
  }

  if (result.error) {
    console.error(`Upload process error: ${result.error.message}`);
  }
  if (typeof result.status === 'number' && result.status !== 0) {
    console.error(`Upload process exited with code ${result.status}`);
  }

  return result.status === 0;
}

const suites = suiteBlocks(xml);
const bySuiteName = new Map();

for (const suite of suites) {
  const name = attr(suite, 'name');
  if (name) {
    bySuiteName.set(name, suite);
  }
}

function normalizeAttachmentMarkers(xmlBlock) {
  const marker = /\[\[ATTACHMENT\|([^\]]+)\]\]/g;
  return xmlBlock.replace(marker, (_full, rawPath) => {
    const trimmed = String(rawPath || '').trim();
    if (!trimmed) {
      return _full;
    }

    const absolute = path.isAbsolute(trimmed)
      ? trimmed
      : (() => {
          const normalized = trimmed.replace(/\\/g, path.sep).replace(/\//g, path.sep);
          const candidateFromXmlDir = path.resolve(inputXmlDir, normalized);
          if (fs.existsSync(candidateFromXmlDir)) {
            return candidateFromXmlDir;
          }

          return path.resolve(rootDir, normalized);
        })();

    const relative = path.relative(runResultOutputDir, absolute).replace(/\\/g, '/');

    return `[[ATTACHMENT|${relative}]]`;
  });
}

let uploaded = 0;
let skipped = 0;

for (const entry of mapping) {
  const suiteName = path.basename(entry.specFile);
  const suiteXml = bySuiteName.get(suiteName);

  if (!suiteXml) {
    console.log(`Skipping ${suiteName}: suite not found in JUnit file.`);
    skipped += 1;
    continue;
  }

  const tests = attr(suiteXml, 'tests') || '0';
  const failures = attr(suiteXml, 'failures') || '0';
  const skippedCount = attr(suiteXml, 'skipped') || '0';
  const errors = attr(suiteXml, 'errors') || '0';
  const time = attr(suiteXml, 'time') || '0';
  const normalizedSuiteXml = normalizeAttachmentMarkers(suiteXml);

  const wrapped = [
    '<?xml version="1.0" encoding="UTF-8"?>',
    `<testsuites name="${suiteName}" tests="${tests}" failures="${failures}" skipped="${skippedCount}" errors="${errors}" time="${time}">`,
    normalizedSuiteXml,
    '</testsuites>',
    '',
  ].join('\n');

  const outFile = path.join(outputDir, suiteName.replace('.spec.ts', '.xml'));
  fs.writeFileSync(outFile, wrapped, 'utf8');

  const sdkVersionForName = unoSdkVersion || 'unknown';
  const runName = explicitRunName || `Gallery - Wasm [Uno.Sdk ${sdkVersionForName}] [${runDate}] [${runUtc}]`;
  console.log(`Uploading ${suiteName} as ${runName}`);

  const ok = uploadFile(outFile, runName);
  if (ok) {
    uploaded += 1;
  } else {
    console.error(`Upload failed for ${suiteName}`);
    process.exit(1);
  }
}

console.log(`Completed folder-mapped uploads. Uploaded: ${uploaded}, Skipped: ${skipped}`);
