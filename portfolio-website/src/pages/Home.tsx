import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Helmet } from 'react-helmet-async';
import { Download, Mail, ExternalLink } from 'lucide-react';
import { Layout } from '../components/Layout';
import { Button } from '../components/Button';
import { ProjectCard } from '../components/ProjectCard';
import { personalInfo, featuredProjects } from '../data/portfolio';
import { getTechColor } from '../utils/helpers';

export const Home = () => {
  const allSkills = [
    ...personalInfo.skills.languages,
    ...personalInfo.skills.frontend,
    ...personalInfo.skills.backend,
    ...personalInfo.skills.databases,
    ...personalInfo.skills.tools,
  ];

  return (
    <Layout>
      <Helmet>
        <title>Ruchika Rajodiya - Software Engineer | Full-Stack & Backend Developer</title>
        <meta
          name="description"
          content="Portfolio of Ruchika Rajodiya - Software Engineer specializing in full-stack and backend development. Experience with React, .NET, Python, and machine learning."
        />
        <meta property="og:title" content="Ruchika Rajodiya - Software Engineer" />
        <meta property="og:description" content={personalInfo.tagline} />
        <meta property="og:type" content="website" />
      </Helmet>
      {/* Hero Section */}
      <section className="container-custom py-20 md:py-32">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-12 items-center">
          <motion.div
            initial={{ opacity: 0, x: -50 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ duration: 0.6 }}
          >
            <div className="mb-6">
              <div className="w-32 h-32 rounded-full bg-gray-200 dark:bg-gray-700 mb-6 mx-auto md:mx-0 overflow-hidden relative flex items-center justify-center">
                <img
                  src="/profile.jpg"
                  alt="Ruchika Rajodiya"
                  className="w-full h-full object-cover"
                  onError={(e) => {
                    const el = e.target as HTMLImageElement;
                    el.style.display = 'none';
                    const span = el.nextElementSibling as HTMLElement | null;
                    if (span) span.classList.remove('hidden');
                  }}
                />
                <span className="hidden absolute inset-0 flex items-center justify-center text-gray-500 dark:text-gray-400 text-sm pointer-events-none">
                  Profile Image
                </span>
              </div>
            </div>
            <h1 className="text-4xl md:text-5xl font-bold mb-4 text-gray-900 dark:text-gray-100">
              {personalInfo.name}
            </h1>
            <p className="text-xl text-gray-600 dark:text-gray-400 mb-6">
              {personalInfo.title}
            </p>
            <p className="text-lg text-gray-700 dark:text-gray-300 mb-8">
              {personalInfo.tagline}
            </p>
            <div className="flex flex-wrap gap-4">
              <Link to="/projects">
                <Button size="lg">View Projects</Button>
              </Link>
              <Link to="/resume">
                <Button variant="outline" size="lg" className="flex items-center gap-2">
                  <Download className="w-5 h-5" />
                  Download Resume
                </Button>
              </Link>
              <Link to="/contact">
                <Button variant="secondary" size="lg" className="flex items-center gap-2">
                  <Mail className="w-5 h-5" />
                  Contact
                </Button>
              </Link>
            </div>
          </motion.div>

          <motion.div
            initial={{ opacity: 0, x: 50 }}
            animate={{ opacity: 1, x: 0 }}
            transition={{ duration: 0.6, delay: 0.2 }}
            className="space-y-6"
          >
            <div>
              <h2 className="text-2xl font-semibold mb-4 text-gray-900 dark:text-gray-100">
                Tech Stack
              </h2>
              <div className="flex flex-wrap gap-2">
                {allSkills.slice(0, 16).map((skill) => (
                  <span
                    key={skill}
                    className={`px-3 py-1 text-sm rounded-full ${getTechColor(skill)}`}
                  >
                    {skill}
                  </span>
                ))}
              </div>
            </div>
          </motion.div>
        </div>
      </section>

      {/* Featured Projects */}
      <section className="container-custom py-16">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          transition={{ duration: 0.6 }}
        >
          <h2 className="text-3xl font-bold mb-4 text-center text-gray-900 dark:text-gray-100">
            Featured Projects
          </h2>
          <p className="text-gray-600 dark:text-gray-400 text-center mb-12">
            A selection of my recent work
          </p>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            {featuredProjects.map((project, index) => (
              <motion.div
                key={project.id}
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ duration: 0.4, delay: index * 0.1 }}
              >
                <ProjectCard project={project} />
              </motion.div>
            ))}
          </div>
          <div className="text-center mt-12">
            <Link to="/projects">
              <Button variant="outline" size="lg" className="flex items-center gap-2 mx-auto">
                View All Projects
                <ExternalLink className="w-5 h-5" />
              </Button>
            </Link>
          </div>
        </motion.div>
      </section>

      {/* About Section */}
      <section className="bg-gray-50 dark:bg-gray-900/50 py-16">
        <div className="container-custom">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ duration: 0.6 }}
          >
            <h2 className="text-3xl font-bold mb-8 text-center text-gray-900 dark:text-gray-100">
              About Me
            </h2>
            <div className="max-w-3xl mx-auto space-y-4 text-gray-700 dark:text-gray-300">
              {personalInfo.about.map((paragraph, index) => (
                <p key={index} className="text-lg leading-relaxed">
                  {paragraph}
                </p>
              ))}
            </div>
          </motion.div>
        </div>
      </section>
    </Layout>
  );
};
