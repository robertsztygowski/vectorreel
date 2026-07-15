'use client';

import Link from 'next/link';
import type { Plan } from '@/lib/pricing';
import { trackCheckoutClicked } from '@/lib/events';
import styles from './PricingCard.module.css';

export function PricingCard({ plan }: { plan: Plan }) {
  return (
    <div className={`card ${styles.card} ${plan.highlighted ? styles.highlighted : ''} ${plan.dark ? styles.dark : ''}`}>
      {plan.highlighted && <span className={styles.badge}>Most teams start here</span>}
      {plan.dark && <span className={styles.badgeDark}>Dark — fallback only</span>}
      <p className={styles.planName}>{plan.name}</p>
      <p className={styles.price}>
        €{plan.priceEur}
        <span>/mo</span>
      </p>
      <p className={styles.tagline}>{plan.tagline}</p>
      <ul className={styles.list}>
        {plan.features.map((f) => (
          <li key={f}>{f}</li>
        ))}
      </ul>
      <Link
        href={`/checkout?plan=${plan.id}`}
        className={`btn ${plan.highlighted ? 'btn-primary' : 'btn-ghost'} ${styles.cta}`}
        onClick={() => trackCheckoutClicked(plan.id)}
      >
        Choose {plan.name}
      </Link>
    </div>
  );
}
