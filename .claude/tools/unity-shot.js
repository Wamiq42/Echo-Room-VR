#!/usr/bin/env node
/*
 * unity-shot.js — capture a Unity Editor screenshot to a PNG file.
 *
 * The unity-mcp-cli screenshot tools return the image as base64 inside a JSON
 * envelope, which is far too large to echo into a terminal. This wrapper calls
 * the tool, extracts the base64 payload, and writes a PNG to disk.
 *
 * Usage:
 *   node .claude/tools/unity-shot.js <out.png> [tool] [inputJson]
 *
 *   tool defaults to screenshot-game-view. Other useful values:
 *     screenshot-scene-view, screenshot-camera, screenshot-isolated
 *
 * Examples:
 *   node .claude/tools/unity-shot.js Docs/QA/issue-02-before.png
 *   node .claude/tools/unity-shot.js shot.png screenshot-scene-view
 */
const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

const outPath = process.argv[2];
const tool = process.argv[3] || 'screenshot-game-view';
const input = process.argv[4] || '{}';

if (!outPath) {
  console.error('usage: node unity-shot.js <out.png> [tool] [inputJson]');
  process.exit(2);
}

let raw;
try {
  // shell:true — npx is a .cmd shim on Windows and cannot be spawned directly.
  const args = JSON.stringify(input);
  raw = execSync(
    `npx unity-mcp-cli run-tool ${tool} --input ${args}`,
    { encoding: 'utf8', maxBuffer: 256 * 1024 * 1024, shell: true }
  );
} catch (err) {
  console.error('FAIL: unity-mcp-cli call failed');
  console.error((err.stderr || err.message || '').slice(0, 800));
  process.exit(1);
}

// The CLI prints human-readable header lines before the JSON body. Take the
// first '{' onward and parse from there.
const start = raw.indexOf('{');
if (start === -1) {
  console.error('FAIL: no JSON body in CLI output');
  console.error(raw.slice(0, 800));
  process.exit(1);
}

let payload;
try {
  payload = JSON.parse(raw.slice(start));
} catch (err) {
  console.error('FAIL: could not parse CLI JSON: ' + err.message);
  process.exit(1);
}

// Walk the envelope for the first node carrying base64 image data.
function findImage(node) {
  if (node == null || typeof node !== 'object') return null;
  if (Array.isArray(node)) {
    for (const item of node) {
      const hit = findImage(item);
      if (hit) return hit;
    }
    return null;
  }
  if (node.type === 'image' && typeof node.data === 'string' && node.data.length > 0) {
    return node.data;
  }
  for (const key of Object.keys(node)) {
    const hit = findImage(node[key]);
    if (hit) return hit;
  }
  return null;
}

const b64 = findImage(payload);
if (!b64) {
  console.error('FAIL: no image payload found in response');
  console.error(JSON.stringify(payload).slice(0, 600));
  process.exit(1);
}

fs.mkdirSync(path.dirname(path.resolve(outPath)), { recursive: true });
const buf = Buffer.from(b64, 'base64');
fs.writeFileSync(outPath, buf);

// PNG magic check so a corrupt capture fails loudly rather than silently.
const isPng = buf.length > 8 && buf[0] === 0x89 && buf[1] === 0x50 && buf[2] === 0x4e && buf[3] === 0x47;
console.log(`${isPng ? 'OK' : 'WARN (not a PNG header)'}: ${outPath} (${Math.round(buf.length / 1024)} KB)`);
process.exit(isPng ? 0 : 1);
