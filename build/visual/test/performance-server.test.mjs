import test from "node:test";
import assert from "node:assert/strict";
import { fork } from "node:child_process";
import { brotliCompressSync } from "node:zlib";
import { mkdtemp, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";

test("performance server models Brotli and immutable cache headers", async () => {
  const root = await mkdtemp(join(tmpdir(), "uno-gallery-performance-server-"));
  const serverPath = new URL("../src/performance-server.mjs", import.meta.url);
  let child;
  try {
    const index = Buffer.from("<html>fixture</html>");
    const script = Buffer.from("console.log('fixture');");
    await writeFile(join(root, "index.html"), index);
    await writeFile(join(root, "app.js"), script);
    await writeFile(join(root, "app.js.br"), brotliCompressSync(script));

    child = fork(serverPath, [root, "0"], {
      stdio: ["ignore", "ignore", "inherit", "ipc"]
    });
    const ready = await new Promise((resolve, reject) => {
      child.once("message", resolve);
      child.once("error", reject);
    });
    const baseUrl = `http://127.0.0.1:${ready.port}`;

    const indexResponse = await fetch(`${baseUrl}/`);
    assert.equal(indexResponse.headers.get("cache-control"), "no-cache");
    const etag = indexResponse.headers.get("etag");
    assert.ok(etag);
    const revalidated = await fetch(`${baseUrl}/`, {
      headers: { "if-none-match": etag }
    });
    assert.equal(revalidated.status, 304);

    const scriptResponse = await fetch(`${baseUrl}/app.js`, {
      headers: { "accept-encoding": "br" }
    });
    assert.equal(scriptResponse.headers.get("content-encoding"), "br");
    assert.equal(
      scriptResponse.headers.get("cache-control"),
      "public, max-age=31536000, immutable"
    );
    assert.equal(await scriptResponse.text(), script.toString());
  } finally {
    if (child?.exitCode === null) {
      child.send({ type: "stop" });
      await new Promise(resolve => child.once("exit", resolve));
    }
    await rm(root, { recursive: true, force: true });
  }
});
