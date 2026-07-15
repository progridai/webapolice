import React from 'react';
import { Card } from '../Card';

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
    <Card className={`p-6 flex flex-col gap-4 ${className}`}>
      <div className="flex flex-col sm:flex-row sm:items-start gap-4">
        {/* Avatar Area */}
        {avatarIcon && (
          <div className="flex-shrink-0 w-16 h-16 rounded-full bg-slate-100 dark:bg-slate-800 flex items-center justify-center text-slate-400 dark:text-slate-500 border border-slate-200 dark:border-slate-700">
            {avatarIcon}
          </div>
        )}

        {/* Info Area */}
        <div className="flex-grow flex flex-col gap-2 min-w-0">
          <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <div className="min-w-0">
              <h2 className="text-2xl font-bold text-slate-900 dark:text-slate-50 truncate mb-1">
                {name}
              </h2>
              {documentInfo && (
                <p className="text-sm text-slate-500 dark:text-slate-400 truncate">
                  {documentInfo}
                </p>
              )}
            </div>
            
            {/* Badges / Status Area */}
            {badges && (
              <div className="flex flex-wrap gap-2 flex-shrink-0">
                {badges}
              </div>
            )}
          </div>

          {/* Secondary Info Area (e.g. key-value pairs) */}
          {secondaryInfo && (
            <div className="pt-4 mt-2 border-t border-slate-200 dark:border-slate-700">
              {secondaryInfo}
            </div>
          )}
        </div>
      </div>
    </Card>
  );
};
