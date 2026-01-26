# Ruchika Rajodiya - Portfolio Website

A modern, responsive portfolio website built with React, TypeScript, and TailwindCSS.

## 🚀 Features

- **Modern Design**: Clean, minimal, and professional
- **Dark Mode**: Toggle between light and dark themes
- **Responsive**: Works perfectly on all devices
- **Fast**: Optimized for performance with lazy loading
- **Accessible**: WCAG compliant with proper ARIA labels
- **SEO Optimized**: Meta tags and structured data
- **Animations**: Smooth transitions with Framer Motion

## 🛠️ Tech Stack

- **Frontend**: React 18 + TypeScript + Vite
- **Styling**: TailwindCSS
- **Animations**: Framer Motion
- **Icons**: lucide-react
- **Forms**: react-hook-form + zod validation
- **SEO**: react-helmet-async
- **Routing**: react-router-dom
- **Email**: EmailJS (with mailto fallback)

## 📦 Installation

1. **Clone the repository**
   ```bash
   git clone <your-repo-url>
   cd portfolio-website
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Start development server**
   ```bash
   npm run dev
   ```

4. **Open in browser**
   Navigate to `http://localhost:5173`

## 🚀 Quick Deploy to Get Public Link

**Want a public link everyone can visit?** See [GET_PUBLIC_LINK.md](./GET_PUBLIC_LINK.md) for the simplest steps!

**TL;DR:**
1. Run `npm run build`
2. Drag `dist` folder to https://app.netlify.com/drop
3. Get your public link in 30 seconds!

## 🏗️ Build

```bash
npm run build
```

The build output will be in the `dist` folder.

## 🚀 Deployment

### Vercel Deployment (Recommended)

**Option 1: Vercel CLI (Quick)**
```bash
# Install Vercel CLI globally
npm install -g vercel

# Deploy (run from portfolio-website directory)
vercel

# For production deployment
vercel --prod
```

**Option 2: Vercel Dashboard (Git Integration)**
1. Push your code to GitHub
2. Go to https://vercel.com
3. Click "Add New Project"
4. Import your GitHub repository
5. Vercel will auto-detect Vite settings:
   - Framework Preset: Vite
   - Build Command: `npm run build`
   - Output Directory: `dist`
6. Click "Deploy"
7. Your site will be live at `https://your-project.vercel.app`

**Note:** The `vercel.json` file is already configured for React Router SPA routing. All routes (`/`, `/projects`, `/projects/:slug`, `/resume`, `/contact`) work on Vercel; direct links and refresh use the same rewrite rules.

### Quick Deployment Steps

1. **Build the project**
   ```bash
   npm run build
   ```

2. **Test the build locally**
   ```bash
   npm run preview
   ```

3. **Deploy to your preferred platform**

   **Netlify:**
   - Drag and drop the `dist` folder to [Netlify](https://app.netlify.com/drop)
   - Or connect your Git repository and set build command: `npm run build` and publish directory: `dist`

   **Vercel:**
   - Install Vercel CLI: `npm i -g vercel`
   - Run: `vercel` in the project directory
   - Or connect your Git repository at [Vercel](https://vercel.com)

   **GitHub Pages:**
   - Install gh-pages: `npm install --save-dev gh-pages`
   - Add to package.json scripts: `"deploy": "gh-pages -d dist"`
   - Update `vite.config.ts` base to `'/your-repo-name/'`
   - Run: `npm run deploy`

For detailed deployment instructions, see [DEPLOYMENT.md](./DEPLOYMENT.md).

## 📁 Project Structure

```
portfolio-website/
├── public/
│   ├── profile.jpg           # Your profile photo (home page)
│   ├── resume.pdf            # Resume PDF
│   └── assets/
│       └── projects/
│           ├── servicehubpro/   # dashboard.png, customers.png, etc.
│           ├── temple/          # dashboard.png, members.png, etc.
│           └── ticket-classifier/ # api.png, swagger.png, results.png
├── src/
│   ├── components/
│   ├── data/portfolio.ts
│   ├── pages/
│   └── ...
├── .env.example              # Copy to .env and add EmailJS keys
└── package.json
```

### Where to place images (production-safe)

- **Profile image:** `public/profile.jpg`  
  - Served as `/profile.jpg`. Used on the Home page.

- **Project screenshots:** `public/assets/projects/<slug>/<name>.png`  
  - Examples:  
    - `public/assets/projects/servicehubpro/dashboard.png` → `/assets/projects/servicehubpro/dashboard.png`  
    - `public/assets/projects/temple/members.png` → `/assets/projects/temple/members.png`  
  - Update `screenshots` in `src/data/portfolio.ts` to match these paths (e.g. `'/assets/projects/servicehubpro/dashboard.png'`).

- **Resume PDF:** `public/resume.pdf` → `/resume.pdf`

---

## 📧 EmailJS setup (contact form → Gmail)

The contact form sends messages via [EmailJS](https://www.emailjs.com/). Without it, the form falls back to `mailto:` (opens the user’s email client).

### 1. Create an EmailJS account

1. Go to [https://www.emailjs.com/](https://www.emailjs.com/) and sign up.
2. Verify your email.

### 2. Add an Email Service (Gmail)

1. **Email Services** → **Add New Service**.
2. Choose **Gmail**.
3. Connect your Gmail account and complete the steps.
4. Copy the **Service ID** (e.g. `service_xxxxx`).

### 3. Create an Email Template

1. **Email Templates** → **Create New Template**.
2. Set **To Email** to your Gmail: `ruchikarajodiya25@gmail.com`.
3. Use variables in the template, for example:
   - **Subject:** `{{subject}}`
   - **Content:**  
     `From: {{from_name}} ({{from_email}})`  
     `Message: {{message}}`
4. Save and copy the **Template ID** (e.g. `template_xxxxx`).

### 4. Get your Public Key

1. **Account** → **API Keys** (or **General**).
2. Copy your **Public Key**.

### 5. Local development (.env)

1. Copy the example file:
   ```bash
   cp .env.example .env
   ```
2. Edit `.env` and set:
   ```
   VITE_EMAILJS_SERVICE_ID=your_service_id
   VITE_EMAILJS_TEMPLATE_ID=your_template_id
   VITE_EMAILJS_PUBLIC_KEY=your_public_key
   ```
3. Restart the dev server (`npm run dev`).  
   **Do not commit `.env`** — it’s gitignored.

### 6. Vercel (production)

1. Open your project on [Vercel](https://vercel.com) → **Settings** → **Environment Variables**.
2. Add:

   | Name | Value |
   |------|--------|
   | `VITE_EMAILJS_SERVICE_ID` | Your Service ID |
   | `VITE_EMAILJS_TEMPLATE_ID` | Your Template ID |
   | `VITE_EMAILJS_PUBLIC_KEY` | Your Public Key |

3. Redeploy (e.g. **Deployments** → **Redeploy** or push a new commit).

Messages from the contact form will be sent to your Gmail via EmailJS. If any of these env vars are missing, the form uses the `mailto:` fallback.

## 📝 Updating Content

See [CONTENT_UPDATE_GUIDE.md](./CONTENT_UPDATE_GUIDE.md) for detailed instructions on:
- Adding/updating profile image
- Adding project screenshots
- Updating project information
- Changing social links
- Uploading resume PDF

## 🚢 Deployment

See [DEPLOYMENT.md](./DEPLOYMENT.md) for deployment instructions:
- Netlify
- Vercel
- GitHub Pages

## 🎨 Customization

### Colors

Edit `tailwind.config.js` to change the color scheme:

```javascript
colors: {
  primary: {
    // Your color values
  }
}
```

### Fonts

Update fonts in `tailwind.config.js` and `index.html`.

## 📄 License

This project is open source and available under the MIT License.

## 👤 Author

**Ruchika Rajodiya**
- Email: ruchikarajodiya25@gmail.com
- LinkedIn: [Your LinkedIn](https://linkedin.com/in/REPLACE_ME)
- GitHub: [Your GitHub](https://github.com/REPLACE_ME)

---

Built with ❤️ using React, TypeScript, and TailwindCSS
