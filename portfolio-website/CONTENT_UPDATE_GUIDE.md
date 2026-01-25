# Content Update Guide

This guide explains how to update various content in your portfolio website.

## Table of Contents
- [Profile Image](#profile-image)
- [Project Screenshots](#project-screenshots)
- [Project Information](#project-information)
- [Social Links](#social-links)
- [Resume PDF](#resume-pdf)
- [Contact Form](#contact-form)

---

## Profile Image

### Location
Place your profile image at: `/src/assets/profile.jpg`

### Steps
1. Prepare your image (recommended: square, 400x400px or larger, JPG/PNG)
2. Replace the file at `portfolio-website/src/assets/profile.jpg`
3. Update the import in `src/pages/Home.tsx` if you change the filename

### Code Update
In `src/pages/Home.tsx`, find the profile image section and update:

```tsx
<img 
  src="/src/assets/profile.jpg" 
  alt="Ruchika Rajodiya" 
  className="w-32 h-32 rounded-full object-cover"
/>
```

---

## Project Screenshots

### Directory Structure
Screenshots should be organized in the following directories:

```
src/assets/
  projects/
    servicehubpro/
      dashboard.png
      customers.png
      appointments.png
      workorders.png
      invoices.png
    temple/
      dashboard.png
      members.png
      services.png
      admin.png
    ticket-classifier/
      api.png
      swagger.png
      results.png
```

### Steps
1. Prepare your screenshots (recommended: 1920x1080px or similar aspect ratio, PNG/JPG)
2. Place them in the appropriate project folder
3. Update the `screenshots` array in `src/data/portfolio.ts` if filenames change

### Code Update
In `src/data/portfolio.ts`, update the `screenshots` array for each project:

```typescript
screenshots: [
  '/src/assets/projects/servicehubpro/dashboard.png',
  '/src/assets/projects/servicehubpro/customers.png',
  // ... add more paths
]
```

**Note:** In production, these paths will be relative to the public folder. For Vite, you can also import images directly:

```typescript
import dashboardImg from '../assets/projects/servicehubpro/dashboard.png';
```

---

## Project Information

### Location
All project data is stored in: `/src/data/portfolio.ts`

### Updating Project Details

1. **Basic Information**
   - `id`: Unique identifier
   - `slug`: URL-friendly identifier (e.g., `servicehub-pro`)
   - `title`: Project title
   - `role`: Your role in the project
   - `date`: Project date
   - `status`: `'completed' | 'in-progress' | 'archived'`
   - `summary`: Short description (shown in cards)
   - `description`: Full description (shown on detail page)

2. **Tech Stack**
   - `frontend.stack`: Array of frontend technologies
   - `backend.stack`: Array of backend technologies
   - `tags`: Array of tags for filtering

3. **Details**
   - `frontend.details`: Array of implementation details
   - `backend.details`: Array of implementation details
   - `features`: Array of feature descriptions
   - `architecture`: (Optional) Architecture details
   - `challenges`: (Optional) Challenges faced
   - `results`: (Optional) Results/metrics

4. **Links**
   - `github`: GitHub repository URL
   - `live`: Live demo URL (or `'https://REPLACE_ME'` if not available)

### Example Update

```typescript
{
  id: '1',
  slug: 'my-new-project',
  title: 'My New Project',
  role: 'Full-Stack Developer',
  date: '2024',
  status: 'completed',
  summary: 'A brief summary of the project',
  description: 'A detailed description...',
  frontend: {
    stack: ['React', 'TypeScript', 'TailwindCSS'],
    details: ['Built responsive UI', 'Implemented state management']
  },
  backend: {
    stack: ['Node.js', 'Express', 'MongoDB'],
    details: ['Created REST APIs', 'Implemented authentication']
  },
  features: ['Feature 1', 'Feature 2'],
  github: 'https://github.com/yourusername/project',
  live: 'https://yourproject.com',
  screenshots: ['/src/assets/projects/myproject/screenshot1.png'],
  tags: ['React', 'Node.js', 'MongoDB']
}
```

### Adding a New Project

1. Add the project object to the `projects` array in `src/data/portfolio.ts`
2. Create a folder for screenshots: `src/assets/projects/[project-slug]/`
3. Add screenshots to the folder
4. Update the `screenshots` array with correct paths
5. The project will automatically appear on the Projects page

---

## Social Links

### Location
Update in: `/src/data/portfolio.ts` → `personalInfo` object

### Fields to Update
- `linkedin`: Your LinkedIn profile URL
- `github`: Your GitHub profile URL
- `email`: Your email address
- `phone`: Your phone number
- `portfolio`: Your portfolio website URL

### Example

```typescript
export const personalInfo: PersonalInfo = {
  // ... other fields
  linkedin: 'https://linkedin.com/in/yourprofile',
  github: 'https://github.com/yourusername',
  email: 'your.email@example.com',
  phone: '+1 123-456-7890',
  portfolio: 'https://yourportfolio.com',
  // ...
}
```

---

## Resume PDF

### Location
Place your resume PDF at: `/public/resume.pdf`

### Steps
1. Prepare your resume as a PDF
2. Name it `resume.pdf`
3. Place it in the `public` folder (create if it doesn't exist)
4. The download button will automatically work

### Code Location
The download functionality is in `src/pages/Resume.tsx`. The button triggers a download of `/resume.pdf`.

---

## Contact Form

### EmailJS Setup (Optional)

If you want to use EmailJS instead of mailto fallback:

1. **Sign up for EmailJS**
   - Go to https://www.emailjs.com/
   - Create an account and verify your email

2. **Create an Email Service**
   - Go to Email Services → Add New Service
   - Choose your email provider (Gmail, Outlook, etc.)
   - Follow the setup instructions

3. **Create an Email Template**
   - Go to Email Templates → Create New Template
   - Use variables: `{{from_name}}`, `{{from_email}}`, `{{subject}}`, `{{message}}`

4. **Get Your Keys**
   - Service ID: Found in Email Services
   - Template ID: Found in Email Templates
   - Public Key: Found in Account → API Keys

5. **Update the Code**
   In `src/pages/Contact.tsx`, uncomment and update:

```typescript
await emailjs.send(
  'YOUR_SERVICE_ID',      // Replace with your Service ID
  'YOUR_TEMPLATE_ID',     // Replace with your Template ID
  {
    from_name: data.name,
    from_email: data.email,
    subject: data.subject,
    message: data.message,
  },
  'YOUR_PUBLIC_KEY'       // Replace with your Public Key
);
```

### Mailto Fallback
The contact form currently uses a mailto fallback, which opens the user's default email client. This works without any setup but requires the user to have an email client configured.

---

## Personal Information

### Location
Update in: `/src/data/portfolio.ts` → `personalInfo` object

### Fields
- `name`: Your full name
- `title`: Your professional title
- `location`: Your location
- `tagline`: Short tagline for hero section
- `about`: Array of paragraphs for About section
- `skills`: Object containing skill arrays

### Example

```typescript
export const personalInfo: PersonalInfo = {
  name: 'Your Name',
  title: 'Your Title',
  location: 'Your Location',
  tagline: 'Your tagline',
  about: [
    'First paragraph about you...',
    'Second paragraph about you...'
  ],
  skills: {
    languages: ['JavaScript', 'Python', 'C#'],
    frontend: ['React', 'Next.js'],
    backend: ['Node.js', 'ASP.NET'],
    databases: ['PostgreSQL', 'MongoDB'],
    tools: ['Git', 'Docker']
  }
}
```

---

## Experience & Education

### Location
Update in: `/src/data/portfolio.ts`

### Experience
Update the `experience` array:

```typescript
export const experience: Experience[] = [
  {
    title: 'Your Job Title',
    company: 'Company Name',
    location: 'City, Country',
    startDate: 'Jan 2023',
    endDate: 'Present',
    bullets: [
      'Achievement 1',
      'Achievement 2',
      // ...
    ]
  }
]
```

### Education
Update the `education` array:

```typescript
export const education: Education[] = [
  {
    degree: 'Degree Name',
    institution: 'Institution Name',
    location: 'Location',
    startDate: 'Jan 2023',
    endDate: 'Dec 2025'
  }
]
```

---

## Styling & Theme

### Colors
Update in: `tailwind.config.js` → `theme.extend.colors.primary`

### Fonts
Update in: `tailwind.config.js` → `theme.extend.fontFamily`

### Dark Mode
Dark mode is automatically handled. Users can toggle it, and their preference is saved in localStorage.

---

## Need Help?

If you encounter any issues while updating content:
1. Check that file paths are correct
2. Ensure TypeScript types match (check `src/data/portfolio.ts` interfaces)
3. Verify image formats are supported (PNG, JPG, SVG)
4. Check browser console for errors

For code-related questions, refer to the main README.md file.
