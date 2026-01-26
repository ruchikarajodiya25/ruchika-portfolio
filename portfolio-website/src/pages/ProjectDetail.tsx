import { useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Helmet } from 'react-helmet-async';
import { ArrowLeft, Github, ExternalLink, Calendar, User, CheckCircle, ImageIcon } from 'lucide-react';
import { Layout } from '../components/Layout';
import { Button } from '../components/Button';
import { Card } from '../components/Card';
import { GalleryModal } from '../components/GalleryModal';
import { projects } from '../data/portfolio';
import { getTechColor, hasValidGithub, hasValidLive } from '../utils/helpers';

export const ProjectDetail = () => {
  const { slug } = useParams<{ slug: string }>();
  const project = projects.find((p) => p.slug === slug);
  const [galleryOpen, setGalleryOpen] = useState(false);
  const hasScreenshots = Boolean(project?.screenshots && project.screenshots.length > 0);

  if (!project) {
    return (
      <Layout>
        <div className="container-custom py-12 text-center">
          <h1 className="text-3xl font-bold mb-4 text-gray-900 dark:text-gray-100">
            Project Not Found
          </h1>
          <Link to="/projects">
            <Button>Back to Projects</Button>
          </Link>
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      <Helmet>
        <title>{project.title} - Ruchika Rajodiya</title>
        <meta name="description" content={project.summary} />
      </Helmet>
      <div className="container-custom py-12">
        {/* Back Button */}
        <Link
          to="/projects"
          className="inline-flex items-center gap-2 text-gray-600 dark:text-gray-400 hover:text-primary-600 dark:hover:text-primary-400 mb-8 transition-colors"
        >
          <ArrowLeft className="w-5 h-5" />
          Back to Projects
        </Link>

        {/* Header */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6 }}
          className="mb-8"
        >
          <div className="flex flex-wrap items-center gap-4 mb-4">
            <h1 className="text-4xl font-bold text-gray-900 dark:text-gray-100">
              {project.title}
            </h1>
            <span
              className={`px-3 py-1 rounded-full text-sm font-medium ${
                project.status === 'completed'
                  ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                  : project.status === 'in-progress'
                  ? 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200'
                  : 'bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-200'
              }`}
            >
              {project.status.charAt(0).toUpperCase() + project.status.slice(1)}
            </span>
          </div>

          <div className="flex flex-wrap items-center gap-6 text-gray-600 dark:text-gray-400 mb-6">
            <div className="flex items-center gap-2">
              <User className="w-5 h-5" />
              <span>{project.role}</span>
            </div>
            <div className="flex items-center gap-2">
              <Calendar className="w-5 h-5" />
              <span>{project.date}</span>
            </div>
          </div>

          <p className="text-lg text-gray-700 dark:text-gray-300 mb-6">
            {project.description}
          </p>

          <div className="flex flex-wrap gap-4">
            {hasValidGithub(project.github) && (
              <a
                href={project.github}
                target="_blank"
                rel="noopener noreferrer"
              >
                <Button variant="outline" className="flex items-center gap-2">
                  <Github className="w-5 h-5" />
                  GitHub Repo
                </Button>
              </a>
            )}
            {hasValidLive(project.live) && (
              <a href={project.live} target="_blank" rel="noopener noreferrer">
                <Button className="flex items-center gap-2">
                  <ExternalLink className="w-5 h-5" />
                  Live Demo
                </Button>
              </a>
            )}
            {hasScreenshots && (
              <Button
                type="button"
                variant="secondary"
                className="flex items-center gap-2"
                onClick={() => setGalleryOpen(true)}
              >
                <ImageIcon className="w-5 h-5" />
                View Screenshots
              </Button>
            )}
          </div>
        </motion.div>

        {/* Tech Stack */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6, delay: 0.3 }}
          className="mb-12"
        >
          <h2 className="text-2xl font-semibold mb-6 text-gray-900 dark:text-gray-100">
            Tech Stack
          </h2>
          <div className="flex flex-wrap gap-2">
            {project.tags.map((tag) => (
              <span
                key={tag}
                className={`px-3 py-1 rounded-full text-sm ${getTechColor(tag)}`}
              >
                {tag}
              </span>
            ))}
          </div>
        </motion.div>

        {/* Features */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6, delay: 0.4 }}
          className="mb-12"
        >
          <Card>
            <h2 className="text-2xl font-semibold mb-6 text-gray-900 dark:text-gray-100">
              Features
            </h2>
            <ul className="space-y-3">
              {project.features.map((feature, index) => (
                <li key={index} className="flex items-start gap-3">
                  <CheckCircle className="w-5 h-5 text-primary-600 dark:text-primary-400 mt-0.5 flex-shrink-0" />
                  <span className="text-gray-700 dark:text-gray-300">{feature}</span>
                </li>
              ))}
            </ul>
          </Card>
        </motion.div>

        {/* Frontend & Backend */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-8 mb-12">
          {/* Frontend */}
          {project.frontend.stack.length > 0 && (
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.6, delay: 0.5 }}
            >
              <Card>
                <h2 className="text-2xl font-semibold mb-4 text-gray-900 dark:text-gray-100">
                  Frontend
                </h2>
                <div className="mb-4">
                  <h3 className="font-medium mb-2 text-gray-800 dark:text-gray-200">
                    Stack:
                  </h3>
                  <div className="flex flex-wrap gap-2 mb-4">
                    {project.frontend.stack.map((tech) => (
                      <span
                        key={tech}
                        className={`px-2 py-1 text-xs rounded-full ${getTechColor(tech)}`}
                      >
                        {tech}
                      </span>
                    ))}
                  </div>
                </div>
                <div>
                  <h3 className="font-medium mb-2 text-gray-800 dark:text-gray-200">
                    Implementation:
                  </h3>
                  <ul className="space-y-2">
                    {project.frontend.details.map((detail, index) => (
                      <li
                        key={index}
                        className="text-sm text-gray-600 dark:text-gray-400 flex items-start gap-2"
                      >
                        <span className="text-primary-600 dark:text-primary-400 mt-1">•</span>
                        <span>{detail}</span>
                      </li>
                    ))}
                  </ul>
                </div>
              </Card>
            </motion.div>
          )}

          {/* Backend */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6, delay: 0.6 }}
          >
            <Card>
              <h2 className="text-2xl font-semibold mb-4 text-gray-900 dark:text-gray-100">
                Backend
              </h2>
              <div className="mb-4">
                <h3 className="font-medium mb-2 text-gray-800 dark:text-gray-200">Stack:</h3>
                <div className="flex flex-wrap gap-2 mb-4">
                  {project.backend.stack.map((tech) => (
                    <span
                      key={tech}
                      className={`px-2 py-1 text-xs rounded-full ${getTechColor(tech)}`}
                    >
                      {tech}
                    </span>
                  ))}
                </div>
              </div>
              <div>
                <h3 className="font-medium mb-2 text-gray-800 dark:text-gray-200">
                  Implementation:
                </h3>
                <ul className="space-y-2">
                  {project.backend.details.map((detail, index) => (
                    <li
                      key={index}
                      className="text-sm text-gray-600 dark:text-gray-400 flex items-start gap-2"
                    >
                      <span className="text-primary-600 dark:text-primary-400 mt-1">•</span>
                      <span>{detail}</span>
                    </li>
                  ))}
                </ul>
              </div>
            </Card>
          </motion.div>
        </div>

        {/* Architecture */}
        {project.architecture && project.architecture.length > 0 && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6, delay: 0.7 }}
            className="mb-12"
          >
            <Card>
              <h2 className="text-2xl font-semibold mb-4 text-gray-900 dark:text-gray-100">
                Architecture
              </h2>
              <ul className="space-y-2">
                {project.architecture.map((item, index) => (
                  <li
                    key={index}
                    className="text-gray-700 dark:text-gray-300 flex items-start gap-2"
                  >
                    <span className="text-primary-600 dark:text-primary-400 mt-1">•</span>
                    <span>{item}</span>
                  </li>
                ))}
              </ul>
            </Card>
          </motion.div>
        )}

        {/* Challenges & Solutions */}
        {project.challenges && project.challenges.length > 0 && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6, delay: 0.8 }}
            className="mb-12"
          >
            <Card>
              <h2 className="text-2xl font-semibold mb-4 text-gray-900 dark:text-gray-100">
                Challenges & Solutions
              </h2>
              <ul className="space-y-3">
                {project.challenges.map((challenge, index) => (
                  <li
                    key={index}
                    className="text-gray-700 dark:text-gray-300 flex items-start gap-2"
                  >
                    <span className="text-primary-600 dark:text-primary-400 mt-1">•</span>
                    <span>{challenge}</span>
                  </li>
                ))}
              </ul>
            </Card>
          </motion.div>
        )}

        {/* Results */}
        {project.results && project.results.length > 0 && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6, delay: 0.9 }}
          >
            <Card>
              <h2 className="text-2xl font-semibold mb-4 text-gray-900 dark:text-gray-100">
                Results
              </h2>
              <ul className="space-y-3">
                {project.results.map((result, index) => (
                  <li
                    key={index}
                    className="text-gray-700 dark:text-gray-300 flex items-start gap-2"
                  >
                    <CheckCircle className="w-5 h-5 text-green-600 dark:text-green-400 mt-0.5 flex-shrink-0" />
                    <span>{result}</span>
                  </li>
                ))}
              </ul>
            </Card>
          </motion.div>
        )}
      </div>

      {hasScreenshots && (
        <GalleryModal
          isOpen={galleryOpen}
          onClose={() => setGalleryOpen(false)}
          images={project.screenshots!}
          title={project.title}
        />
      )}
    </Layout>
  );
};
