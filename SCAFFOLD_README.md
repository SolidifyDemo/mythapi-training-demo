# Scaffolding the Next.js Frontend

## Quick Start

To scaffold the Next.js 14+ frontend for this project, run:

```bash
chmod +x scaffold-nextjs.sh
./scaffold-nextjs.sh
```

## What This Does

The script will:
1. ✅ Check Node.js version (18.17+ required)
2. ✅ Run `create-next-app` with correct configuration
3. ✅ Verify all required files are created
4. ✅ Test development server
5. ✅ Run production build test

## Requirements

- Node.js 18.17 or later
- npm 9.0 or later
- Internet connection (to download packages)

## Options

### Configuration Used

The script scaffolds with these options:
- **App Router:** Modern routing with `app/` directory
- **TypeScript:** Type-safe development
- **Tailwind CSS:** Utility-first styling
- **ESLint:** Code quality checks
- **src/ directory:** Source code organization
- **No import alias:** Clean imports without @/ prefix

### Manual Scaffolding

If you prefer to scaffold manually:

```bash
npx create-next-app@latest src/frontend \
  --app \
  --ts \
  --tailwind \
  --eslint \
  --src-dir \
  --no-import-alias
```

## After Scaffolding

1. **Start Development Server:**
   ```bash
   cd src/frontend
   npm run dev
   ```
   Open http://localhost:3000

2. **Build for Production:**
   ```bash
   cd src/frontend
   npm run build
   ```

3. **Read Full Documentation:**
   See [NEXTJS_SCAFFOLDING_GUIDE.md](./NEXTJS_SCAFFOLDING_GUIDE.md) for comprehensive details.

## Troubleshooting

### Script Permission Denied
```bash
chmod +x scaffold-nextjs.sh
```

### Node.js Not Found
Install Node.js from https://nodejs.org/ (LTS version recommended)

### Port 3000 In Use
```bash
npx kill-port 3000
# or
PORT=3001 npm run dev
```

## Next Steps

After successful scaffolding:
- Review generated files in `src/frontend/`
- Customize `src/app/page.tsx` for your needs
- Set up API integration with backend
- Add additional pages and components

For detailed next steps, see the full guide: [NEXTJS_SCAFFOLDING_GUIDE.md](./NEXTJS_SCAFFOLDING_GUIDE.md)
