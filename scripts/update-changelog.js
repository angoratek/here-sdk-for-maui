#!/usr/bin/env node
// Appends a conventional-commit entry to the "## [Unreleased]" section of
// CHANGELOG.md. Invoked by .github/workflows/changelog.yml on PR merge.
//
// Usage:
//   node scripts/update-changelog.js \
//     --title "feat(refapp): add map dark mode toggle" \
//     --pr 123 \
//     --author "alice" \
//     --sha "abc1234"
//
// The entry is grouped under one of these sub-headings, derived from the
// conventional-commit type:
//
//   feat     -> ### Added
//   fix      -> ### Fixed
//   refactor -> ### Changed
//   perf     -> ### Changed
//   docs     -> ### Documentation
//   test     -> ### Testing
//   ci       -> ### CI
//   chore    -> ### Chore
//   build    -> ### CI
//   style    -> Changed
//
// Anything else (e.g. "wip:" or unscoped) goes under ### Other.
// The first line of the title is the bullet; scope "(scope)" is stripped from
// the displayed text but preserved for grouping. A back-link to the PR is
// appended.

'use strict';

const fs = require('fs');
const path = require('path');

const CHANGELOG_PATH = path.resolve(__dirname, '..', 'CHANGELOG.md');

const TYPE_TO_SECTION = {
  feat: '### Added',
  fix: '### Fixed',
  refactor: '### Changed',
  perf: '### Changed',
  docs: '### Documentation',
  test: '### Testing',
  ci: '### CI',
  build: '### CI',
  chore: '### Chore',
  style: '### Changed',
};

const KNOWN_SECTIONS = [
  '### Added',
  '### Changed',
  '### Fixed',
  '### Removed',
  '### Deprecated',
  '### Security',
  '### Documentation',
  '### Testing',
  '### CI',
  '### Chore',
  '### Other',
];

function parseArgs(argv) {
  const out = {};
  for (let i = 2; i < argv.length; i += 2) {
    const key = argv[i].replace(/^--/, '');
    out[key] = argv[i + 1];
  }
  return out;
}

function parseTitle(rawTitle) {
  // Conventional commits: <type>[(scope)][!]: <subject>
  // e.g.  feat(refapp): add dark mode toggle
  //       fix: handle null route
  const match = rawTitle.match(/^([a-zA-Z]+)(?:\(([^)]+)\))?(!)?:\s*(.+)$/);
  if (!match) {
    return { type: null, scope: null, breaking: false, subject: rawTitle.trim() };
  }
  return {
    type: match[1].toLowerCase(),
    scope: match[2] || null,
    breaking: match[3] === '!',
    subject: match[4].trim(),
  };
}

function buildEntry({ title, pr, author, sha }) {
  const { type, subject } = parseTitle(title);
  const section = (type && TYPE_TO_SECTION[type]) || '### Other';

  // Prefer the PR link; fall back to the commit SHA.
  let refLink;
  if (pr) {
    refLink = `[#${pr}](https://github.com/${process.env.GITHUB_REPOSITORY || 'angoratek/here-sdk-for-maui'}/pull/${pr})`;
  } else if (sha) {
    const short = sha.slice(0, 7);
    refLink = `[${short}](https://github.com/${process.env.GITHUB_REPOSITORY || 'angoratek/here-sdk-for-maui'}/commit/${sha})`;
  } else {
    refLink = '';
  }

  const authorTag = author ? ` (@${author})` : '';
  const bullet = `- ${subject}${authorTag}${refLink ? ' ' + refLink : ''}`;
  return { section, bullet };
}

function insertEntry(changelog, { section, bullet }) {
  if (!changelog.includes('## [Unreleased]')) {
    throw new Error('CHANGELOG.md is missing the "## [Unreleased]" section.');
  }

  const lines = changelog.split('\n');
  const unreleasedIdx = lines.findIndex((l) => l.startsWith('## [Unreleased]'));

  // Find the insertion point: end of the Unreleased block, which is the
  // next H2 ('## ') heading.
  let endIdx = lines.length;
  for (let i = unreleasedIdx + 1; i < lines.length; i++) {
    if (/^##\s/.test(lines[i])) {
      endIdx = i;
      break;
    }
  }

  // Locate the target sub-section. If it already exists, append to the end
  // of its bullet list. Otherwise, insert a new sub-section at the end of
  // the Unreleased block.
  const sectionIdx = lines
    .slice(unreleasedIdx, endIdx)
    .findIndex((l) => l.trim() === section);

  if (sectionIdx === -1) {
    // Insert a new sub-section header at the end of the Unreleased block,
    // grouped alongside the existing ones (Added / Changed / Fixed first,
    // then Documentation / Testing / CI / Chore / Other).
    const insertAt = endIdx;
    const insertion = ['', section, bullet];
    lines.splice(insertAt, 0, ...insertion);
    return lines.join('\n');
  }

  // Find the last bullet in the section, then insert immediately after.
  const absoluteSectionIdx = unreleasedIdx + sectionIdx;
  let insertAt = absoluteSectionIdx + 1;
  for (let i = absoluteSectionIdx + 1; i < endIdx; i++) {
    if (lines[i].startsWith('### ')) {
      break;
    }
    if (lines[i].startsWith('- ') || lines[i].trim() === '') {
      insertAt = i + 1;
    }
  }
  lines.splice(insertAt, 0, bullet);
  return lines.join('\n');
}

function main() {
  const args = parseArgs(process.argv);
  if (!args.title) {
    console.error('Missing --title');
    process.exit(1);
  }

  const original = fs.readFileSync(CHANGELOG_PATH, 'utf8');
  const entry = buildEntry(args);
  const updated = insertEntry(original, entry);
  fs.writeFileSync(CHANGELOG_PATH, updated);
  console.log(`Added to ${entry.section}: ${entry.bullet}`);
}

if (require.main === module) {
  main();
}

module.exports = { parseTitle, buildEntry, insertEntry, TYPE_TO_SECTION, KNOWN_SECTIONS };
