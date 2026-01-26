import { useState } from 'react';
import type { Project } from '../data/portfolio';
import { Card } from './Card';
import { Link } from 'react-router-dom';
import { ExternalLink, Github, ImageIcon } from 'lucide-react';
import { getTechColor, hasValidGithub, hasValidLive } from '../utils/helpers';
import { GalleryModal } from './GalleryModal';

interface ProjectCardProps {
  project: Project;
}

export const ProjectCard = ({ project }: ProjectCardProps) => {
  const [galleryOpen, setGalleryOpen] = useState(false);
  const hasScreenshots = Boolean(project.screenshots && project.screenshots.length > 0);
  const firstScreenshot = hasScreenshots ? project.screenshots![0] : null;

  return (
    <Card hover className="h-full flex flex-col">
      <div className="flex-1">
        {firstScreenshot && (
          <div className="w-full h-48 rounded-lg mb-4 overflow-hidden bg-gray-200 dark:bg-gray-700">
            <img
              src={firstScreenshot}
              alt={project.title}
              className="w-full h-full object-cover"
              loading="lazy"
            />
          </div>
        )}

        <h3 className="text-xl font-semibold mb-2 text-gray-900 dark:text-gray-100">
          {project.title}
        </h3>
        <p className="text-gray-600 dark:text-gray-400 text-sm mb-4 line-clamp-2">
          {project.summary}
        </p>

        <div className="flex flex-wrap gap-2 mb-4">
          {project.tags.slice(0, 3).map((tag) => (
            <span
              key={tag}
              className={`px-2 py-1 text-xs rounded-full ${getTechColor(tag)}`}
            >
              {tag}
            </span>
          ))}
          {project.tags.length > 3 && (
            <span className="px-2 py-1 text-xs rounded-full bg-gray-100 text-gray-600 dark:bg-gray-700 dark:text-gray-400">
              +{project.tags.length - 3}
            </span>
          )}
        </div>
      </div>

      <div className="flex flex-wrap items-center justify-between gap-2 pt-4 border-t border-gray-200 dark:border-gray-700">
        <Link
          to={`/projects/${project.slug}`}
          className="text-primary-600 dark:text-primary-400 hover:text-primary-700 dark:hover:text-primary-300 font-medium text-sm flex items-center gap-1"
        >
          View Details
          <ExternalLink className="w-4 h-4" />
        </Link>
        <div className="flex items-center gap-2">
          {hasScreenshots && (
            <button
              type="button"
              onClick={() => setGalleryOpen(true)}
              className="px-3 py-1.5 rounded-lg text-sm font-medium bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-300 dark:hover:bg-gray-600 transition-colors flex items-center gap-1"
              aria-label="View screenshots"
            >
              <ImageIcon className="w-4 h-4" />
              View Screenshots
            </button>
          )}
          {hasValidGithub(project.github) && (
            <a
              href={project.github}
              target="_blank"
              rel="noopener noreferrer"
              className="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
              aria-label="GitHub"
            >
              <Github className="w-4 h-4 text-gray-600 dark:text-gray-400" />
            </a>
          )}
          {hasValidLive(project.live) && (
            <a
              href={project.live}
              target="_blank"
              rel="noopener noreferrer"
              className="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
              aria-label="Live Demo"
            >
              <ExternalLink className="w-4 h-4 text-gray-600 dark:text-gray-400" />
            </a>
          )}
        </div>
      </div>

      {hasScreenshots && (
        <GalleryModal
          isOpen={galleryOpen}
          onClose={() => setGalleryOpen(false)}
          images={project.screenshots!}
          title={project.title}
        />
      )}
    </Card>
  );
};
