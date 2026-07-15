export function BrandMark({ gradientId, size = 28 }: { gradientId: string; size?: number }) {
  return (
    <svg className="brand-mark" width={size} height={size} viewBox="0 0 32 32" fill="none" aria-hidden="true">
      <rect x="1.5" y="1.5" width="29" height="29" rx="8" stroke={`url(#${gradientId})`} strokeWidth="2" />
      <path
        d="M13 11.2v9.6a1 1 0 0 0 1.5.87l8.2-4.8a1 1 0 0 0 0-1.74l-8.2-4.8a1 1 0 0 0-1.5.87Z"
        fill={`url(#${gradientId})`}
      />
      <defs>
        <linearGradient id={gradientId} x1="2" y1="2" x2="30" y2="30" gradientUnits="userSpaceOnUse">
          <stop stopColor="#6366f1" />
          <stop offset="1" stopColor="#8b5cf6" />
        </linearGradient>
      </defs>
    </svg>
  );
}
