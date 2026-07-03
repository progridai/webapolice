import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import App from '../src/app/App';

describe('App Component', () => {
  it('should render application name and technical foundation active message', () => {
    render(<App />);

    expect(screen.getByRole('heading', { name: /WebApólice/i })).not.toBeNull();
    expect(screen.getByText(/A fundação técnica do frontend está ativa/i)).not.toBeNull();
  });
});
