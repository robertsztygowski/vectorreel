# Next.js 15 standalone app on Cloud Run (listens on 8080).
# ⚠️ The project must NOT live at /app: with WORKDIR /app the App Router directory collides with
# the workdir and `next build` prerenders "/" with the /app route's page (verified 2026-07-17).
FROM node:20-alpine AS deps
WORKDIR /web
COPY package.json package-lock.json* ./
RUN npm ci

FROM node:20-alpine AS build
WORKDIR /web
COPY --from=deps /web/node_modules ./node_modules
COPY . .
# NEXT_PUBLIC_* is inlined into the client bundle at build time. The e2e profile sets it to the
# host-reachable API URL so the browser (running on the host) posts events/checkout to the real API.
ARG NEXT_PUBLIC_API_BASE=
ENV NEXT_PUBLIC_API_BASE=$NEXT_PUBLIC_API_BASE
RUN npm run build

FROM node:20-alpine AS runtime
WORKDIR /web
ENV NODE_ENV=production
ENV PORT=8080

# fixtures/ is read via fs at request time (lib/corpus.ts), not statically imported, so Next's
# output-file-tracing won't discover it automatically — copy it explicitly.
COPY --from=build /web/public ./public
COPY --from=build /web/fixtures ./fixtures
COPY --from=build /web/.next/standalone ./
COPY --from=build /web/.next/static ./.next/static

EXPOSE 8080
CMD ["node", "server.js"]
