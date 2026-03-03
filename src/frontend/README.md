# Frontend Directory

This directory is prepared for a Next.js frontend application.

## Status

⚠️ **The Next.js application has not been created yet.**

According to the project workflow, Steps 1-9 should create the Next.js frontend application in this directory before Step 10 (verify .gitignore and final checks) can be completed.

## Expected Structure

Once the Next.js application is created (using `create-next-app`), this directory should contain:

- `package.json` - Node.js dependencies
- `next.config.js` or `next.config.mjs` - Next.js configuration
- `tsconfig.json` - TypeScript configuration
- `app/` or `pages/` - Next.js routing directory
- `components/` - React components
- `public/` - Static assets

## Getting Started

To create the Next.js application:

```bash
cd src/frontend
npx create-next-app@latest . --typescript --tailwind --app --no-src-dir
```

## Environment Variables

Create a `.env.local` file with:

```
NEXT_PUBLIC_API_URL=http://localhost:8080/api/v1
```

## Development

```bash
npm run dev
```

## Production Build

```bash
npm run build
npm start
```

## .gitignore

The `.gitignore` file in this directory is already configured to exclude:
- `node_modules/` - Dependencies
- `.next/` - Next.js build output
- `out/` - Static export output
- `.env*.local` - Local environment variables
