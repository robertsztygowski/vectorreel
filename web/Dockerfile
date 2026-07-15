# Next.js 15 standalone app on Cloud Run (listens on 8080).
FROM node:20-alpine AS deps
WORKDIR /app
COPY package.json package-lock.json* ./
RUN npm ci

FROM node:20-alpine AS build
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .
RUN npm run build

FROM node:20-alpine AS runtime
WORKDIR /app
ENV NODE_ENV=production
ENV PORT=8080

# fixtures/ is read via fs at request time (lib/corpus.ts), not statically imported, so Next's
# output-file-tracing won't discover it automatically — copy it explicitly.
COPY --from=build /app/public ./public
COPY --from=build /app/fixtures ./fixtures
COPY --from=build /app/.next/standalone ./
COPY --from=build /app/.next/static ./.next/static

EXPOSE 8080
CMD ["node", "server.js"]
