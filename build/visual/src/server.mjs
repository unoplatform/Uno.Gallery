import { createReadStream } from "node:fs";
import { stat } from "node:fs/promises";
import { createServer } from "node:http";
import { extname, resolve, sep } from "node:path";

const root = resolve(process.argv[2]);
const requestedPort = Number(process.argv[3] ?? 0);
const mimeTypes = new Map([
  [".br", "application/octet-stream"],
  [".css", "text/css; charset=utf-8"],
  [".dat", "application/octet-stream"],
  [".dll", "application/octet-stream"],
  [".html", "text/html; charset=utf-8"],
  [".ico", "image/x-icon"],
  [".js", "text/javascript; charset=utf-8"],
  [".json", "application/json; charset=utf-8"],
  [".mjs", "text/javascript; charset=utf-8"],
  [".pdb", "application/octet-stream"],
  [".png", "image/png"],
  [".svg", "image/svg+xml"],
  [".wasm", "application/wasm"],
  [".woff", "font/woff"],
  [".woff2", "font/woff2"]
]);

const server = createServer(async (request, response) => {
  try {
    const rawPath = new URL(request.url, "http://localhost").pathname;
    const relativePath = rawPath === "/" ? "index.html" : decodeURIComponent(rawPath.slice(1));
    const filePath = resolve(root, relativePath);
    if (filePath !== root && !filePath.startsWith(root + sep)) {
      response.writeHead(403).end("Forbidden");
      return;
    }
    const info = await stat(filePath);
    if (!info.isFile()) {
      response.writeHead(404).end("Not found");
      return;
    }
    response.writeHead(200, {
      "Cache-Control": "no-store",
      "Content-Length": info.size,
      "Content-Type": mimeTypes.get(extname(filePath).toLowerCase()) ?? "application/octet-stream"
    });
    createReadStream(filePath).pipe(response);
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
