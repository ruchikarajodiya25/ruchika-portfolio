import { Project } from '../data/portfolio';
import { Card } from './Card';
import { Link } from 'react-router-dom';
import { ExternalLink, Github } from 'lucide-react';
import { getTechColor } from '../utils/helpers';

interface ProjectCardProps {
  project: Project;
}

export const ProjectCard = ({ project }: ProjectCardProps) => {
  return (
    <Card hover className="h-full flex flex-col">
      <div className="flex-1">
        {/* Screenshot */}
        <div className="w-full h-48 bg-gray-200 dark:bg-gray-700 rounded-lg mb-4 overflow-hidden relative">
          {project.screenshots && project.screenshots.length > 0 ? (
            <img
              src={project.screenshots[0]}
              alt={project.title}
              className="w-full h-full object-cover"
              loading="lazy"
              onError={(e) => {
                const target = e.target as HTMLImageElement;
                target.style.display = 'none';
              }}
            />
          ) : null}
          <div className="absolute inset-0 flex items-center justify-center bg-gray-200 dark:bg-gray-700">
            <span className="text-gray-500 dark:text-gray-400 text-sm">
              {project.title}
            </span>
          </div>
        </div>

        <h3 className="text-xl font-semibold mb-2 text-gray-900 dark:text-gray-100">
          {project.title}
        </h3>
        <p className="text-gray-600 dark:text-gray-400 text-sm mb-4 line-clamp-2">
          {project.summary}
        </p>

        {/* Tech Tags */}
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

      <div className="flex items-center justify-between pt-4 border-t border-gray-200 dark:border-gray-700">
        <Link
          to={`/projects/${project.slug}`}
          className="text-primary-600 dark:text-primary-400 hover:text-primary-700 dark:hover:text-primary-300 font-medium text-sm flex items-center gap-1"
        >
          View Details
          <ExternalLink className="w-4 h-4" />
        </Link>
        <div className="flex gap-2">
          <a
            href={project.github}
            target="_blank"
            rel="noopener noreferrer"
            className="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
            aria-label="GitHub"
          >
            <Github className="w-4 h-4 text-gray-600 dark:text-gray-400" />
          </a>
          {project.live !== 'https://REPLACE_ME' && (
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
    </Card>
  );
};
