'use client';

import { useEffect } from 'react';
import { assignAbArmIfAbsent, captureFirstTouchIfAbsent } from '@/lib/attribution';

// Mounted once in the root layout so first-touch UTM + ab_arm are captured as early as possible
// site-wide, even on pages that don't render Hero (which also self-assigns, idempotently).
export function AttributionInit() {
  useEffect(() => {
    captureFirstTouchIfAbsent();
    assignAbArmIfAbsent();
  }, []);
  return null;
}
