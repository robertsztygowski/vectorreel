// Flags gate what is visible in public, so the DEFAULT is the thing worth testing: an unset,
// empty or ambiguous value must resolve to the safe state. A typo in a deploy command should never
// be what publishes something.

import { test } from 'node:test';
import assert from 'node:assert/strict';
import { isEnabled, SHOW_COLLECTIONS, SHOW_STARTER_PLAN } from './flags';

test('a flag is on only for the exact string "true"', () => {
  assert.equal(isEnabled('true'), true);

  for (const off of [undefined, '', '1', 'TRUE', 'True', 'yes', 'on', 'false', ' true']) {
    assert.equal(isEnabled(off), false, `${JSON.stringify(off)} must be off`);
  }
});

test('both public-surface flags are dark unless the environment says otherwise', () => {
  // The test environment sets neither, which is the state a forgetful production build is in.
  assert.equal(SHOW_COLLECTIONS, isEnabled(process.env.NEXT_PUBLIC_SHOW_COLLECTIONS));
  assert.equal(SHOW_STARTER_PLAN, isEnabled(process.env.NEXT_PUBLIC_SHOW_STARTER));
  assert.equal(SHOW_COLLECTIONS, false, 'collection pages are OFF without an explicit opt-in');
  assert.equal(SHOW_STARTER_PLAN, false, 'the €99 Starter stays dark without an explicit opt-in');
});
