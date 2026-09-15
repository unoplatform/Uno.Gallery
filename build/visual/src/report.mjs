import { mkdir, writeFile } from "node:fs/promises";
import { dirname } from "node:path";

export async function writeReport(path, report) {
  await mkdir(dirname(path), { recursive: true });
  await writeFile(path, JSON.stringify(report, null, 2) + "\n");
  await writeFile(path.replace(/\.json$/, ".html"), renderHtml(report));
}

function escapeHtml(value) {
  return String(value ?? "").replace(/[&<>"']/g, char => ({
    "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;"
  })[char]);
}

function renderHtml(report) {
  const rows = (report.results ?? []).map(result => `<tr class="${result.passed ? "pass" : "fail"}">
<td>${escapeHtml(result.id)}</td><td>${result.passed ? "PASS" : "FAIL"}</td>
<td>${escapeHtml(result.differentPixels)}</td><td>${escapeHtml(
    result.diffRatio == null ? "" : (result.diffRatio * 100).toFixed(6) + "%"
  )}</td><td>${escapeHtml(result.reason)}</td></tr>`).join("\n");
  return `<!doctype html><meta charset="utf-8"><title>Uno Gallery visual report</title>
<style>body{font-family:system-ui;margin:24px}table{border-collapse:collapse;width:100%}th,td{border:1px solid #ccc;padding:6px;text-align:left}.pass{background:#e8f5e9}.fail{background:#ffebee}code{white-space:pre-wrap}</style>
<h1>Uno Gallery visual regression</h1>
<p>Status: <strong>${report.passed ? "PASS" : "FAIL"}</strong></p>
<p>Browser: ${escapeHtml(report.browserVersion)}; viewport: ${escapeHtml(report.viewport)}</p>
${report.error ? `<h2>Harness failure</h2><code>${escapeHtml(report.error)}</code>` : ""}
<table><thead><tr><th>Sample</th><th>Status</th><th>Different pixels</th><th>Ratio</th><th>Reason</th></tr></thead><tbody>${rows}</tbody></table>`;
}
