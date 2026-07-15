'use client';

import Link from 'next/link';
import type { Plan } from '@/lib/pricing';
import { trackCheckoutClicked } from '@/lib/events';
import styles from './PricingCard.module.css';

export function PricingCard({ plan }: { plan: Plan }) {
  const badge = plan.highlighted ? 'most teams' : plan.dark ? 'dark — fallback' : null;
  return (
    <div className={`${styles.card} ${plan.highlighted ? styles.highlighted : ''} ${plan.dark ? styles.dark : ''}`}>
      <div className={styles.head}>
        <span className={styles.planName}>{plan.name.toLowerCase()}</span>
        {badge && <span className={styles.badge}>{badge}</span>}
      </div>
      <div className={styles.priceRow}>
        <span className={styles.price}>€{plan.priceEur}</span>
        <span className={styles.per}>/mo</span>
      </div>
      <p className={styles.hours}>{plan.hoursPerMonth} h of video / month</p>
      <p className={styles.tagline}>{plan.tagline}</p>
      <ul className={styles.list}>
        {plan.features.map((f) => (
          <li key={f}>
            <span>{f}</span>
          </li>
        ))}
      </ul>
      <Link
        href={`/checkout?plan=${plan.id}`}
        className={`${styles.cta} ${plan.highlighted ? styles.ctaPrimary : ''}`}
        onClick={() => trackCheckoutClicked(plan.id)}
      >
        {plan.highlighted ? `start with ${plan.name.toLowerCase()}` : `choose ${plan.name.toLowerCase()}`}
      </Link>
    </div>
  );
}
