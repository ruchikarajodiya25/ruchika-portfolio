// ServiceHub Pro screenshots (01.png–13.png in src/assets/projects/)
import sh01 from '../assets/projects/01.png';
import sh02 from '../assets/projects/02.png';
import sh03 from '../assets/projects/03.png';
import sh04 from '../assets/projects/04.png';
import sh05 from '../assets/projects/05.png';
import sh06 from '../assets/projects/06.png';
import sh07 from '../assets/projects/07.png';
import sh08 from '../assets/projects/08.png';
import sh09 from '../assets/projects/09.png';
import sh10 from '../assets/projects/10.png';
import sh11 from '../assets/projects/11.png';
import sh12 from '../assets/projects/12.png';
import sh13 from '../assets/projects/13.png';

export interface Project {
  id: string;
  slug: string;
  title: string;
  role: string;
  date: string;
  status: 'completed' | 'in-progress' | 'archived';
  summary: string;
  description: string;
  frontend: {
    stack: string[];
    details: string[];
  };
  backend: {
    stack: string[];
    details: string[];
  };
  features: string[];
  architecture?: string[];
  challenges?: string[];
  results?: string[];
  github: string;
  live: string;
  /** Public URLs, e.g. /assets/projects/slug/name.png. Empty = no screenshots. */
  screenshots?: string[];
  tags: string[];
}

export interface Experience {
  title: string;
  company: string;
  location: string;
  startDate: string;
  endDate: string;
  bullets: string[];
}

export interface Education {
  degree: string;
  institution: string;
  location: string;
  startDate: string;
  endDate: string;
}

export interface PersonalInfo {
  name: string;
  title: string;
  location: string;
  email: string;
  phone: string;
  linkedin: string;
  github: string;
  portfolio: string;
  tagline: string;
  about: string[];
  skills: {
    languages: string[];
    frontend: string[];
    backend: string[];
    databases: string[];
    tools: string[];
  };
}

export const personalInfo: PersonalInfo = {
  name: 'Ruchika Rajodiya',
  title: 'Software Engineer | Full-Stack & Backend Developer',
  location: 'Columbus, OH',
  email: 'ruchikarajodiya25@gmail.com',
  phone: '+1 312-284-7145',
  linkedin: 'https://www.linkedin.com/in/ruchika-nareshbhai-rajodiya-05199a360/',
  github: 'https://github.com/ruchikarajodiya25',
  portfolio: 'https://ruchikarajodiya.com',
  tagline: 'Building scalable backend systems and full-stack applications',
  about: [
    'Software Engineer with hands-on experience building backend and full-stack business applications using Node.js, Python, JavaScript, SQL, MongoDB, and ASP.NET MVC. Experienced in REST API development, database design, system integration, and improving legacy systems.',
    'Comfortable working in fast-paced environments and collaborating with cross-functional teams. Strong interest in AI-assisted applications, system optimization, and data-driven decision-making within business and logistics platforms.'
  ],
  skills: {
    languages: ['JavaScript', 'Python', 'C#', 'SQL', 'Java', 'TypeScript', 'HTML5', 'CSS3'],
    frontend: ['React', 'Next.js', 'TailwindCSS', 'HTML/CSS', 'Responsive UI'],
    backend: ['Node.js', 'Express.js', 'ASP.NET Core', 'ASP.NET MVC', 'FastAPI', 'REST APIs'],
    databases: ['PostgreSQL', 'SQL Server', 'MongoDB'],
    tools: ['Git', 'GitHub', 'CI/CD', 'Postman', 'Agile/Scrum', 'Docker']
  }
};

export const projects: Project[] = [
  {
    id: '1',
    slug: 'servicehub-pro',
    title: 'ServiceHub Pro — Multi-Tenant Business Operations Platform',
    role: 'Full-Stack Developer',
    date: '2024',
    status: 'completed',
    summary: 'A comprehensive multi-tenant platform for managing business operations including customers, appointments, work orders, inventory, invoicing, and payments.',
    description: 'ServiceHub Pro is a full-stack multi-tenant business operations platform designed to help service-based businesses manage their entire workflow. The platform provides complete isolation between tenants while maintaining a scalable architecture.',
    frontend: {
      stack: ['React', 'TypeScript', 'TailwindCSS', 'React Router', 'React Hook Form', 'Zod'],
      details: [
        'Built responsive dashboard with dark mode support',
        'Implemented reusable component library with TailwindCSS',
        'Created dynamic forms with validation using react-hook-form and zod',
        'Developed real-time UI updates with optimistic rendering',
        'Implemented role-based UI rendering (Owner/Manager/Staff views)',
        'Added smooth animations and transitions with Framer Motion'
      ]
    },
    backend: {
      stack: ['ASP.NET Core 8', 'Entity Framework Core', 'SQL Server', 'Identity', 'JWT', 'MediatR', 'Clean Architecture'],
      details: [
        'Implemented Clean Architecture with separation of concerns',
        'Built RESTful APIs following REST principles',
        'Implemented JWT authentication with access and refresh tokens',
        'Created multi-tenant isolation at database and application level',
        'Used MediatR for CQRS pattern implementation',
        'Implemented role-based access control (RBAC)',
        'Added comprehensive logging and error handling',
        'Optimized database queries with EF Core'
      ]
    },
    features: [
      'Multi-tenant isolation ensuring complete data separation',
      'JWT-based authentication with access/refresh token rotation',
      'Role-based access control: Owner, Manager, and Staff roles',
      'Customer management with contact history and preferences',
      'Appointment scheduling with conflict detection',
      'Work order creation and tracking with status updates',
      'Inventory management with low stock alerts',
      'Automated invoice generation from work orders',
      'Payment processing and tracking',
      'Comprehensive reporting and analytics dashboard'
    ],
    architecture: [
      'Clean Architecture with Domain, Application, Infrastructure, and Presentation layers',
      'CQRS pattern using MediatR for command/query separation',
      'Repository pattern for data access abstraction',
      'Unit of Work pattern for transaction management',
      'Dependency Injection throughout the application',
      'Multi-tenant data isolation using tenant ID filtering'
    ],
    challenges: [
      'Ensuring complete tenant isolation while maintaining performance',
      'Implementing complex business rules for work order to invoice conversion',
      'Managing concurrent access to shared resources',
      'Optimizing database queries for large datasets'
    ],
    results: [
      'Reduced API response time by ~40% through query optimization',
      'Achieved 99.9% uptime with proper error handling',
      'Scaled to support 100+ concurrent tenants',
      'Improved code maintainability with Clean Architecture'
    ],
    github: '',
    live: 'https://REPLACE_ME',
    screenshots: [sh01, sh02, sh03, sh04, sh05, sh06, sh07, sh08, sh09, sh10, sh11, sh12, sh13],
    tags: ['React', 'TypeScript', '.NET', 'SQL Server', 'Multi-Tenant', 'Clean Architecture']
  },
  {
    id: '2',
    slug: 'temple-management',
    title: 'Online Temple Management System',
    role: 'Full-Stack Developer',
    date: '2023',
    status: 'completed',
    summary: 'A comprehensive management system for temples to handle member records, service scheduling, and administrative tasks.',
    description: 'A full-stack web application built for temple administration to manage members, schedule services, and maintain records efficiently. The system includes admin dashboards, search functionality, and comprehensive validation.',
    frontend: {
      stack: ['ASP.NET MVC', 'Razor Pages', 'HTML5', 'CSS3', 'JavaScript', 'Bootstrap'],
      details: [
        'Built responsive UI with Bootstrap framework',
        'Created dynamic forms with client-side validation',
        'Implemented search and filtering functionality',
        'Developed admin dashboard with data visualization',
        'Added real-time form validation feedback'
      ]
    },
    backend: {
      stack: ['ASP.NET MVC', '.NET', 'SQL Server', 'Entity Framework'],
      details: [
        'Designed normalized database schema for member and service data',
        'Implemented CRUD operations with Entity Framework',
        'Created stored procedures for complex queries',
        'Added server-side validation and error handling',
        'Implemented authentication and authorization',
        'Optimized database queries for performance'
      ]
    },
    features: [
      'Member registration and profile management',
      'Service scheduling and calendar management',
      'Admin dashboard with key metrics',
      'Advanced search and filtering capabilities',
      'Data validation at both client and server side',
      'Role-based access for different user types',
      'Reporting and analytics features',
      'Transaction history tracking'
    ],
    challenges: [
      'Handling complex scheduling conflicts',
      'Managing large member databases efficiently',
      'Ensuring data integrity across related tables'
    ],
    results: [
      'Reduced data entry errors by ~60% through validation',
      'Improved query performance by ~35% with optimized stored procedures',
      'Streamlined administrative workflows'
    ],
    github: '',
    live: 'https://REPLACE_ME',
    screenshots: [],
    tags: ['.NET', 'ASP.NET MVC', 'SQL Server', 'Bootstrap']
  },
  {
    id: '3',
    slug: 'ticket-classifier',
    title: 'AI-Powered Customer Support Ticket Classifier',
    role: 'ML Engineer & Backend Developer',
    date: '2024',
    status: 'completed',
    summary: 'An ML-powered system that automatically classifies customer support tickets and assigns priority using natural language processing.',
    description: 'A machine learning application that processes customer support tickets, classifies them by category and priority, and provides a REST API for integration with existing support systems.',
    frontend: {
      stack: [],
      details: [
        'API-only project with Swagger UI documentation',
        'RESTful endpoints for ticket classification'
      ]
    },
    backend: {
      stack: ['Python', 'FastAPI', 'scikit-learn', 'pandas', 'numpy', 'SQL Server/PostgreSQL'],
      details: [
        'Built REST API with FastAPI framework',
        'Implemented text preprocessing pipeline (tokenization, stemming, TF-IDF)',
        'Trained classification models using scikit-learn',
        'Used cross-validation for model evaluation',
        'Stored predictions and metadata in database',
        'Created endpoints for real-time ticket scoring',
        'Implemented model versioning and A/B testing support'
      ]
    },
    features: [
      'Automatic ticket classification by category',
      'Priority assignment based on content analysis',
      'Text preprocessing and feature engineering',
      'REST API for integration with support systems',
      'Prediction storage for analytics and reporting',
      'Model evaluation with cross-validation',
      'Support for multiple classification models'
    ],
    challenges: [
      'Handling imbalanced dataset classes',
      'Preprocessing noisy text data',
      'Achieving high accuracy while maintaining low latency',
      'Scaling model inference for high-volume requests'
    ],
    results: [
      'Achieved ~85% classification accuracy',
      'Reduced average ticket processing time by ~50%',
      'Improved priority assignment accuracy',
      'Enabled data-driven insights through prediction analytics'
    ],
    github: '',
    live: 'https://REPLACE_ME',
    screenshots: [],
    tags: ['Python', 'FastAPI', 'Machine Learning', 'scikit-learn', 'NLP']
  }
];

export const experience: Experience[] = [
  {
    title: 'Software Engineer',
    company: 'Destiny Solution Pvt. Ltd',
    location: 'Ahmedabad, India',
    startDate: 'Sep 2022',
    endDate: 'Nov 2023',
    bullets: [
      'Built backend features using C# and ASP.NET MVC for authentication, admin workflows, and business logic',
      'Created and optimized SQL Server tables and queries for reliable data processing',
      'Helped maintain and improve legacy systems by fixing bugs and updating modules',
      'Implemented role-based access control for secure user permissions',
      'Debugged issues and improved backend stability and performance',
      'Worked with the team in Agile/Scrum to deliver features on time'
    ]
  },
  {
    title: 'Python Developer Intern',
    company: 'Infolabz IT Solution Pvt. Ltd',
    location: 'Ahmedabad, India',
    startDate: 'Jun 2022',
    endDate: 'Jul 2022',
    bullets: [
      'Built backend services and REST APIs using Python for data-driven applications',
      'Processed structured data (JSON) and integrated third-party APIs',
      'Implemented CRUD operations and optimized database queries',
      'Wrote unit tests and supported CI-friendly development practices',
      'Assisted in debugging and improving existing application workflows'
    ]
  }
];

export const education: Education[] = [
  {
    degree: 'Master of Computer Science',
    institution: 'Illinois Institute of Technology',
    location: 'USA',
    startDate: 'Jan 2023',
    endDate: 'Dec 2025'
  },
  {
    degree: 'Bachelor of Computer Engineering',
    institution: 'Gujarat Technological University',
    location: 'India',
    startDate: 'Jun 2019',
    endDate: 'May 2023'
  }
];

export const featuredProjects = projects.slice(0, 3);
