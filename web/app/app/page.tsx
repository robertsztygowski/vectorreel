import type { Metadata } from 'next';
import Link from 'next/link';
import { categoryLabel, listLibrary } from '@/lib/mockLibrary';
import { LibraryTable } from '@/components/app/LibraryTable';

export const metadata: Metadata = { title: 'Library — mdreel' };

export default function AppLibraryPage() {
  const items = listLibrary();
  const categoryLabels = Object.fromEntries(items.map((i) => [i.category, categoryLabel(i.category)]));

  return (
    <div className="app-page">
      <div className="wrap">
        <div className="app-head-row">
          <h1>Library</h1>
          <Link className="btn btn-primary btn-sm" href="/app/upload">
            process a video
          </Link>
        </div>
        <LibraryTable items={items} categoryLabels={categoryLabels} />
      </div>
    </div>
  );
}
