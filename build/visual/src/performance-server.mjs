import { createReadStream } from "node:fs";
import { stat } from "node:fs/promises";
import { createServer } from "node:http";
import { basename, extname, resolve, sep } from "node:path";

const root = resolve(process.argv[2]);
const requestedPort = Number(process.argv[3] ?? 0);
const mimeTypes = new Map([
  [".css", "text/css; charset=utf-8"],
  [".dat", "application/octet-stream"],
  [".html", "text/html; charset=utf-8"],
  [".ico", "image/x-icon"],
  [".js", "text/javascript; charset=utf-8"],
  [".json", "application/json; charset=utf-8"],
  [".mjs", "text/javascript; charset=utf-8"],
  [".png", "image/png"],
  [".svg", "image/svg+xml"],
  [".wasm", "application/wasm"],
  [".woff", "font/woff"],
  [".woff2", "font/woff2"]
]);
const noCacheNames = new Set([
  "index.html",
  "service-worker.js",
  "service-worker-assets.js",
  "uno-config.js"
]);

async function tryStat(path) {
  try {
    const info = await stat(path);
    return info.isFile() ? info : null;
  } catch {
    return null;
  }
}

const server = createServer(async (request, response) => {
  try {
    const rawPath = new URL(request.url, "http://localhost").pathname;
    const relativePath = rawPath === "/" ? "index.html" : decodeURIComponent(rawPath.slice(1));
    const filePath = resolve(root, relativePath);
    if (filePath !== root && !filePath.startsWith(root + sep)) {
      response.writeHead(403).end("Forbidden");
      return;
    }

    const original = await tryStat(filePath);
    if (!original) {
      response.writeHead(404).end("Not found");
      return;
    }

    const acceptsBrotli = /\bbr\b/.test(request.headers["accept-encoding"] ?? "");
    const brotliPath = `${filePath}.br`;
    const brotli = acceptsBrotli ? await tryStat(brotliPath) : null;
    const selectedPath = brotli ? brotliPath : filePath;
    const selected = brotli ?? original;
    const fileName = basename(filePath);
    const cacheControl = noCacheNames.has(fileName)
      ? "no-cache"
      : "public, max-age=31536000, immutable";
    const headers = {
      "Cache-Control": cacheControl,
      "Content-Length": selected.size,
      "Content-Type": mimeTypes.get(extname(filePath).toLowerCase()) ?? "application/octet-stream",
      "Vary": "Accept-Encoding"
    };
    if (brotli) {
      headers["Content-Encoding"] = "br";
    }
    response.writeHead(200, headers);
    if (request.method === "HEAD") {
      response.end();
      return;
    }
    createReadStream(selectedPath).pipe(response);
  } catch {
    response.writeHead(404).end("Not found");
  }
});

server.listen(requestedPort, "127.0.0.1", () => {
  const address = server.address();
  process.send?.({ type: "ready", port: address.port, pid: process.pid });
});

process.on("message", message => {
  if (message?.type === "stop") {
    server.close(() => process.exit(0));
  }
});
