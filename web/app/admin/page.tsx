'use client';

import { FormEvent, useEffect, useState } from 'react';
import Link from 'next/link';
import { getAdminOverview, postAdminAdSpend } from '@/lib/api';
import {
  formatCents,
  formatMinutes,
  sourceLabel,
  type AdminAdSpendInput,
  type AdminFunnelWindow,
  type AdminOverview,
} from '@/lib/admin';

const FUNNEL_STEPS: { key: keyof Omit<AdminFunnelWindow, 'window'>; label: string }[] = [
  { key: 'pageView', label: 'page_view' },
  { key: 'signupView', label: 'signup_view' },
  { key: 'signup', label: 'signup' },
  { key: 'uploadStarted', label: 'upload_started' },
  { key: 'jobCompleted', label: 'job_completed' },
  { key: 'checkoutClicked', label: 'checkout_clicked' },
  { key: 'paymentSucceeded', label: 'payment_succeeded' },
];

export default function AdminPage() {
  const [overview, setOverview] = useState<AdminOverview | 'not-authorized' | null>(null);
  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState<string | null>(null);

  async function load() {
    setLoading(true);
    setOverview(await getAdminOverview());
    setLoading(false);
  }

  useEffect(() => {
    void load();
  }, []);

  async function submitAdSpend(input: AdminAdSpendInput) {
    setMessage(null);
    const ok = await postAdminAdSpend(input);
    setMessage(ok ? 'ad spend recorded' : 'could not record ad spend');
    if (ok) await load();
  }

  if (loading) {
    return (
      <div className="app-page">
        <div className="wrap">
          <p className="micro">loading admin overview…</p>
        </div>
      </div>
    );
  }

  if (overview === 'not-authorized') {
    return (
      <div className="app-page">
        <div className="wrap">
          <h1 className="app-h1">Not authorized</h1>
        </div>
      </div>
    );
  }

  if (!overview) {
    return (
      <div className="app-page">
        <div className="wrap">
          <div className="error-panel" role="alert">
            <p className="headline">admin overview unavailable</p>
            <p>Try again after the API is reachable.</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="app-page">
      <div className="wrap">
        <div className="app-head-row">
          <div>
            <h1>Admin overview</h1>
            <p className="app-h1-sub">Is anyone using this, and where did they come from?</p>
          </div>
          <Link className="btn btn-ghost btn-sm" href="https://stats.mdreel.com">
            Umami stats
          </Link>
        </div>

        <section className="admin-section">
          <h2 className="eyebrow">## funnel</h2>
          <div className="data-table">
            <div className="data-head admin-funnel-row">
              <span>window</span>
              {FUNNEL_STEPS.map((step) => (
                <span key={step.key}>{step.label}</span>
              ))}
            </div>
            {overview.funnel.map((row) => (
              <div className="data-row admin-funnel-row" key={row.window}>
                <span className="cell-name">{row.window}</span>
                {FUNNEL_STEPS.map((step) => (
                  <span className="cell" key={step.key}>
                    {row[step.key]}
                  </span>
                ))}
              </div>
            ))}
          </div>
        </section>

        <section className="admin-grid">
          <div>
            <h2 className="eyebrow">## usage</h2>
            <div className="data-table">
              <div className="data-head admin-usage-row">
                <span>window</span>
                <span>videos</span>
                <span>minutes</span>
              </div>
              {[
                ['today', overview.usage.today],
                ['7d', overview.usage.sevenDays],
              ].map(([label, value]) => (
                <div className="data-row admin-usage-row" key={label as string}>
                  <span className="cell-name">{label as string}</span>
                  <span className="cell">{(value as typeof overview.usage.today).videosProcessed}</span>
                  <span className="cell">{formatMinutes((value as typeof overview.usage.today).videoMinutes)}</span>
                </div>
              ))}
            </div>
          </div>

          <div>
            <h2 className="eyebrow">## retention</h2>
            <div className="meta-table meta-table-wide admin-card">
              <span className="k">new_7d:</span>
              <span>{overview.retention.newLast7d}</span>
              <span className="k">new_30d:</span>
              <span>{overview.retention.newLast30d}</span>
              <span className="k">returning_this_week:</span>
              <span>{overview.retention.returningThisWeek}</span>
              <span className="k">inactive_30d:</span>
              <span>{overview.retention.inactive30d}</span>
            </div>
            <div className="data-table">
              <div className="data-head admin-week-row">
                <span>signup week</span>
                <span>tenants</span>
              </div>
              {overview.retention.signupWeeks.map((week) => (
                <div className="data-row admin-week-row" key={week.week}>
                  <span className="cell-name">{week.week}</span>
                  <span className="cell">{week.tenants}</span>
                </div>
              ))}
            </div>
          </div>
        </section>

        <section className="admin-section">
          <div className="app-head-row">
            <h2 className="eyebrow">## sources</h2>
            <AdSpendForm onSubmit={submitAdSpend} />
          </div>
          {message ? <p className="micro admin-message">{message}</p> : null}
          <div className="data-table">
            <div className="data-head admin-source-row">
              <span>source / medium / campaign</span>
              <span>tenants</span>
              <span>paying</span>
              <span>revenue</span>
              <span>spend</span>
              <span>CAC</span>
            </div>
            {overview.sources.map((source) => (
              <div className="data-row admin-source-row" key={sourceLabel(source)}>
                <span className="cell-name">{sourceLabel(source)}</span>
                <span className="cell">{source.tenantCount}</span>
                <span className="cell">{source.payingTenantCount}</span>
                <span className="cell">{formatCents(source.revenueCents)}</span>
                <span className="cell">{formatCents(source.adSpendCents)}</span>
                <span className="cell">{formatCents(source.cacCents)}</span>
              </div>
            ))}
          </div>
        </section>
      </div>
    </div>
  );
}

function AdSpendForm({ onSubmit }: { onSubmit: (input: AdminAdSpendInput) => Promise<void> }) {
  const [source, setSource] = useState('google_ads');
  const [campaign, setCampaign] = useState('launch');
  const [amount, setAmount] = useState('');
  const [spentOn, setSpentOn] = useState(() => new Date().toISOString().slice(0, 10));
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSubmitting(true);
    await onSubmit({
      source,
      campaign,
      amount_cents: Math.round(Number(amount) * 100),
      currency: 'EUR',
      spent_on: spentOn,
    });
    setSubmitting(false);
    setAmount('');
  }

  return (
    <form className="admin-ad-form" onSubmit={handleSubmit}>
      <input aria-label="ad source" value={source} onChange={(e) => setSource(e.target.value)} />
      <input aria-label="ad campaign" value={campaign} onChange={(e) => setCampaign(e.target.value)} />
      <input aria-label="ad amount eur" inputMode="decimal" placeholder="EUR" value={amount} onChange={(e) => setAmount(e.target.value)} />
      <input aria-label="spent on" type="date" value={spentOn} onChange={(e) => setSpentOn(e.target.value)} />
      <button className="btn btn-primary btn-sm" disabled={submitting || !amount} type="submit">
        add spend
      </button>
    </form>
  );
}
