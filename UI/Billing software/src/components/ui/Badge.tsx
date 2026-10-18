import React from 'react';
import { clsx } from 'clsx';

export type BadgeVariant = 'success' | 'warning' | 'danger' | 'info' | 'neutral' | 'indigo' | 'purple' | 'amber';

interface BadgeProps {
  children: React.ReactNode;
  variant?: BadgeVariant;
  size?: 'sm' | 'md';
  className?: string;
}

export const Badge: React.FC<BadgeProps> = ({
  children,
  variant = 'neutral',
  size = 'md',
  className,
}) => {
  const variantStyles: Record<BadgeVariant, string> = {
    success: 'bg-emerald-50 text-emerald-700 border-emerald-200/80',
    warning: 'bg-amber-50 text-amber-700 border-amber-200/80',
    danger: 'bg-rose-50 text-rose-700 border-rose-200/80',
    info: 'bg-sky-50 text-sky-700 border-sky-200/80',
    neutral: 'bg-slate-100 text-slate-700 border-slate-200',
    indigo: 'bg-indigo-50 text-indigo-700 border-indigo-200',
    purple: 'bg-purple-50 text-purple-700 border-purple-200',
    amber: 'bg-amber-50 text-amber-700 border-amber-200',
  };

  const sizeStyles = {
    sm: 'px-1.5 py-0.5 text-xs font-medium',
    md: 'px-2.5 py-1 text-xs font-medium',
  };

  return (
    <span
      className={clsx(
        'inline-flex items-center rounded-md border font-sans leading-none transition-colors',
        variantStyles[variant],
        sizeStyles[size],
        className
      )}
    >
      {children}
    </span>
  );
};
