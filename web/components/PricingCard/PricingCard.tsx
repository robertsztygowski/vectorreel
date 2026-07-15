'use client';

import Link from 'next/link';
import { PRICING } from '@/lib/pricing';
import { trackCheckoutClicked } from '@/lib/events';
import styles from './PricingCard.module.css';

export function PricingCard() {
  return (
    <div className={`card ${styles.card}`}>
      <p className={styles.planName}>{PRICING.planName}</p>
      <p className={styles.price}>
        €{PRICING.priceEur}
        <span>/mo</span>
      </p>
      <ul className={styles.list}>
        <li>{PRICING.hoursPerMonth} hours of video processed per month</li>
        <li>€{PRICING.overagePerHourEur}/hour overage</li>
        <li>UI + API access, webhooks</li>
        <li>EU-only processing, source deleted after processing</li>
      </ul>
      <Link href="/checkout" className={`btn btn-primary ${styles.cta}`} onClick={() => trackCheckoutClicked()}>
        Get started
      </Link>
    </div>
  );
}
