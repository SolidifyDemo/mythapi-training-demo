# Next.js Scaffolding Verification Checklist

## Purpose
Use this checklist to verify that the Next.js 14+ project has been successfully scaffolded in `src/frontend/`.

## Pre-Execution Checklist

Before running the scaffolding script:
- [ ] Node.js 18.17+ is installed (`node --version`)
- [ ] npm is available (`npm --version`)
- [ ] Internet connection is available
- [ ] You are in the repository root directory
- [ ] The `scaffold-nextjs.sh` script is executable (`chmod +x scaffold-nextjs.sh`)

## Post-Scaffolding File Verification

After running the scaffolding, verify these files exist:

### Configuration Files
- [ ] `src/frontend/package.json` - Project dependencies and scripts
- [ ] `src/frontend/tsconfig.json` - TypeScript configuration
- [ ] `src/frontend/next.config.js` - Next.js framework configuration
- [ ] `src/frontend/tailwind.config.ts` - Tailwind CSS configuration
- [ ] `src/frontend/postcss.config.js` - PostCSS configuration for Tailwind
- [ ] `src/frontend/.eslintrc.json` - ESLint configuration
- [ ] `src/frontend/.gitignore` - Git ignore rules for frontend
- [ ] `src/frontend/next-env.d.ts` - Next.js TypeScript declarations

### Application Files
- [ ] `src/frontend/src/app/layout.tsx` - Root layout component
- [ ] `src/frontend/src/app/page.tsx` - Home page component
- [ ] `src/frontend/src/app/globals.css` - Global styles with Tailwind directives
- [ ] `src/frontend/src/app/favicon.ico` - Site favicon

### Build Artifacts (after npm install)
- [ ] `src/frontend/node_modules/` - Dependencies directory exists
- [ ] `src/frontend/package-lock.json` - Locked dependency versions

### Public Assets
- [ ] `src/frontend/public/` - Public assets directory exists

## Package.json Verification

Open `src/frontend/package.json` and verify:

```json
{
  "scripts": {
    "dev": "next dev",        // ✓ Dev script exists
    "build": "next build",    // ✓ Build script exists
    "start": "next start",    // ✓ Start script exists
    "lint": "next lint"       // ✓ Lint script exists
  },
  "dependencies": {
    "next": "^14.x.x",        // ✓ Next.js 14+
    "react": "^18.x.x",       // ✓ React 18+
    "react-dom": "^18.x.x"    // ✓ React DOM 18+
  },
  "devDependencies": {
    "typescript": "^5.x.x",   // ✓ TypeScript 5+
    "tailwindcss": "^3.x.x",  // ✓ Tailwind CSS 3+
    "eslint": "^8.x.x",       // ✓ ESLint 8+
    "@types/node": "^20",     // ✓ Node types
    "@types/react": "^18",    // ✓ React types
    "@types/react-dom": "^18" // ✓ React DOM types
  }
}
```

## TypeScript Configuration Verification

Open `src/frontend/tsconfig.json` and verify key settings:

- [ ] `"jsx": "preserve"` - JSX is preserved for Next.js
- [ ] `"plugins": [{"name": "next"}]` - Next.js TypeScript plugin
- [ ] `"paths": {"@/*": ["./src/*"]}` - Path aliases configured
- [ ] `"strict": true` - Strict mode enabled
- [ ] Include paths contain `"**/*.ts"`, `"**/*.tsx"`, `".next/types/**/*.ts"`

## Tailwind Configuration Verification

Open `src/frontend/tailwind.config.ts` and verify:

- [ ] Content paths include: `'./src/**/*.{js,ts,jsx,tsx,mdx}'`
- [ ] Config exports TypeScript type: `import type { Config } from 'tailwindcss'`

Check `src/frontend/src/app/globals.css`:

- [ ] Contains `@tailwind base;`
- [ ] Contains `@tailwind components;`
- [ ] Contains `@tailwind utilities;`

## Functional Testing

### 1. Development Server Test

```bash
cd src/frontend
npm run dev
```

**Verify:**
- [ ] Server starts without errors
- [ ] Output shows: `Ready in Xms`
- [ ] Output shows: `Local: http://localhost:3000`
- [ ] No TypeScript compilation errors
- [ ] No ESLint errors

**Browser Test:**
- [ ] Open http://localhost:3000
- [ ] Default Next.js welcome page displays
- [ ] No console errors in browser DevTools
- [ ] Page title shows "Create Next App" or similar

**Hot Reload Test:**
- [ ] Edit `src/app/page.tsx`
- [ ] Save the file
- [ ] Browser automatically refreshes
- [ ] Changes appear without manual reload

### 2. Production Build Test

```bash
cd src/frontend
npm run build
```

**Verify:**
- [ ] Build completes successfully
- [ ] Output shows: `✓ Compiled successfully`
- [ ] Output shows: `✓ Linting and checking validity of types`
- [ ] Output shows: `✓ Collecting page data`
- [ ] Output shows: `✓ Generating static pages`
- [ ] Output shows: `✓ Finalizing page optimization`
- [ ] No errors or warnings
- [ ] `.next/` directory is created
- [ ] Build summary shows route table

**Expected route table:**
```
Route (app)                Size     First Load JS
┌ ○ /                      X kB          XX kB
└ ○ /_not-found            X kB          XX kB

○  (Static)  prerendered as static content
```

### 3. Production Server Test

```bash
cd src/frontend
npm start
```

**Verify:**
- [ ] Production server starts
- [ ] Listens on port 3000
- [ ] Browser can access http://localhost:3000
- [ ] Page loads correctly in production mode

### 4. Linting Test

```bash
cd src/frontend
npm run lint
```

**Verify:**
- [ ] ESLint runs successfully
- [ ] No linting errors in default files
- [ ] May show suggestions but no blocking errors

## Architecture Verification

### App Router Structure
- [ ] Uses `src/app/` directory (NOT `pages/` directory)
- [ ] `layout.tsx` exists at root of app directory
- [ ] `page.tsx` defines the home route

### TypeScript Integration
- [ ] All component files use `.tsx` extension
- [ ] No `.js` or `.jsx` files in src/app/
- [ ] TypeScript compiles without errors

### Styling System
- [ ] Tailwind CSS classes work in components
- [ ] Global styles compile correctly
- [ ] PostCSS processes Tailwind directives

## Common Issues and Fixes

### Issue: "Module not found" errors
**Fix:**
```bash
cd src/frontend
rm -rf node_modules package-lock.json
npm install
```

### Issue: TypeScript errors on first run
**Fix:**
```bash
cd src/frontend
rm -rf .next
npm run dev
```

### Issue: Port 3000 already in use
**Fix:**
```bash
npx kill-port 3000
# Or use different port
PORT=3001 npm run dev
```

### Issue: Tailwind styles not applying
**Fix:**
1. Check `tailwind.config.ts` content paths
2. Verify `globals.css` has Tailwind directives
3. Restart dev server

### Issue: Hot reload not working
**Fix:**
1. Check file watcher limits (Linux):
   ```bash
   echo fs.inotify.max_user_watches=524288 | sudo tee -a /etc/sysctl.conf
   sudo sysctl -p
   ```
2. Restart dev server

## Final Acceptance Criteria

All of these must be ✓ checked:

- [ ] ✅ All required files exist
- [ ] ✅ `npm run dev` starts without errors
- [ ] ✅ Dev server accessible at http://localhost:3000
- [ ] ✅ Default welcome page displays correctly
- [ ] ✅ Hot reload works
- [ ] ✅ `npm run build` completes successfully
- [ ] ✅ No TypeScript errors
- [ ] ✅ No ESLint errors
- [ ] ✅ App Router architecture (src/app/ directory)
- [ ] ✅ TypeScript enabled (all .tsx files)
- [ ] ✅ Tailwind CSS working

## Documentation

After verification:
- [ ] Screenshot of running app taken
- [ ] Any issues encountered documented
- [ ] Next steps identified
- [ ] Team notified of completion

## Next Steps

Once all criteria are met:
1. Proceed to Step 3: Configure API integration
2. Begin building actual application pages
3. Implement component library
4. Add state management if needed
5. Set up testing infrastructure

---

**Checklist Version:** 1.0  
**Created:** 2026-02-24  
**For:** MythAPI Training Demo - Step 2
