import React from 'react';
import './Table.css';

export const Table: React.FC<React.TableHTMLAttributes<HTMLTableElement>> = ({ className = '', ...props }) => (
  <div className="table-responsive-wrapper">
    <table className={`data-table-component ${className}`} {...props} />
  </div>
);

export const TableHeader: React.FC<React.HTMLAttributes<HTMLTableSectionElement>> = (props) => (
  <thead {...props} />
);

export const TableBody: React.FC<React.HTMLAttributes<HTMLTableSectionElement>> = (props) => (
  <tbody {...props} />
);

export interface TableRowProps extends React.HTMLAttributes<HTMLTableRowElement> {
  selecionado?: boolean;
}

export const TableRow: React.FC<TableRowProps> = ({
  selecionado = false,
  className = '',
  ...props
}) => (
  <tr className={`${selecionado ? 'row-selected-component' : ''} ${className}`.trim()} {...props} />
);

export interface TableCellProps extends React.TdHTMLAttributes<HTMLTableCellElement> {
  header?: boolean;
}

export const TableCell: React.FC<TableCellProps> = ({
  header = false,
  className = '',
  ...props
}) => {
  if (header) {
    return <th className={className} {...props} />;
  }
  return <td className={className} {...props} />;
};
