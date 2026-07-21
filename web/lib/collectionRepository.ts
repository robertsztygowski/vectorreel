import { readFileSync, existsSync, readdirSync } from 'node:fs';
import { join } from 'node:path';
import { parseDocument, type OutputDocument } from './outputDocument';

// Server-only reader for an ARCHITECTURE.md §4b repository (v1.1). Never import from a Client
// Component.
//
// The site pages are DERIVED FROM THE CONTRACT, not hand-authored per collection: the same manifest
// that ships inside the GitHub repository drives the pages here. That is the point — if the two ever
// disagreed, the public collection would stop being a demo of the private deliverable.

export type { OutputDocument } from './outputDocument';

export type InclusionTier = 'full' | 'reference';

export interface ManifestCitation {
  timestamp: string;
  label: string;
}

export interface ManifestSession {
  slug: string;
  inclusion?: InclusionTier;
  file?: string;
  title: string;
  duration?: string;
  language?: string;
  processed_at?: string;
  recorded_at?: string;
  source: string;
  event?: string;
  year?: number;
  licence?: string;
  licence_verified_via?: string;
  citations?: ManifestCitation[];
  tags: string[];
  speakers?: string[];
  attribution?: string;
}

export interface ManifestIndex {
  slug: string;
  file: string;
  label?: string;
  name?: string;
  sessions: string[];
}

export interface RepositoryManifest {
  repository: {
    name: string;
    description: string;
    generator: string;
    generated_at: string;
    visibility: 'public-collection' | 'private';
    contract_version: string;
    licence_note?: string;
  };
  sessions: ManifestSession[];
  topics: ManifestIndex[];
  speakers: ManifestIndex[];
}

export interface Collection {
  slug: string;
  manifest: RepositoryManifest;
}

const REPOSITORIES_DIR = join(process.cwd(), 'fixtures');

// §4b v1.1: an absent tier means `full`, so a v1 manifest keeps its meaning.
export const tierOf = (session: ManifestSession): InclusionTier => session.inclusion ?? 'full';

export const isFull = (session: ManifestSession) => tierOf(session) === 'full';

export function listCollections(): string[] {
  const root = join(REPOSITORIES_DIR, 'repository');
  return existsSync(join(root, 'metadata', 'manifest.json')) ? ['repository'] : [];
}

export function loadCollection(slug: string): Collection | null {
  const path = join(REPOSITORIES_DIR, slug, 'metadata', 'manifest.json');
  if (!existsSync(path)) {
    return null;
  }
  return { slug, manifest: JSON.parse(readFileSync(path, 'utf-8')) as RepositoryManifest };
}

export function loadSessionDocument(slug: string, session: ManifestSession): OutputDocument | null {
  // A reference entry has no document by contract — asking for one is a bug, not a miss.
  if (!isFull(session) || !session.file) {
    return null;
  }
  const path = join(REPOSITORIES_DIR, slug, session.file);
  return existsSync(path) ? parseDocument(readFileSync(path, 'utf-8')) : null;
}

export function findSession(collection: Collection, sessionSlug: string): ManifestSession | undefined {
  return collection.manifest.sessions.find((s) => s.slug === sessionSlug);
}

export function sessionsOf(collection: Collection, slugs: string[]): ManifestSession[] {
  const bySlug = new Map(collection.manifest.sessions.map((s) => [s.slug, s]));
  return slugs.map((slug) => bySlug.get(slug)).filter((s): s is ManifestSession => Boolean(s));
}

/** Chronological, matching timeline/index.md. Undated entries sort last rather than to 1970. */
export function chronological(sessions: ManifestSession[]): ManifestSession[] {
  return [...sessions].sort((a, b) => (dateOf(a) ?? '9999').localeCompare(dateOf(b) ?? '9999'));
}

export function dateOf(session: ManifestSession): string | undefined {
  return session.recorded_at ?? session.processed_at?.slice(0, 10);
}

/**
 * A deep link into the original video at a given timestamp — how a `reference` entry is cited.
 * Reference entries never link to an mdreel session page, because no document exists to link to.
 */
export function deepLink(session: ManifestSession, timestamp: string): string {
  const [h, m, s] = timestamp.split(':').map(Number);
  const seconds = h * 3600 + m * 60 + s;
  const separator = session.source.includes('?') ? '&' : '?';
  return `${session.source}${separator}t=${seconds}s`;
}

export function readmeOf(slug: string): string | null {
  const path = join(REPOSITORIES_DIR, slug, 'README.md');
  return existsSync(path) ? readFileSync(path, 'utf-8') : null;
}

export function sessionFiles(slug: string): string[] {
  const dir = join(REPOSITORIES_DIR, slug, 'sessions');
  return existsSync(dir) ? readdirSync(dir).sort() : [];
}

export interface CollectionCounts {
  full: number;
  reference: number;
  topics: number;
  speakers: number;
}

export function countsOf(collection: Collection): CollectionCounts {
  return {
    full: collection.manifest.sessions.filter(isFull).length,
    reference: collection.manifest.sessions.filter((s) => !isFull(s)).length,
    topics: collection.manifest.topics.length,
    speakers: collection.manifest.speakers.length,
  };
}
