import React, { useState, useRef, useEffect, ReactNode } from 'react';
import './DropdownMenu.css';

export interface DropdownMenuProps {
  trigger: ReactNode;
  children: ReactNode;
  align?: 'left' | 'right';
}

export const DropdownMenu: React.FC<DropdownMenuProps> = ({
  trigger,
  children,
  align = 'right',
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    if (isOpen) {
      document.addEventListener('mousedown', handleClickOutside);
    }
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [isOpen]);

  const toggleDropdown = () => setIsOpen((prev) => !prev);

  // Close when clicking an item inside the menu
  const handleMenuClick = () => {
    setIsOpen(false);
  };

  return (
    <div className="dropdown-menu-container" ref={dropdownRef}>
      <div className="dropdown-menu-trigger" onClick={toggleDropdown} aria-haspopup="menu" aria-expanded={isOpen}>
        {trigger}
      </div>
      {isOpen && (
        <div className={`dropdown-menu-content dropdown-menu-align-${align}`} role="menu" onClick={handleMenuClick}>
          {children}
        </div>
      )}
    </div>
  );
};

export interface DropdownMenuItemProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  children: ReactNode;
  icon?: ReactNode;
}

export const DropdownMenuItem: React.FC<DropdownMenuItemProps> = ({ children, icon, className = '', ...props }) => {
  return (
    <button className={`dropdown-menu-item ${className}`} role="menuitem" {...props}>
      {icon && <span className="dropdown-menu-item-icon">{icon}</span>}
      {children}
    </button>
  );
};
