import React from 'react';
import './Skeleton.css';

export type SkeletonVariant = 'text' | 'avatar' | 'block' | 'row';

export interface SkeletonProps extends React.HTMLAttributes<HTMLDivElement> {
  variant?: SkeletonVariant;
  width?: string | number;
  height?: string | number;
}

export const Skeleton: React.FC<SkeletonProps> = ({
  variant = 'block',
  width,
  height,
  className = '',
  style,
  ...props
}) => {
  const customStyle: React.CSSProperties = {
    width,
    height,
    ...style,
  };

  return (
    <div
      className={`skeleton skeleton-${variant} ${className}`}
      style={customStyle}
      {...props}
    />
  );
};
export default Skeleton;
