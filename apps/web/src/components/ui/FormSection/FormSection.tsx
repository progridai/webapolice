import React from 'react';
import './FormSection.css';

export interface FormSectionProps {
  title: string;
  description?: string;
  icon?: React.ReactNode;
  action?: React.ReactNode;
  children: React.ReactNode;
  className?: string;
}

export const FormSection: React.FC<FormSectionProps> = ({
  title,
  description,
  icon,
  action,
  children,
  className = ''
}) => {
  return (
    <section className={`form-section ${className}`}>
      <div className="form-section-header">
        <div className="form-section-title-group">
          {icon && <div className="form-section-icon" aria-hidden="true">{icon}</div>}
          <div className="form-section-text">
            <h3 className="form-section-title">{title}</h3>
            {description && <p className="form-section-description">{description}</p>}
          </div>
        </div>
        {action && <div className="form-section-action">{action}</div>}
      </div>
      <div className="form-section-content">
        {children}
      </div>
    </section>
  );
};
