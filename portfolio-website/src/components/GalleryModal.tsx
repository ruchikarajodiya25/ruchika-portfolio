import { useState, useEffect, useCallback } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { X, ChevronLeft, ChevronRight } from 'lucide-react';

interface GalleryModalProps {
  isOpen: boolean;
  onClose: () => void;
  images: string[];
  title: string;
  initialIndex?: number;
}

export const GalleryModal = ({
  isOpen,
  onClose,
  images,
  title,
  initialIndex = 0,
}: GalleryModalProps) => {
  const [currentIndex, setCurrentIndex] = useState(initialIndex);

  const goPrev = useCallback(() => {
    setCurrentIndex((i) => (i <= 0 ? images.length - 1 : i - 1));
  }, [images.length]);

  const goNext = useCallback(() => {
    setCurrentIndex((i) => (i >= images.length - 1 ? 0 : i + 1));
  }, [images.length]);

  const handleKeyDown = useCallback(
    (e: KeyboardEvent) => {
      if (!isOpen) return;
      if (e.key === 'Escape') onClose();
      if (e.key === 'ArrowLeft') goPrev();
      if (e.key === 'ArrowRight') goNext();
    },
    [isOpen, onClose, goPrev, goNext]
  );

  useEffect(() => {
    setCurrentIndex(initialIndex);
  }, [initialIndex, isOpen]);

  useEffect(() => {
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [handleKeyDown]);

  useEffect(() => {
    if (isOpen) document.body.style.overflow = 'hidden';
    else document.body.style.overflow = '';
    return () => {
      document.body.style.overflow = '';
    };
  }, [isOpen]);

  return (
    <AnimatePresence>
      {isOpen && (
        <motion.div
          key="gallery-modal"
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          transition={{ duration: 0.2 }}
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/90 p-4"
          onClick={onClose}
          role="dialog"
          aria-modal="true"
          aria-label={`${title} screenshots gallery`}
        >
        <button
          type="button"
          onClick={(e) => { e.stopPropagation(); onClose(); }}
          className="absolute top-4 right-4 p-2 rounded-full bg-white/10 hover:bg-white/20 text-white transition-colors z-10"
          aria-label="Close gallery"
        >
          <X className="w-6 h-6" />
        </button>

        <div
          className="relative max-w-5xl w-full flex items-center gap-4"
          onClick={(e) => e.stopPropagation()}
        >
          <button
            type="button"
            onClick={(e) => { e.stopPropagation(); goPrev(); }}
            className="p-2 rounded-full bg-white/10 hover:bg-white/20 text-white transition-colors flex-shrink-0"
            aria-label="Previous image"
          >
            <ChevronLeft className="w-8 h-8" />
          </button>

          <div className="flex-1 flex flex-col items-center min-w-0">
            <motion.img
              key={currentIndex}
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              src={images[currentIndex]}
              alt={`${title} screenshot ${currentIndex + 1}`}
              className="max-h-[80vh] max-w-full object-contain rounded-lg"
            />
            <p className="mt-2 text-white/80 text-sm">
              {currentIndex + 1} / {images.length}
            </p>
          </div>

          <button
            type="button"
            onClick={(e) => { e.stopPropagation(); goNext(); }}
            className="p-2 rounded-full bg-white/10 hover:bg-white/20 text-white transition-colors flex-shrink-0"
            aria-label="Next image"
          >
            <ChevronRight className="w-8 h-8" />
          </button>
        </div>
        </motion.div>
      )}
    </AnimatePresence>
  );
};
