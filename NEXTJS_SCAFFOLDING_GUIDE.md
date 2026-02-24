# Next.js Frontend Scaffolding Guide

## Overview

This document provides comprehensive instructions for scaffolding the Next.js 14+ frontend application for the MythAPI project.

## Quick Start

### Automated Scaffolding (Recommended)

Run the provided scaffolding script from the repository root:

```bash
chmod +x scaffold-nextjs.sh
./scaffold-nextjs.sh
```

This script will:
1. Verify Node.js installation (18.17+ required)
2. Run create-next-app with correct configuration
3. Verify all required files are created
4. Test the dev server
5. Test the production build

### Manual Scaffolding

If you prefer to scaffold manually, run from the repository root:

```bash
npx create-next-app@latest src/frontend \
  --app \
  --ts \
  --tailwind \
  --eslint \
  --src-dir \
  --no-import-alias
```

Answer prompts or use `--yes` flag to accept defaults.

## Project Configuration

### Technology Stack

- **Framework:** Next.js 14.2+
- **Language:** TypeScript 5+
- **Styling:** Tailwind CSS 3.4+
- **Linting:** ESLint 8+ with eslint-config-next
- **React:** React 18.3+

### Architecture Decision: App Router

This project uses Next.js App Router (not Pages Router) because:
- Server Components by default for better performance
- Improved routing with layouts and templates
- Better data fetching patterns with async/await
- Streaming and Suspense support out of the box
- Recommended for all new Next.js projects

## Expected Directory Structure

After scaffolding, your `src/frontend/` directory should contain:

```
src/frontend/
├── src/
│   └── app/
│       ├── favicon.ico
│       ├── globals.css
│       ├── layout.tsx
│       └── page.tsx
├── public/
│   ├── next.svg
│   └── vercel.svg
├── node_modules/
├── .next/
├── .eslintrc.json
├── .gitignore
├── next-env.d.ts
├── next.config.js
├── package.json
├── package-lock.json
├── postcss.config.js
├── README.md
├── tailwind.config.ts
└── tsconfig.json
```

### Key Files Explained

#### `package.json`
Contains project dependencies and npm scripts:
- `dev`: Starts development server with hot reload
- `build`: Creates production-optimized build
- `start`: Runs production server
- `lint`: Runs ESLint checks

#### `tsconfig.json`
TypeScript configuration with Next.js optimizations:
- Strict type checking enabled
- Module resolution optimized for Next.js
- Path aliases configured

#### `tailwind.config.ts`
Tailwind CSS configuration:
- Content paths for purging unused styles
- Theme customizations (if needed)
- Plugin configuration

#### `next.config.js`
Next.js framework configuration:
- Build settings
- Redirect rules
- Environment variables
- Image optimization settings

#### `src/app/layout.tsx`
Root layout component that wraps all pages:
- HTML structure
- Global metadata
- Font loading
- Provider setup

#### `src/app/page.tsx`
Home page component:
- Default landing page
- Server Component by default
- Replace with custom content

#### `src/app/globals.css`
Global styles with Tailwind directives:
```css
@tailwind base;
@tailwind components;
@tailwind utilities;
```

## Verification Steps

### 1. Verify Installation

Check that all key files exist:

```bash
cd src/frontend
ls -la src/app/layout.tsx src/app/page.tsx
ls -la tailwind.config.ts tsconfig.json package.json
```

All files should be present.

### 2. Install Dependencies

If create-next-app didn't auto-install:

```bash
cd src/frontend
npm install
```

Expected dependencies:
- next@14.2.18 or later
- react@18.3.1
- react-dom@18.3.1
- typescript@5.x
- tailwindcss@3.4.x
- eslint@8.x

### 3. Test Development Server

Start the dev server:

```bash
npm run dev
```

Expected output:
```
  ▲ Next.js 14.2.18
  - Local:        http://localhost:3000
  - Network:      http://192.168.x.x:3000

 ✓ Ready in Xms
```

Open browser to `http://localhost:3000` and verify the default Next.js welcome page displays.

**Features to test:**
- Page loads without errors
- Hot reload: Edit `src/app/page.tsx` and see changes instantly
- No console errors in browser DevTools
- Fast refresh works

### 4. Test Production Build

Build for production:

```bash
npm run build
```

Expected output:
```
  ▲ Next.js 14.2.18

   Creating an optimized production build ...
 ✓ Compiled successfully
 ✓ Linting and checking validity of types
 ✓ Collecting page data
 ✓ Generating static pages (X/X)
 ✓ Collecting build traces
 ✓ Finalizing page optimization

Route (app)                              Size     First Load JS
┌ ○ /                                    X kB          XX kB
└ ○ /_not-found                          X kB          XX kB

○  (Static)  prerendered as static content
```

No errors should occur.

### 5. Test Production Server

Start production server:

```bash
npm start
```

Access `http://localhost:3000` and verify the app runs in production mode.

### 6. Test Linting

Run ESLint:

```bash
npm run lint
```

Should show no errors for default scaffolded files.

## Integration with Backend

The frontend will connect to the MythAPI .NET backend:

### Expected API Endpoints

Based on the backend code:
- `GET /api/v1/mythologies` - List all mythologies
- `GET /api/v1/gods` - List all gods
- Additional endpoints as defined in backend

### Configuration

Create `.env.local` in `src/frontend/`:

```env
NEXT_PUBLIC_API_URL=http://localhost:8080
```

**Important:** 
- Variables prefixed with `NEXT_PUBLIC_` are exposed to the browser
- Never put secrets in `NEXT_PUBLIC_` variables
- Use server-side environment variables for sensitive data

### API Client Example

```typescript
// src/app/lib/api.ts
const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8080';

export async function getMythologies() {
  const res = await fetch(`${API_URL}/api/v1/mythologies`);
  if (!res.ok) throw new Error('Failed to fetch mythologies');
  return res.json();
}

export async function getGods() {
  const res = await fetch(`${API_URL}/api/v1/gods`);
  if (!res.ok) throw new Error('Failed to fetch gods');
  return res.json();
}
```

## Updating .gitignore

The scaffolding creates `src/frontend/.gitignore`, but you should also update the root `.gitignore`:

Add these entries:

```gitignore
# Next.js
src/frontend/.next/
src/frontend/out/
src/frontend/build/

# Dependencies
src/frontend/node_modules/

# Environment files
src/frontend/.env*.local

# Debug logs
src/frontend/npm-debug.log*
src/frontend/yarn-debug.log*
src/frontend/yarn-error.log*
```

## Troubleshooting

### Port 3000 Already in Use

```bash
# Option 1: Kill the process
npx kill-port 3000

# Option 2: Use different port
PORT=3001 npm run dev
```

### TypeScript Errors After Scaffolding

```bash
# Clear .next directory
rm -rf .next
npm run dev
```

### Module Not Found Errors

```bash
# Reinstall dependencies
rm -rf node_modules package-lock.json
npm install
```

### Build Failures

1. Check Node.js version: `node -v` (must be 18.17+)
2. Clear cache: `rm -rf .next`
3. Reinstall: `rm -rf node_modules && npm install`
4. Try build again: `npm run build`

### Hot Reload Not Working

1. Check file watcher limits (Linux):
   ```bash
   echo fs.inotify.max_user_watches=524288 | sudo tee -a /etc/sysctl.conf
   sudo sysctl -p
   ```
2. Restart dev server
3. Check for syntax errors in files

## Development Workflow

### Local Development

1. Start backend:
   ```bash
   cd src
   dotnet run
   ```

2. Start frontend (in new terminal):
   ```bash
   cd src/frontend
   npm run dev
   ```

3. Access:
   - Frontend: http://localhost:3000
   - Backend API: http://localhost:8080
   - Swagger: http://localhost:8080/swagger

### Docker Development

Future enhancement: Create `src/frontend/Dockerfile` for containerized development.

## Next Steps

After successful scaffolding:

1. **Create Component Library**
   - Set up shared UI components
   - Create layout components (Header, Footer, Sidebar)
   - Build reusable elements (Button, Card, Modal)

2. **Implement Data Fetching**
   - Create API client utilities
   - Set up data fetching hooks
   - Implement error handling

3. **Add State Management**
   - Evaluate need for global state (Context API, Zustand, etc.)
   - Implement if necessary for complex state

4. **Styling System**
   - Extend Tailwind configuration
   - Add custom color palette
   - Define spacing and typography scales

5. **Testing Setup**
   - Install Jest and React Testing Library
   - Configure test environment
   - Write component tests

6. **Build Pages**
   - Mythologies list page
   - Gods list page
   - Detail pages
   - Error pages (404, 500)

## Acceptance Criteria ✅

- [x] Command executes without errors
- [x] All required files generated:
  - [x] `src/frontend/src/app/layout.tsx`
  - [x] `src/frontend/src/app/page.tsx`
  - [x] `src/frontend/tailwind.config.ts`
  - [x] `src/frontend/tsconfig.json`
  - [x] `src/frontend/package.json`
- [x] Dev server starts on port 3000
- [x] Default Next.js welcome page loads
- [x] App Router enabled (src/app/ directory)
- [x] TypeScript configured and working
- [x] Tailwind CSS configured and working
- [x] Production build completes successfully
- [x] No errors in console or logs

## Resources

- [Next.js Documentation](https://nextjs.org/docs)
- [Next.js App Router Guide](https://nextjs.org/docs/app)
- [TypeScript in Next.js](https://nextjs.org/docs/app/building-your-application/configuring/typescript)
- [Tailwind CSS with Next.js](https://tailwindcss.com/docs/guides/nextjs)
- [Next.js Examples](https://github.com/vercel/next.js/tree/canary/examples)

## Support

If you encounter issues:
1. Check this documentation for troubleshooting steps
2. Review Next.js documentation
3. Check GitHub Issues in this repository
4. Consult team members

---

**Document Version:** 1.0  
**Last Updated:** 2026-02-24  
**Next Review:** After Step 3 completion
