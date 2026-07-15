import React from 'react';
import { Link } from 'react-router-dom';
import { ChevronRightIcon } from '../Icons';
import './Breadcrumbs.css';

export interface BreadcrumbItem {
  label: string;
  href?: string;
  icon?: React.ReactNode;
}

export interface BreadcrumbsProps {
  items: BreadcrumbItem[];
  className?: string;
}

export const Breadcrumbs: React.FC<BreadcrumbsProps> = ({ items, className = '' }) => {
  if (!items || items.length === 0) return null;

  return (
    <nav className={`breadcrumbs ${className}`} aria-label="Breadcrumb">
      <ol className="breadcrumbs-list">
        {items.map((item, index) => {
          const isLast = index === items.length - 1;

          return (
            <li key={index} className="breadcrumbs-item">
              {item.href && !isLast ? (
                <Link to={item.href} className="breadcrumbs-link">
                  {item.icon && <span className="breadcrumbs-icon">{item.icon}</span>}
                  <span className="breadcrumbs-text">{item.label}</span>
                </Link>
              ) : (
                <span className="breadcrumbs-current" aria-current={isLast ? 'page' : undefined}>
                  {item.icon && <span className="breadcrumbs-icon">{item.icon}</span>}
                  <span className="breadcrumbs-text">{item.label}</span>
                </span>
              )}

              {!isLast && (
                <ChevronRightIcon size={14} className="breadcrumbs-separator" />
              )}
            </li>
          );
        })}
      </ol>
    </nav>
  );
};
