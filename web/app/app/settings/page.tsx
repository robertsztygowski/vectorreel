'use client';

import { Suspense, useState } from 'react';
import Link from 'next/link';
import { useSearchParams } from 'next/navigation';
import { requestBillingPortal } from '@/lib/billing';
import { getTenantId } from '@/lib/session';

type SettingsScreen = 'keys' | 'webhooks' | 'usage' | 'account';

function tabHref(screen: SettingsScreen, usage: string) {
  return `/app/settings?screen=${screen}&usage=${usage}`;
}

// useSearchParams() forces a CSR bailout during prerender; the Suspense wrapper is required
// for `next build` — same pattern as checkout/page.tsx.
export default function AppSettingsPage() {
  return (
    <Suspense fallback={null}>
      <SettingsInner />
    </Suspense>
  );
}

function SettingsInner() {
  const searchParams = useSearchParams();
  const usage = searchParams.get('usage') ?? 'plan';
  const screen = (searchParams.get('screen') as SettingsScreen) ?? 'keys';
  const [keyRevealed, setKeyRevealed] = useState(false);
  const [confirmErase, setConfirmErase] = useState(false);
  const [billingNote, setBillingNote] = useState<string | null>(null);
  const [billingBusy, setBillingBusy] = useState(false);

  async function openBillingPortal() {
    setBillingNote(null);
    setBillingBusy(true);
    try {
      const portal = await requestBillingPortal(getTenantId() ?? '');
      if (portal?.url) {
        window.location.assign(portal.url);
        return;
      }
      setBillingNote('The billing portal is not available yet — contact support to manage your plan.');
    } finally {
      setBillingBusy(false);
    }
  }

  return (
    <div className="app-page">
      <div className="wrap">
        <h1 className="app-h1">Settings</h1>

        <div className="settings-tabs">
          <Link href={tabHref('keys', usage)} className={screen === 'keys' ? 'active' : undefined}>
            api keys
          </Link>
          <Link href={tabHref('webhooks', usage)} className={screen === 'webhooks' ? 'active' : undefined}>
            webhooks
          </Link>
          <Link href={tabHref('usage', usage)} className={screen === 'usage' ? 'active' : undefined}>
            usage &amp; billing
          </Link>
          <Link href={tabHref('account', usage)} className={screen === 'account' ? 'active' : undefined}>
            account
          </Link>
        </div>

        {screen === 'keys' && (
          <>
            <p className="micro" style={{ marginBottom: 20 }}>
              keys authenticate the REST API and the MCP server — <Link href="/docs#mcp">docs →</Link>
            </p>

            {keyRevealed && (
              <div className="key-reveal">
                <span>
                  <span className="secret">sk_live_placeholder_9f2ab4c8d1e7f3a2b5c9d4e8f1a6b3c7</span>
                  <span className="note">copy it now — we store only a hash; this is the only time you will see it</span>
                </span>
                <span className="actions">
                  <button className="btn-copy" type="button">
                    copy key
                  </button>
                  <button className="btn btn-ghost btn-sm" type="button" onClick={() => setKeyRevealed(false)}>
                    dismiss
                  </button>
                </span>
              </div>
            )}

            <div className="data-table">
              <div className="data-head keys-row">
                <span>name</span>
                <span>key</span>
                <span>created</span>
                <span>last used</span>
                <span style={{ textAlign: 'right' }}>actions</span>
              </div>
              {[
                { name: 'ci pipeline', key: 'sk_live_placeholder_9f2a..........k21', created: '2026-07-02', lastUsed: '2026-07-15' },
                { name: 'local dev', key: 'sk_live_placeholder_c41b..........m88', created: '2026-06-18', lastUsed: '2026-07-14' },
              ].map((row) => (
                <div className="data-row keys-row" key={row.name}>
                  <span className="cell-name">{row.name}</span>
                  <span className="cell">{row.key}</span>
                  <span className="cell">{row.created}</span>
                  <span className="cell">{row.lastUsed}</span>
                  <span className="data-actions">
                    <a className="act-revoke" href="#">
                      revoke
                    </a>
                  </span>
                </div>
              ))}
            </div>

            <div style={{ marginTop: 18 }}>
              <button className="btn btn-primary btn-sm" type="button" onClick={() => setKeyRevealed(true)}>
                create key
              </button>
            </div>
            <p className="micro" style={{ marginTop: 16 }}>
              keys are POST /keys · revoking takes effect immediately — <Link href="/docs">do this via API →</Link>
            </p>
          </>
        )}

        {screen === 'webhooks' && (
          <>
            <p className="micro" style={{ marginBottom: 20 }}>
              we POST job.completed to your endpoint when a document is ready — <Link href="/docs#webhooks">docs →</Link>
            </p>
            <div className="data-table">
              <div className="data-head hook-row">
                <span>url</span>
                <span>events</span>
                <span>secret</span>
                <span>status</span>
                <span style={{ textAlign: 'right' }}>actions</span>
              </div>
              {[
                {
                  url: 'https://ci.acme.eu/hooks/mdreel',
                  events: 'job.completed',
                  secret: 'whsec_......4f2',
                  status: 'ok',
                  failing: false,
                },
                {
                  url: 'https://staging.acme.eu/hooks/mdreel',
                  events: 'job.completed',
                  secret: 'whsec_......9c7',
                  status: 'failing',
                  failing: true,
                },
              ].map((row) => (
                <div className="data-row hook-row" key={row.url}>
                  <span style={{ display: 'flex', flexDirection: 'column', gap: 4 }}>
                    <span className="cell-name">{row.url}</span>
                    {row.failing && <span className="hook-fail">last delivery 503 — retrying with backoff</span>}
                  </span>
                  <span className="cell">{row.events}</span>
                  <span className="cell">{row.secret}</span>
                  <span>
                    <span className={`badge ${row.failing ? 'badge-failed' : 'badge-done'}`}>{row.status}</span>
                  </span>
                  <span className="data-actions">
                    <a className="act-test" href="#">
                      send test event
                    </a>
                    <a className="act-remove" href="#">
                      remove
                    </a>
                  </span>
                </div>
              ))}
            </div>
            <div style={{ marginTop: 18 }}>
              <button className="btn btn-ghost btn-sm" type="button">
                add endpoint
              </button>
            </div>
            <p className="micro" style={{ marginTop: 16 }}>
              verify X-Mdreel-Signature (HMAC-SHA256 over the raw body) before trusting a payload —{' '}
              <Link href="/docs#webhooks">docs →</Link>
            </p>
          </>
        )}

        {screen === 'usage' && (
          <>
            <div className="usage-card">
              <p className="eyebrow"># current period</p>
              <div className="meter-line">
                <span className="usage-meter-lg">
                  <i style={{ width: usage === 'trial' ? '20%' : usage === 'at-cap' ? '100%' : '70%' }} />
                </span>
                <span className="meter-caption">
                  {usage === 'trial'
                    ? '0.2 / 1.0 h used · trial · resets 2026-08-01'
                    : usage === 'at-cap'
                      ? '25 / 25 h used · pro · resets 2026-08-01'
                      : '17.4 / 25 h used · pro · resets 2026-08-01'}
                </span>
              </div>
              <p className="cap-note">Hard cap: processing pauses at the limit. No overage will be billed.</p>
              <div style={{ display: 'flex', gap: 12, flexWrap: 'wrap', alignItems: 'center' }}>
                <Link className="btn btn-ghost btn-sm" href="/pricing">
                  change plan
                </Link>
                <button className="btn btn-ghost btn-sm" type="button" disabled={billingBusy} onClick={() => void openBillingPortal()}>
                  manage billing
                </button>
              </div>
              {billingNote && (
                <p className="micro" style={{ marginTop: 12 }}>
                  {billingNote}
                </p>
              )}
            </div>

            <div className="data-table">
              <div className="data-head ledger-row">
                <span>date</span>
                <span>document</span>
                <span>duration</span>
                <span style={{ textAlign: 'right' }}>counted</span>
              </div>
              {[
                { date: '2026-07-15', doc: 'demo-billing.md', dur: '47:12', counted: '0.79 h' },
                { date: '2026-07-14', doc: 'onboarding-week1.md', dur: '1:22:40', counted: '1.38 h' },
                { date: '2026-07-12', doc: 'incident-runbook-walkthrough.md', dur: '18:05', counted: '0.30 h' },
                { date: '2026-07-11', doc: 'kubecon-keynote.md', dur: '52:30', counted: '0.88 h' },
                { date: '2026-07-09', doc: 'api-gateway-migration.md', dur: '1:04:18', counted: '1.07 h' },
              ].map((row) => (
                <div className="data-row ledger-row" key={`${row.date}-${row.doc}`}>
                  <span className="cell">{row.date}</span>
                  <span className="cell-name">{row.doc}</span>
                  <span className="cell">{row.dur}</span>
                  <span className="cell counted">{row.counted}</span>
                </div>
              ))}
            </div>
            <p className="micro" style={{ marginTop: 16 }}>
              this table is GET /usage — <Link href="/docs">do this via API →</Link>
            </p>
          </>
        )}

        {screen === 'account' && (
          <>
            <div className="meta-table meta-table-wide" style={{ maxWidth: 640 }}>
              <span className="k">email</span>
              <span>jonas@acme.eu</span>
              <span className="k">plan</span>
              <span>pro — €149/mo</span>
              <span className="k">default retention</span>
              <span>0 days — source deleted after processing (default)</span>
              <span className="k">compliance</span>
              <span>
                <a href="#">DPA</a> · <a href="#">subprocessor list</a> · <a href="#">data-flow diagram</a>
              </span>
            </div>
            <p className="micro" style={{ marginTop: 16 }}>
              these fields are GET /account — <Link href="/docs">do this via API →</Link>
            </p>

            <div className="error-panel" style={{ marginTop: 40 }}>
              <p className="headline">erase everything</p>
              <p style={{ margin: '0 0 18px', fontSize: 15, color: 'var(--ink-soft)' }}>
                Deletes every document, all metadata and all API keys. This runs the same audit-logged erasure as
                DELETE /jobs/{'{id}'} — for the whole account.
              </p>
              {!confirmErase ? (
                <button className="btn btn-danger-ghost btn-sm" type="button" onClick={() => setConfirmErase(true)}>
                  erase account data
                </button>
              ) : (
                <div className="confirm-inline">
                  <p>
                    <b>erase this whole account permanently?</b> Audit-logged, nothing retained.
                  </p>
                  <span style={{ display: 'flex', gap: 10 }}>
                    <button className="btn btn-ghost btn-sm" type="button" onClick={() => setConfirmErase(false)}>
                      cancel
                    </button>
                    <button className="btn btn-danger btn-sm" type="button" onClick={() => setConfirmErase(false)}>
                      erase — audit-logged
                    </button>
                  </span>
                </div>
              )}
            </div>
          </>
        )}
      </div>
    </div>
  );
}
