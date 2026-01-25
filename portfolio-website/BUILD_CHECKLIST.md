# Build & Deployment Checklist

## ✅ Fixed Issues

1. **Project Type Export** - Verified `Project` interface is correctly exported from `src/data/portfolio.ts`
2. **Image Paths** - Updated all image paths from `/src/assets/...` to `/assets/...` for production compatibility
3. **Deployment Steps** - Added deployment instructions to README.md

## 📝 Changed Files

1. `src/data/portfolio.ts` - Updated screenshot paths (3 projects)
2. `src/pages/Home.tsx` - Updated profile image path
3. `README.md` - Added deployment section

## 🏗️ Build Command

```bash
cd portfolio-website
npm run build
```

## 📋 Pre-Build Checklist

- [x] Project type is exported correctly
- [x] All image paths updated to use `/assets/...` (public folder)
- [x] TypeScript types are consistent
- [x] All imports are correct

## 📦 Image Setup

**Important:** Images should be placed in the `public` folder:

```
public/
├── assets/
│   ├── profile.jpg
│   └── projects/
│       ├── servicehubpro/
│       │   ├── dashboard.png
│       │   ├── customers.png
│       │   ├── appointments.png
│       │   ├── workorders.png
│       │   └── invoices.png
│       ├── temple/
│       │   ├── dashboard.png
│       │   ├── members.png
│       │   ├── services.png
│       │   └── admin.png
│       └── ticket-classifier/
│           ├── api.png
│           ├── swagger.png
│           └── results.png
└── resume.pdf
```

## 🚀 Deployment

After building, deploy the `dist` folder to:
- **Netlify**: Drag & drop `dist` folder or connect Git repo
- **Vercel**: Run `vercel` or connect Git repo
- **GitHub Pages**: Use `gh-pages` package (see DEPLOYMENT.md)

## 🔍 Verification

After build, verify:
1. No TypeScript errors
2. All images load correctly
3. Routes work (test with `npm run preview`)
4. Dark mode toggle works
5. All pages render correctly
