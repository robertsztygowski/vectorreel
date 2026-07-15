import type { ReactNode } from 'react';
import { AppNav } from '@/components/app/AppNav';

export default function AppLayout({ children }: { children: ReactNode }) {
  return (
    <>
      <AppNav />
      {children}
    </>
  );
}
