import { Suspense, type ReactNode } from 'react';
import { AppNav } from '@/components/app/AppNav';

export default function AppLayout({ children }: { children: ReactNode }) {
  return (
    <>
      {/* AppNav reads search params (usage-state preview); Suspense keeps /app prerenderable. */}
      <Suspense fallback={null}>
        <AppNav />
      </Suspense>
      {children}
    </>
  );
}
