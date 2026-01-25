export const scrollToSection = (id: string) => {
  const element = document.getElementById(id);
  if (element) {
    const offset = 80; // Account for sticky navbar
    const elementPosition = element.getBoundingClientRect().top;
    const offsetPosition = elementPosition + window.pageYOffset - offset;

    window.scrollTo({
      top: offsetPosition,
      behavior: 'smooth'
    });
  }
};

export const formatDate = (dateString: string): string => {
  return dateString;
};

export const getTechColor = (tech: string): string => {
  const colors: Record<string, string> = {
    'React': 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200',
    'TypeScript': 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200',
    '.NET': 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200',
    'Python': 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200',
    'SQL Server': 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200',
    'PostgreSQL': 'bg-indigo-100 text-indigo-800 dark:bg-indigo-900 dark:text-indigo-200',
    'FastAPI': 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200',
    'Machine Learning': 'bg-pink-100 text-pink-800 dark:bg-pink-900 dark:text-pink-200',
  };
  return colors[tech] || 'bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-200';
};
