# ADR-0014: Frontend Architecture (React 19 + Vite + TanStack Query + i18next + Nginx)

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-23 |
| Owner | Publink Audit frontend |
| Sources | `src/frontend/package.json`, `src/frontend/vite.config.ts`, `docker/Dockerfile.frontend`, `docker/nginx.frontend.conf` |
| Confidence | High |
| Related | [ADR-0001](adr-0001-docker-compose-runtime.md) |

## Status

Accepted.

## Date

2026-06-20.

## Context

The product requirement is a bilingual (Polish/English) SPA that lets audit operators browse active and archived contracts, inspect timelines and trigger ZIP exports. The frontend must communicate with the backend REST API and handle server state (paginated data, staleness, refetching) without introducing a global client-side state manager.

## Problem

A modern audit SPA requires decisions across four coupled concerns: build tooling, server-state management, internationalisation and production serving. Each decision affects the others (e.g. how i18n resources are bundled, how the API proxy is configured in dev vs prod, how the SPA is served in Docker).

## Considered Options

**Build tooling:** Create React App (deprecated), Next.js (SSR not needed for a read-only SPA with a separate API), Vite (fast HMR, standard ESM output, proxy support for local dev).

**Server-state management:** React Context + useEffect (manual fetching, no caching), SWR (similar to TanStack Query, smaller ecosystem), TanStack Query v5 (built-in caching, pagination helpers, query invalidation, TypeScript-first).

**Internationalisation:** Hardcoded strings (unacceptable given explicit bilingual requirement), FormatJS/react-intl (heavier, message extraction workflow), i18next + react-i18next (established, namespace-based, runtime language switch).

**Production serving:** Node.js serve (not suitable for static assets in production), Apache (legacy), Nginx (standard static SPA server; also acts as reverse proxy to the backend, eliminating CORS and aligning with the ADR-0001 Docker Compose runtime).

## Decision

- **Vite 8** as the build tool: fast dev server with `/api` and `/health` proxy to `query-api:8080`, TypeScript compilation via `tsc -b && vite build`.
- **React 19** as the UI framework.
- **TanStack Query v5** for all server state: active/archive search results, timeline pages, synchronisation status. No global client-state manager is present.
- **i18next 26 + react-i18next 17** for bilingual support. Polish and English namespace resources are bundled statically. Language is switched at runtime.
- **Nginx 1.29** (multi-stage Docker build from Node 24) as the production static server. `try_files $uri $uri/ /index.html` handles SPA client-side routing. `/api` and `/health` routes are proxied to the backend, co-locating security headers (CSP, X-Frame-Options, Referrer-Policy) with the SPA serving layer.

## Consequences

- No server-side rendering: the initial HTML is a shell; content requires JavaScript. This is acceptable for an internal audit tool.
- TanStack Query manages cache lifetime and refetch windows; stale data windows are configured at the query level.
- The Nginx proxy eliminates the need for CORS configuration on the API; changing the API port or hostname requires updating `nginx.frontend.conf`.
- Two sets of security headers exist (Nginx + `ResponseSecurityHeadersMiddleware` on the API): defence in depth, but they must be kept consistent.
- i18n namespace files are bundled at build time; adding a third language requires adding resource files and a rebuild.
- Vitest + Testing Library are used for unit and component tests; no end-to-end test suite is present: Assumption – requires validation.
