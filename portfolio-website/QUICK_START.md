# Quick Start Guide

## 🚀 Getting Started

### 1. Install Dependencies

```bash
npm install
```

### 2. Update Content

Before running the project, update the following:

1. **Replace placeholder links** in `src/data/portfolio.ts`:
   - Replace `REPLACE_ME` in GitHub and LinkedIn URLs
   - Update email and phone if needed

2. **Add your profile image**:
   - Place `profile.jpg` in `src/assets/profile.jpg`
   - Recommended size: 400x400px or larger, square format

3. **Add project screenshots**:
   - ServiceHub Pro: `src/assets/projects/servicehubpro/`
   - Temple Management: `src/assets/projects/temple/`
   - Ticket Classifier: `src/assets/projects/ticket-classifier/`

4. **Add resume PDF**:
   - Place `resume.pdf` in `public/resume.pdf`

### 3. Run Development Server

```bash
npm run dev
```

Open [http://localhost:5173](http://localhost:5173) in your browser.

### 4. Build for Production

```bash
npm run build
```

The built files will be in the `dist` folder.

### 5. Preview Production Build

```bash
npm run preview
```

## 📝 Important Notes

### Image Paths

For images in `src/assets`, you have two options:

**Option 1: Import images (Recommended)**
```typescript
import profileImg from '../assets/profile.jpg';
// Use: <img src={profileImg} />
```

**Option 2: Use public folder**
- Move images to `public/` folder
- Reference as `/image-name.jpg`

Currently, the code uses paths like `/src/assets/...` which will work in development but may need adjustment for production. Consider importing images directly or moving them to the public folder.

### EmailJS Setup

The contact form currently uses a mailto fallback. To use EmailJS:

1. Sign up at https://www.emailjs.com/
2. Get your Service ID, Template ID, and Public Key
3. Uncomment and update the EmailJS code in `src/pages/Contact.tsx`

See `CONTENT_UPDATE_GUIDE.md` for detailed instructions.

## 🐛 Troubleshooting

**Images not loading?**
- Check file paths are correct
- Ensure images are in the right folders
- Try importing images directly instead of using paths

**Build errors?**
- Run `npm install` again
- Check TypeScript errors: `npm run build`
- Ensure all dependencies are installed

**Dark mode not working?**
- Check browser console for errors
- Clear localStorage and try again

## 📚 Next Steps

- Read `CONTENT_UPDATE_GUIDE.md` for detailed content updates
- Read `DEPLOYMENT.md` for deployment instructions
- Customize colors in `tailwind.config.js`
- Update content in `src/data/portfolio.ts`
