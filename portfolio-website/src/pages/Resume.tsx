import { motion } from 'framer-motion';
import { Helmet } from 'react-helmet-async';
import { Download, MapPin, Mail, Phone } from 'lucide-react';
import { Layout } from '../components/Layout';
import { Card } from '../components/Card';
import { Button } from '../components/Button';
import { personalInfo, experience, education } from '../data/portfolio';
import { getTechColor } from '../utils/helpers';

export const Resume = () => {
  const handleDownload = () => {
    // Create a link element and trigger download
    const link = document.createElement('a');
    link.href = '/resume.pdf';
    link.download = 'Ruchika_Rajodiya_Resume.pdf';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  return (
    <Layout>
      <Helmet>
        <title>Resume - Ruchika Rajodiya</title>
        <meta name="description" content="Download or view Ruchika Rajodiya's resume - Software Engineer with experience in full-stack and backend development." />
      </Helmet>
      <div className="container-custom py-12">
        {/* Header */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6 }}
          className="mb-8"
        >
          <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
            <div>
              <h1 className="text-4xl font-bold mb-2 text-gray-900 dark:text-gray-100">
                Resume
              </h1>
              <p className="text-gray-600 dark:text-gray-400">
                Download my resume or view it online
              </p>
            </div>
            <Button size="lg" onClick={handleDownload} className="flex items-center gap-2">
              <Download className="w-5 h-5" />
              Download PDF
            </Button>
          </div>
        </motion.div>

        {/* Contact Info */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6, delay: 0.1 }}
          className="mb-8"
        >
          <Card>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div className="flex items-center gap-3">
                <MapPin className="w-5 h-5 text-primary-600 dark:text-primary-400" />
                <span className="text-gray-700 dark:text-gray-300">{personalInfo.location}</span>
              </div>
              <div className="flex items-center gap-3">
                <Mail className="w-5 h-5 text-primary-600 dark:text-primary-400" />
                <a
                  href={`mailto:${personalInfo.email}`}
                  className="text-gray-700 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400 transition-colors"
                >
                  {personalInfo.email}
                </a>
              </div>
              <div className="flex items-center gap-3">
                <Phone className="w-5 h-5 text-primary-600 dark:text-primary-400" />
                <a
                  href={`tel:${personalInfo.phone}`}
                  className="text-gray-700 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400 transition-colors"
                >
                  {personalInfo.phone}
                </a>
              </div>
            </div>
          </Card>
        </motion.div>

        {/* Professional Summary */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6, delay: 0.2 }}
          className="mb-8"
        >
          <Card>
            <h2 className="text-2xl font-semibold mb-4 text-gray-900 dark:text-gray-100">
              Professional Summary
            </h2>
            <div className="space-y-4 text-gray-700 dark:text-gray-300">
              {personalInfo.about.map((paragraph, index) => (
                <p key={index} className="leading-relaxed">
                  {paragraph}
                </p>
              ))}
            </div>
          </Card>
        </motion.div>

        {/* Skills */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6, delay: 0.3 }}
          className="mb-8"
        >
          <Card>
            <h2 className="text-2xl font-semibold mb-6 text-gray-900 dark:text-gray-100">
              Technical Skills
            </h2>
            <div className="space-y-6">
              <div>
                <h3 className="font-semibold mb-3 text-gray-800 dark:text-gray-200">
                  Languages
                </h3>
                <div className="flex flex-wrap gap-2">
                  {personalInfo.skills.languages.map((skill) => (
                    <span
                      key={skill}
                      className={`px-3 py-1 rounded-full text-sm ${getTechColor(skill)}`}
                    >
                      {skill}
                    </span>
                  ))}
                </div>
              </div>
              <div>
                <h3 className="font-semibold mb-3 text-gray-800 dark:text-gray-200">
                  Frontend
                </h3>
                <div className="flex flex-wrap gap-2">
                  {personalInfo.skills.frontend.map((skill) => (
                    <span
                      key={skill}
                      className={`px-3 py-1 rounded-full text-sm ${getTechColor(skill)}`}
                    >
                      {skill}
                    </span>
                  ))}
                </div>
              </div>
              <div>
                <h3 className="font-semibold mb-3 text-gray-800 dark:text-gray-200">
                  Backend
                </h3>
                <div className="flex flex-wrap gap-2">
                  {personalInfo.skills.backend.map((skill) => (
                    <span
                      key={skill}
                      className={`px-3 py-1 rounded-full text-sm ${getTechColor(skill)}`}
                    >
                      {skill}
                    </span>
                  ))}
                </div>
              </div>
              <div>
                <h3 className="font-semibold mb-3 text-gray-800 dark:text-gray-200">
                  Databases
                </h3>
                <div className="flex flex-wrap gap-2">
                  {personalInfo.skills.databases.map((skill) => (
                    <span
                      key={skill}
                      className={`px-3 py-1 rounded-full text-sm ${getTechColor(skill)}`}
                    >
                      {skill}
                    </span>
                  ))}
                </div>
              </div>
              <div>
                <h3 className="font-semibold mb-3 text-gray-800 dark:text-gray-200">
                  Tools & Technologies
                </h3>
                <div className="flex flex-wrap gap-2">
                  {personalInfo.skills.tools.map((skill) => (
                    <span
                      key={skill}
                      className={`px-3 py-1 rounded-full text-sm ${getTechColor(skill)}`}
                    >
                      {skill}
                    </span>
                  ))}
                </div>
              </div>
            </div>
          </Card>
        </motion.div>

        {/* Experience */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6, delay: 0.4 }}
          className="mb-8"
        >
          <Card>
            <h2 className="text-2xl font-semibold mb-6 text-gray-900 dark:text-gray-100">
              Work Experience
            </h2>
            <div className="space-y-8">
              {experience.map((exp, index) => (
                <div key={index} className="border-l-4 border-primary-600 dark:border-primary-400 pl-6">
                  <div className="flex flex-col md:flex-row md:items-center md:justify-between mb-2">
                    <h3 className="text-xl font-semibold text-gray-900 dark:text-gray-100">
                      {exp.title}
                    </h3>
                    <span className="text-gray-600 dark:text-gray-400 text-sm">
                      {exp.startDate} - {exp.endDate}
                    </span>
                  </div>
                  <p className="text-primary-600 dark:text-primary-400 font-medium mb-3">
                    {exp.company} • {exp.location}
                  </p>
                  <ul className="space-y-2">
                    {exp.bullets.map((bullet, bulletIndex) => (
                      <li
                        key={bulletIndex}
                        className="text-gray-700 dark:text-gray-300 flex items-start gap-2"
                      >
                        <span className="text-primary-600 dark:text-primary-400 mt-1">•</span>
                        <span>{bullet}</span>
                      </li>
                    ))}
                  </ul>
                </div>
              ))}
            </div>
          </Card>
        </motion.div>

        {/* Education */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6, delay: 0.5 }}
          className="mb-8"
        >
          <Card>
            <h2 className="text-2xl font-semibold mb-6 text-gray-900 dark:text-gray-100">
              Education
            </h2>
            <div className="space-y-6">
              {education.map((edu, index) => (
                <div key={index} className="border-l-4 border-primary-600 dark:border-primary-400 pl-6">
                  <h3 className="text-xl font-semibold mb-1 text-gray-900 dark:text-gray-100">
                    {edu.degree}
                  </h3>
                  <p className="text-primary-600 dark:text-primary-400 font-medium mb-1">
                    {edu.institution}
                  </p>
                  <p className="text-gray-600 dark:text-gray-400 text-sm">
                    {edu.location} • {edu.startDate} - {edu.endDate}
                  </p>
                </div>
              ))}
            </div>
          </Card>
        </motion.div>
      </div>
    </Layout>
  );
};
