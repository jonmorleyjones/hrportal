import { motion, AnimatePresence, type Variants } from 'framer-motion';
import { type ReactNode } from 'react';

// Animation variants
export const fadeIn: Variants = {
  initial: { opacity: 0 },
  animate: { opacity: 1 },
  exit: { opacity: 0 },
};

export const fadeInUp: Variants = {
  initial: { opacity: 0, y: 20 },
  animate: { opacity: 1, y: 0 },
  exit: { opacity: 0, y: 20 },
};

export const fadeInDown: Variants = {
  initial: { opacity: 0, y: -20 },
  animate: { opacity: 1, y: 0 },
  exit: { opacity: 0, y: -20 },
};

export const slideInLeft: Variants = {
  initial: { opacity: 0, x: -20 },
  animate: { opacity: 1, x: 0 },
  exit: { opacity: 0, x: -20 },
};

export const slideInRight: Variants = {
  initial: { opacity: 0, x: 20 },
  animate: { opacity: 1, x: 0 },
  exit: { opacity: 0, x: 20 },
};

export const scaleIn: Variants = {
  initial: { opacity: 0, scale: 0.95 },
  animate: { opacity: 1, scale: 1 },
  exit: { opacity: 0, scale: 0.95 },
};

export const staggerContainer: Variants = {
  initial: {},
  animate: {
    transition: {
      staggerChildren: 0.1,
      delayChildren: 0.1,
    },
  },
};

export const staggerItem: Variants = {
  initial: { opacity: 0, y: 20 },
  animate: {
    opacity: 1,
    y: 0,
    transition: {
      duration: 0.4,
      ease: [0.4, 0, 0.2, 1],
    },
  },
};

// Transition presets
export const springTransition = {
  type: 'spring',
  stiffness: 300,
  damping: 30,
};

export const smoothTransition = {
  duration: 0.4,
  ease: [0.4, 0, 0.2, 1],
};

export const fastTransition = {
  duration: 0.2,
  ease: [0.4, 0, 0.2, 1],
};

// Page transition wrapper
interface PageTransitionProps {
  children: ReactNode;
  className?: string;
}

export function PageTransition({ children, className }: PageTransitionProps) {
  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: 20 }}
      transition={smoothTransition}
      className={className}
    >
      {children}
    </motion.div>
  );
}

// Staggered list container
interface StaggerContainerProps {
  children: ReactNode;
  className?: string;
  delay?: number;
}

export function StaggerContainer({ children, className, delay = 0.1 }: StaggerContainerProps) {
  return (
    <motion.div
      initial="initial"
      animate="animate"
      exit="exit"
      variants={{
        initial: {},
        animate: {
          transition: {
            staggerChildren: delay,
            delayChildren: 0.1,
          },
        },
      }}
      className={className}
    >
      {children}
    </motion.div>
  );
}

// Staggered list item
interface StaggerItemProps {
  children: ReactNode;
  className?: string;
}

export function StaggerItem({ children, className }: StaggerItemProps) {
  return (
    <motion.div
      variants={staggerItem}
      className={className}
    >
      {children}
    </motion.div>
  );
}

// Hover card effect
interface HoverCardProps {
  children: ReactNode;
  className?: string;
}

export function HoverCard({ children, className }: HoverCardProps) {
  return (
    <motion.div
      whileHover={{
        y: -4,
        scale: 1.02,
        transition: fastTransition,
      }}
      whileTap={{ scale: 0.98 }}
      className={className}
    >
      {children}
    </motion.div>
  );
}

// Skeleton loader
interface SkeletonProps {
  className?: string;
}

export function Skeleton({ className }: SkeletonProps) {
  return (
    <motion.div
      className={`rounded-lg bg-muted shimmer ${className}`}
      initial={{ opacity: 0.5 }}
      animate={{ opacity: 1 }}
      transition={{
        repeat: Infinity,
        repeatType: 'reverse',
        duration: 1,
      }}
    />
  );
}

// Spinning loader
interface SpinnerProps {
  size?: 'sm' | 'md' | 'lg';
  className?: string;
}

export function Spinner({ size = 'md', className }: SpinnerProps) {
  const sizeClasses = {
    sm: 'w-4 h-4',
    md: 'w-6 h-6',
    lg: 'w-8 h-8',
  };

  return (
    <motion.div
      className={`rounded-full border-2 border-muted border-t-primary ${sizeClasses[size]} ${className}`}
      animate={{ rotate: 360 }}
      transition={{
        duration: 1,
        repeat: Infinity,
        ease: 'linear',
      }}
    />
  );
}

// Animated counter
interface AnimatedCounterProps {
  value: number;
  className?: string;
}

export function AnimatedCounter({ value, className }: AnimatedCounterProps) {
  return (
    <motion.span
      key={value}
      initial={{ opacity: 0, y: 10 }}
      animate={{ opacity: 1, y: 0 }}
      transition={springTransition}
      className={className}
    >
      {value}
    </motion.span>
  );
}

// Animated presence wrapper
interface AnimatedPresenceWrapperProps {
  children: ReactNode;
  show: boolean;
}

export function AnimatedPresenceWrapper({ children, show }: AnimatedPresenceWrapperProps) {
  return (
    <AnimatePresence mode="wait">
      {show && (
        <motion.div
          initial={{ opacity: 0, scale: 0.95 }}
          animate={{ opacity: 1, scale: 1 }}
          exit={{ opacity: 0, scale: 0.95 }}
          transition={smoothTransition}
        >
          {children}
        </motion.div>
      )}
    </AnimatePresence>
  );
}

// Gradient background with floating orbs
export function GradientBackground() {
  return (
    <div className="gradient-bg">
      <motion.div
        className="floating-orb orb-1"
        animate={{
          x: [0, 20, -10, 30, 0],
          y: [0, -30, 20, 10, 0],
          scale: [1, 1.05, 0.95, 1.02, 1],
        }}
        transition={{
          duration: 15,
          repeat: Infinity,
          ease: 'easeInOut',
        }}
      />
      <motion.div
        className="floating-orb orb-2"
        animate={{
          x: [0, -20, 30, -10, 0],
          y: [0, 20, -30, 15, 0],
          scale: [1, 0.95, 1.05, 0.98, 1],
        }}
        transition={{
          duration: 18,
          repeat: Infinity,
          ease: 'easeInOut',
          delay: 2,
        }}
      />
      <motion.div
        className="floating-orb orb-3"
        animate={{
          x: [0, 30, -20, 15, 0],
          y: [0, -20, 30, -15, 0],
          scale: [1, 1.02, 0.98, 1.05, 1],
        }}
        transition={{
          duration: 20,
          repeat: Infinity,
          ease: 'easeInOut',
          delay: 4,
        }}
      />
    </div>
  );
}

// Glass card component
interface GlassCardProps {
  children: ReactNode;
  className?: string;
  hover?: boolean;
}

export function GlassCard({ children, className, hover = true }: GlassCardProps) {
  const Component = hover ? motion.div : 'div';

  return (
    <Component
      className={`glass rounded-xl ${className}`}
      {...(hover && {
        whileHover: {
          y: -2,
          boxShadow: '0 20px 40px 0 rgba(0, 0, 0, 0.4)',
        },
        transition: fastTransition,
      })}
    >
      {children}
    </Component>
  );
}

export { motion, AnimatePresence };
