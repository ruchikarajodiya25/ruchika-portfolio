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

**Note:** The `vercel.json` file is already configured for React Router SPA routing.

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
│   └── resume.pdf          # Your resume PDF
├── src/
│   ├── assets/
│   │   ├── profile.jpg      # Profile image
│   │   └── projects/        # Project screenshots
│   ├── components/          # Reusable components
│   ├── data/
│   │   └── portfolio.ts     # All content data
│   ├── hooks/               # Custom React hooks
│   ├── pages/               # Page components
│   ├── utils/               # Utility functions
│   ├── App.tsx              # Main app component
│   ├── main.tsx             # Entry point
│   └── index.css            # Global styles
├── CONTENT_UPDATE_GUIDE.md   # How to update content
├── DEPLOYMENT.md            # Deployment instructions
└── package.json
```

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
