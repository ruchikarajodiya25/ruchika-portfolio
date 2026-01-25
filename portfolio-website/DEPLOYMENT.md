# Deployment Guide

This guide covers deploying your portfolio website to various platforms.

## Table of Contents
- [Prerequisites](#prerequisites)
- [Netlify](#netlify)
- [Vercel](#vercel)
- [GitHub Pages](#github-pages)
- [General Deployment Tips](#general-deployment-tips)

---

## Prerequisites

Before deploying, ensure you have:

1. **Built the project locally**
   ```bash
   npm install
   npm run build
   ```

2. **Tested the build**
   ```bash
   npm run preview
   ```

3. **Updated all placeholder content**
   - Replace `REPLACE_ME` in GitHub/LinkedIn links
   - Add your profile image
   - Add project screenshots
   - Upload resume PDF

---

## Netlify

### Option 1: Netlify CLI

1. **Install Netlify CLI**
   ```bash
   npm install -g netlify-cli
   ```

2. **Login to Netlify**
   ```bash
   netlify login
   ```

3. **Initialize Netlify**
   ```bash
   netlify init
   ```
   - Choose "Create & configure a new site"
   - Select your team
   - Choose a site name (or let Netlify generate one)

4. **Deploy**
   ```bash
   netlify deploy --prod
   ```

### Option 2: Netlify Dashboard (Drag & Drop)

1. **Build the project**
   ```bash
   npm run build
   ```

2. **Go to Netlify Dashboard**
   - Visit https://app.netlify.com
   - Drag and drop the `dist` folder to deploy

### Option 3: Git Integration (Recommended)

1. **Push to GitHub**
   ```bash
   git add .
   git commit -m "Initial commit"
   git push origin main
   ```

2. **Connect to Netlify**
   - Go to https://app.netlify.com
   - Click "Add new site" → "Import an existing project"
   - Connect your GitHub repository
   - Configure build settings:
     - **Build command:** `npm run build`
     - **Publish directory:** `dist`
   - Click "Deploy site"

### Netlify Configuration File

Create `netlify.toml` in the root directory:

```toml
[build]
  command = "npm run build"
  publish = "dist"

[[redirects]]
  from = "/*"
  to = "/index.html"
  status = 200
```

This ensures React Router works correctly with client-side routing.

---

## Vercel

### Option 1: Vercel CLI (Quickest)

1. **Install Vercel CLI**
   ```bash
   npm install -g vercel
   ```

2. **Login to Vercel**
   ```bash
   vercel login
   ```
   This will open your browser to authenticate.

3. **Deploy from project directory**
   ```bash
   cd portfolio-website
   vercel
   ```
   - Follow the prompts:
     - Set up and deploy? **Yes**
     - Which scope? (Select your account)
     - Link to existing project? **No** (for first deployment)
     - Project name? (Press Enter for default)
     - Directory? **./** (current directory)
     - Override settings? **No**

4. **Deploy to production**
   ```bash
   vercel --prod
   ```
   Your site will be live at `https://your-project.vercel.app`

### Option 2: Git Integration (Recommended - Auto-Deploy)

1. **Push to GitHub**
   ```bash
   git add .
   git commit -m "Initial commit"
   git push origin main
   ```

2. **Connect to Vercel**
   - Go to https://vercel.com
   - Click "Add New Project"
   - Import your GitHub repository
   - Vercel will auto-detect Vite settings:
     - **Framework Preset:** Vite
     - **Build Command:** `npm run build`
     - **Output Directory:** `dist`
     - **Install Command:** `npm install`
   - Click "Deploy"

3. **Automatic deployments**
   - Every push to `main` branch = automatic production deployment
   - Every pull request = preview deployment

### Vercel Configuration

The `vercel.json` file is already configured in your project:

```json
{
  "rewrites": [
    {
      "source": "/(.*)",
      "destination": "/index.html"
    }
  ]
}
```

This ensures React Router works correctly - all routes redirect to `index.html` for client-side routing.

### Vercel Build Settings

Vercel automatically detects Vite projects, but you can verify:
- **Build Command:** `npm run build`
- **Output Directory:** `dist`
- **Install Command:** `npm install`

### Custom Domain on Vercel

1. Go to your project → Settings → Domains
2. Add your custom domain (e.g., `ruchikarajodiya.com`)
3. Follow DNS configuration instructions
4. Vercel will automatically provision SSL certificate

---

## GitHub Pages

### Setup

1. **Install gh-pages package**
   ```bash
   npm install --save-dev gh-pages
   ```

2. **Update package.json**
   Add these scripts:
   ```json
   {
     "scripts": {
       "predeploy": "npm run build",
       "deploy": "gh-pages -d dist"
     }
   }
   ```

3. **Update vite.config.ts**
   ```typescript
   import { defineConfig } from 'vite'
   import react from '@vitejs/plugin-react'

   export default defineConfig({
     plugins: [react()],
     base: '/your-repo-name/', // Replace with your GitHub repo name
   })
   ```

4. **Deploy**
   ```bash
   npm run deploy
   ```

5. **Enable GitHub Pages**
   - Go to your repository on GitHub
   - Settings → Pages
   - Source: `gh-pages` branch
   - Save

### Important Notes for GitHub Pages

- Update the `base` path in `vite.config.ts` to match your repository name
- If your repo is `ruchika-portfolio`, set `base: '/ruchika-portfolio/'`
- For custom domains, you can set `base: '/'` after configuring the domain

---

## General Deployment Tips

### Environment Variables

If you need environment variables (e.g., EmailJS keys):

1. **Create `.env` file** (for local development)
   ```
   VITE_EMAILJS_SERVICE_ID=your_service_id
   VITE_EMAILJS_TEMPLATE_ID=your_template_id
   VITE_EMAILJS_PUBLIC_KEY=your_public_key
   ```

2. **Update code** to use `import.meta.env.VITE_EMAILJS_SERVICE_ID`

3. **Set in deployment platform**
   - Netlify: Site settings → Environment variables
   - Vercel: Project settings → Environment Variables
   - GitHub Pages: Not supported (use build-time values)

### Image Optimization

1. **Use WebP format** for better compression
2. **Optimize images** before uploading (use tools like TinyPNG)
3. **Use lazy loading** (already implemented in components)

### Performance

1. **Enable compression** (usually automatic on Netlify/Vercel)
2. **Use CDN** (automatic on Netlify/Vercel)
3. **Check Lighthouse scores** after deployment

### Custom Domain

1. **Purchase a domain** (e.g., from Namecheap, Google Domains)
2. **Configure DNS**
   - Netlify: Add domain in Site settings → Domain management
   - Vercel: Add domain in Project settings → Domains
3. **Update base path** in `vite.config.ts` if needed

### Troubleshooting

**Issue: 404 errors on refresh**
- Solution: Ensure redirect rules are configured (see Netlify/Vercel configs above)

**Issue: Images not loading**
- Solution: Check image paths are correct and relative to public folder

**Issue: Dark mode not persisting**
- Solution: localStorage should work, but check browser console for errors

**Issue: Contact form not working**
- Solution: Configure EmailJS or ensure mailto fallback works

---

## Post-Deployment Checklist

- [ ] Test all pages load correctly
- [ ] Verify all links work (GitHub, LinkedIn, etc.)
- [ ] Check images load properly
- [ ] Test contact form
- [ ] Verify dark mode toggle works
- [ ] Test on mobile devices
- [ ] Run Lighthouse audit
- [ ] Check SEO meta tags
- [ ] Verify resume PDF downloads correctly
- [ ] Test all project detail pages

---

## Continuous Deployment

Once set up with Git integration:

1. **Make changes** to your code
2. **Commit and push** to GitHub
3. **Automatic deployment** will trigger
4. **Check deployment status** in your platform's dashboard

---

## Need Help?

- **Netlify Docs:** https://docs.netlify.com
- **Vercel Docs:** https://vercel.com/docs
- **GitHub Pages Docs:** https://docs.github.com/pages
- **Vite Deployment Guide:** https://vitejs.dev/guide/static-deploy.html
