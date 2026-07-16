import type { Metadata } from 'next';
import Link from 'next/link';
import { categoryLabel, listLibrary } from '@/lib/mockLibrary';
import { LibraryTable } from '@/components/app/LibraryTable';

export const metadata: Metadata = { title: 'Library — mdreel' };

export default function AppLibraryPage() {
  const items = listLibrary();
  const categoryLabels = Object.fromEntries(items.map((i) => [i.category, categoryLabel(i.category)]));

  return (
    <>
      <div className="page-head">
        <div className="wrap">
          <div className="app-head-row">
            <div>
              <h1>Your library</h1>
              <p className="lead">Every recording you&apos;ve processed — view, download the Markdown, or erase it.</p>
            </div>
            <Link className="btn btn-primary" href="/app/upload">
              process a video
            </Link>
          </div>
        </div>
      </div>
      <div className="page-body">
        <div className="wrap">
          <LibraryTable items={items} categoryLabels={categoryLabels} />
        </div>
      </div>
    </>
  );
}
