import React from 'react';
import { Card } from '../Card';
import './EntitySummary.css';

export interface EntitySummaryProps {
  name: string;
  documentInfo?: string;
  avatarIcon?: React.ReactNode;
  badges?: React.ReactNode;
  secondaryInfo?: React.ReactNode;
  className?: string;
}

export const EntitySummary: React.FC<EntitySummaryProps> = ({
  name,
  documentInfo,
  avatarIcon,
  badges,
  secondaryInfo,
  className = '',
}) => {
  return (
    <Card className={`entity-summary ${className}`}>
      <div className="entity-summary-content">
        {/* Avatar Area */}
        {avatarIcon && (
          <div className="entity-summary-avatar">
            {avatarIcon}
          </div>
        )}

        {/* Info Area */}
        <div className="entity-summary-info">
          <div className="entity-summary-header">
            <div className="entity-summary-title-group">
              <h2 className="entity-summary-title" title={name}>
                {name}
              </h2>
              {documentInfo && (
                <p className="entity-summary-document" title={documentInfo}>
                  {documentInfo}
                </p>
              )}
            </div>
            
            {/* Badges / Status Area */}
            {badges && (
              <div className="entity-summary-badges">
                {badges}
              </div>
            )}
          </div>

          {/* Secondary Info Area (e.g. key-value pairs) */}
          {secondaryInfo && (
            <div className="entity-summary-secondary">
              {secondaryInfo}
            </div>
          )}
        </div>
      </div>
    </Card>
  );
};
