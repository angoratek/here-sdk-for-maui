#!/usr/bin/env node
// Promotes the "## [Unreleased]" section of CHANGELOG.md to a dated,
// versioned entry (e.g. "## [4.25.5.0] - 2026-07-16"), then seeds a fresh
// empty "## [Unreleased]" block at the top of the changelog so subsequent
// PRs land in the new unreleased bucket.
//
// Usage:
//   node scripts/release-changelog.js --version 4.25.5.0 --date 2026-07-16
//
// The script preserves everything below the original Unreleased block
// untouched. If the Unreleased block is empty (no sub-sections), the new
// versioned entry is still created with no bullets, and a blank
// "## [Unreleased]" is inserted above it.
//
// Idempotency: re-running with the same --version will throw because the
// versioned entry already exists.

'use strict';

const fs = require('fs');
const path = require('path');

const CHANGELOG_PATH = path.resolve(__dirname, '..', 'CHANGELOG.md');

function parseArgs(argv) {
  const out = {};
  for (let i = 2; i < argv.length; i += 2) {
    const key = argv[i].replace(/^--/, '');
    out[key] = argv[i + 1];
  }
  return out;
}

function assertArgs(args) {
  if (!args.version || !args.date) {
    console.error('Usage: --version <X.Y.Z.W> --date <YYYY-MM-DD>');
    process.exit(1);
  }
  if (!/^\d+(\.\d+){2,3}([-+][0-9A-Za-z.-]+)?$/.test(args.version)) {
    throw new Error(`Invalid --version: ${args.version}`);
  }
  if (!/^\d{4}-\d{2}-\d{2}$/.test(args.date)) {
    throw new Error(`Invalid --date: ${args.date} (expected YYYY-MM-DD)`);
  }
}

function promoteUnreleased(changelog, { version, date }) {
  if (changelog.includes(`## [${version}]`)) {
    throw new Error(`CHANGELOG.md already contains a "[${version}]" entry.`);
  }
  if (!changelog.includes('## [Unreleased]')) {
    throw new Error('CHANGELOG.md is missing the "## [Unreleased]" section.');
  }

  const lines = changelog.split('\n');
  const unreleasedIdx = lines.findIndex((l) => l.startsWith('## [Unreleased]'));

  // Find the end of the Unreleased block (next H2 or EOF).
  let endIdx = lines.length;
  for (let i = unreleasedIdx + 1; i < lines.length; i++) {
    if (/^##\s/.test(lines[i])) {
      endIdx = i;
      break;
    }
  }

  // Slice the body, then strip leading/trailing blank lines.
  const body = lines.slice(unreleasedIdx + 1, endIdx);
  while (body.length && body[0].trim() === '') body.shift();
  while (body.length && body[body.length - 1].trim() === '') body.pop();

  const hasContent = body.some((l) => l.startsWith('### ') || l.startsWith('- '));

  const newVersionedHeader = `## [${version}] - ${date}`;
  const freshUnreleased = [
    '## [Unreleased]',
    '',
    '### Added',
    '',
    '### Changed',
    '',
    '### Fixed',
    '',
  ];

  if (hasContent) {
    const newVersionedBlock = [newVersionedHeader, ...body, ''];
    return [
      ...lines.slice(0, unreleasedIdx),
      ...freshUnreleased,
      '',
      ...newVersionedBlock,
      ...lines.slice(endIdx),
    ].join('\n');
  }

  // No content under Unreleased: drop the empty Unreleased block, insert a
  // fresh one above the new versioned entry so subsequent PRs have a home.
  return [
    ...lines.slice(0, unreleasedIdx),
    ...freshUnreleased,
    '',
    newVersionedHeader,
    '',
    '_No changes since the previous release._',
    '',
    ...lines.slice(endIdx),
  ].join('\n');
}

function main() {
  const args = parseArgs(process.argv);
  assertArgs(args);
  const original = fs.readFileSync(CHANGELOG_PATH, 'utf8');
  const updated = promoteUnreleased(original, args);
  fs.writeFileSync(CHANGELOG_PATH, updated);
  console.log(`Promoted ## [Unreleased] -> ## [${args.version}] - ${args.date}`);
}

if (require.main === module) {
  main();
}

module.exports = { promoteUnreleased };
