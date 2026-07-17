/** @type {import('next').NextConfig} */
const nextConfig = {
  output: 'standalone',
  // The same-origin auth proxy lives in middleware.ts (runtime rewrite), not here: next.config
  // `rewrites()` is evaluated at build time and cannot read the runtime API_ORIGIN env var.
};

export default nextConfig;

