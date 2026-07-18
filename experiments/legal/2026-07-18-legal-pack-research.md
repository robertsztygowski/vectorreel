# Legal-pack research memo — Polish JDG B2B SaaS legal pages

> ⚠️ **STATUS: point-in-time memo, 2026-07-18. NEVER AUTHORITATIVE** (CLAUDE.md — `experiments/**`).
> True on the date written; frozen afterwards. If this ever contradicts a living doc, the living
> doc wins. The load-bearing output of this memo is the six published pages under `web/app/legal/`
> and the NEEDS-FOUNDER items filed in PLAN.md STATUS (Polish lawyer review first). **This is a
> reasoning trail written by an AI research pass, not legal advice.**

Grounds the legal pack shipped this run for **Royalcode Robert Sztygowski** (JDG, NIP `PL 8222319203`,
ul. Józefa Wybickiego 2/9, 05-820 Piastów, Poland), a strictly-B2B EU SaaS. Full source list at the
bottom. Every load-bearing legal fact below carries a primary-source citation.

---

## 1. Polish imprint + regulamin (Act on Providing Services by Electronic Means — UŚUDE, 18 Jul 2002)

**Imprint — Art. 5 UŚUDE** (transposing E-Commerce Directive 2000/31/EC Art. 5): the provider must
disclose identifying data "clearly, unambiguously and directly accessible" — a permanent page, not a
buried PDF. For a **JDG** (natural person), Art. 5(2)(1) requires **full name + residence/business
address**; Art. 5(3) UŚUDE (via Prawo Przedsiębiorców Art. 20(3)) requires the **NIP** on all
commercial online material; E-Commerce Directive Art. 5(1)(c) requires a **contact email** (a form
alone is insufficient). A JDG has **no KRS number** — it is registered in **CEIDG**, which has no
discrete registration number; **NIP is the functional public identifier.** REGON display is *not*
clearly mandated → NEEDS-FOUNDER (lawyer confirm).

**Regulamin (ToS) — Art. 8 UŚUDE** is *mandatory* and must be made available **before** contract
conclusion, in a form the recipient can save/reproduce (Art. 8(1)). Non-disclosed clauses do **not
bind** the recipient (Art. 8(2)). Art. 8(3) mandatory contents: **(1)** types & scope of services;
**(2)** conditions of provision incl. (a) technical requirements and (b) **prohibition on the
recipient providing illegal content**; **(3)** conditions of concluding & terminating contracts;
**(4)** complaint procedure (*tryb reklamacyjny*). List is non-exhaustive ("w szczególności") — add
B2B payment/liability/IP/amendment terms.

## 2. B2B-only under Polish law

A provider may lawfully restrict the service to **entrepreneurs** and exclude the consumer apparatus
(14-day withdrawal under Consumer Rights Act Art. 27, ODR, abusive-clause review under KC
Art. 385¹–385³) — those apply only to a **konsument** (KC Art. 22¹, natural person outside their
trade). **Risk: the "przedsiębiorca na prawach konsumenta" rule (KC Art. 385⁵, since 2021-01-01):**
a sole trader whose purchase lacks *professional character* (judged by their CEIDG activity) gets
abusive-clause + warranty protection — but **not** the withdrawal right. A business AI/knowledge-base
tool is almost always professional in character for the buyer, so the residual risk is low.

**Drafting recommendations (research):** (1) explicit B2B-only clause naming Prawo Przedsiębiorców
Art. 4(1) / excluding KC Art. 22¹ consumers; (2) collect VAT/NIP at checkout as evidence of business
status; (3) research **favours an affirmative checkbox** over a passive notice for the business-
capacity declaration (clearer Art. 385⁵ §11 record) — **flagged lawyer-confirm**. Liability cap:
KC Art. 473 permits contractual limitation **except for wilful damage** (Art. 473 §2) — a cap at
fees paid in the prior 12 months + exclusion of indirect damages is enforceable B2B.
Unilateral amendment: KC Art. 384¹ — give **≥14 days' notice**, state amendment grounds, allow
penalty-free termination; valid B2B if clearly drafted.

> **Decision this run:** ship a **prominent B2B-only clause + a passive business-capacity notice** at
> signup/checkout (founder's stated default: "notice, not checkbox"). The research recommends
> upgrading to an *affirmative checkbox* — filed as a NEEDS-FOUNDER item for the Polish lawyer to
> confirm, so it can be turned on without re-litigating. Not made a hard blocker because it is
> lawyer-gated and changes the signup funnel.

## 3. GDPR privacy policy (Art. 13/14)

Art. 13 (data from the subject) mandatory contents: controller identity + contact (13(1)(a)); DPO
if applicable (13(1)(b) — **mdreel needs none**, no large-scale special-category processing, Art. 37);
**purposes + Art. 6 legal basis** (13(1)(c)); legitimate interests if 6(1)(f) (13(1)(d)); recipients
/ categories (13(1)(e)); transfer disclosures (13(1)(f)); **retention** (13(2)(a)); **rights
Arts. 15–22** (13(2)(b)); withdraw-consent where consent-based (13(2)(c)); **right to complain to a
supervisory authority** (13(2)(d)); statutory/contractual necessity (13(2)(e)); automated
decision-making (13(2)(f)). Art. 14 (data *not* from the subject — e.g. people appearing in uploaded
video) adds **categories of data** (14(1)(d)) and **source** (14(2)(f)); the **disproportionate-
effort exemption (Art. 14(5)(b))** covers mdreel's inability to notify every individual in a
recording → satisfy it by making the info public in the policy.

**Polish supervisory authority: Prezes Urzędu Ochrony Danych Osobowych (UODO)**, ul. Stawki 2,
00-193 Warszawa, https://uodo.gov.pl.

**Controller/processor split (confirmed correct):** mdreel is **processor** for uploaded
video/audio + generated outputs (customer is controller — governed by the DPA); **controller** for
account, billing/invoicing (6(1)(b)+(c)) and self-hosted analytics/security (6(1)(f)). **All
processing intra-EEA** (Google Cloud EU regions, Stripe Payments Europe Ltd/IE, Brevo SAS/FR) ⇒
**no Chapter V/SCC mechanism** needed in mdreel's own disclosures. **Umami cookieless self-hosted**
⇒ no cookie-consent banner required; disclose it anyway as transparency.

## 4. GDPR Art. 28 DPA mandatory clauses

Preamble: subject-matter, duration, nature & purpose, data types, categories of data subjects,
controller obligations/rights. Then **28(3)(a)** documented instructions only; **(b)** confidentiality
of authorised persons; **(c)** Art. 32 security; **(d)** sub-processor conditions per 28(2)/(4);
**(e)** assist with data-subject rights (Ch. III); **(f)** assist with Arts. 32–36 (security, 72-h
breach notice, DPIA, prior consultation); **(g)** delete/return data at end; **(h)** make available
compliance info + allow audits, and **immediately inform** the controller if an instruction infringes.
**28(2)/(4):** written authorisation for sub-processors + **flow-down** of the same obligations +
processor stays fully liable. **28(9):** written **incl. electronic** form — **click-through DPA
incorporated by reference into the ToS is standard and sufficient** (Figma/Atlassian/GitHub pattern).
Intra-EEA ⇒ state no non-EEA transfer occurs by service design.

## 5. Reference EU B2B SaaS legal centres

- **Figma** `/legal/…`: Terms (master, incorporates the rest by reference), Privacy (controller vs
  processor split), **click-through DPA**, live **Subprocessors** page (date-stamped, columns
  service/activity/**location**/data categories, email-subscribe, **15-day** change notice + objection).
- **Atlassian**: Privacy explicitly carves out processor role → "refer to the DPA"; DPA + subprocessor
  pages with email subscription for change notifications.
- **GitHub**: subprocessor list versioned in git, "Last updated" dated page.
- **German TMG §5 Impressum** (same E-Commerce-Directive origin as UŚUDE Art. 5): standalone,
  permanently reachable imprint page — the model mdreel follows at `/legal/imprint`.

Adopted conventions: six permanent `/legal/*` pages; DPA incorporated by reference; subprocessor page
date-stamped with a **30-day** change-notice + objection right and a subscribe-by-email (`hello@`)
mechanism; every subprocessor row states its **EU location**.

## 6. Acceptable Use Policy norms (media/AI SaaS)

Standard prohibitions: **illegal content** (CSAM, violence/terror, defamation/harassment); **IP** —
customer **warrants they hold all rights/consents** to uploaded content, no model-training on outputs,
no reverse engineering; **privacy/surveillance/biometrics** — no recordings made without required
consent, no stalkerware, no Art. 9 special-category data without a lawful basis, no non-consensual
deepfakes; **platform integrity** — no bypassing rate limits/security, no scraping, no resale;
**sanctions/export** compliance. Tie into the ToS by reference; breach = material breach → tiered
enforcement (immediate suspension for serious/illegal/sanctions; notice + cure for material/technical);
customer **indemnifies** mdreel for third-party claims from their content; abuse-reporting contact.
(mdreel uses the single `hello@mdreel.com` for abuse reports — no separate alias.)

---

## NEEDS-FOUNDER (legal) — filed to PLAN.md STATUS

1. **Polish lawyer review of the entire pack** (link the shipping commit) — first priority.
2. **`hello@mdreel.com` must actually receive mail** — it is the imprint/privacy/DPO-ish/abuse
   contact on every page; verify the mailbox delivers.
3. **Affirmative B2B checkbox** at signup/checkout (research recommends over the passive notice;
   KC Art. 385⁵ §11) — lawyer to confirm sufficiency, then flip on.
4. **REGON** on the imprint — confirm whether Polish practice requires it alongside NIP.
5. **Art. 38a / sole-trader withdrawal** current status; **KC Art. 385⁵ checkbox** sufficiency;
   **DPO** not-required confirmation; **Google Cloud Standard DPA** SCC wording review.

## Primary sources

- GDPR — Regulation (EU) 2016/679, CELEX:32016R0679 — https://eur-lex.europa.eu/legal-content/EN/TXT/HTML/?uri=CELEX:32016R0679
- E-Commerce Directive 2000/31/EC, CELEX:32000L0031 — https://eur-lex.europa.eu/legal-content/EN/TXT/?uri=CELEX:32000L0031
- UŚUDE (18 Jul 2002, Dz.U. 2002 nr 144 poz. 1204) — https://isap.sejm.gov.pl/isap.nsf/DocDetails.xsp?id=WDU20021441204 ; Art. 5 / Art. 8 — https://lexlege.pl/ustawa-o-swiadczeniu-uslug-droga-elektroniczna/art-5/ , /art-8/
- Kodeks cywilny Art. 22¹, 385¹–385⁵, 384¹, 473 — https://lexlege.pl/kc/art-385-5/
- Ustawa o prawach konsumenta (30 May 2014) Art. 27 — https://lexlege.pl/ustawa-o-prawach-konsumenta/art-27/
- Prawo Przedsiębiorców (6 Mar 2018) Art. 4/20 — Dz.U. 2018 poz. 646
- UODO — https://uodo.gov.pl
- Reference legal centres: figma.com/legal, atlassian.com/legal, docs.github.com/site-policy; TMG §5 (DE)
