import Link from 'next/link';

export default function GalleryVideoNotFound() {
  return (
    <div className="page-body">
      <div className="wrap page-narrow">
        <h1>Video not found</h1>
        <p className="lead">
          We don&apos;t have a mocked result for that video. <Link href="/gallery">Back to the gallery</Link>.
        </p>
      </div>
    </div>
  );
}
