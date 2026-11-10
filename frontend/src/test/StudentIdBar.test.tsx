import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { StudentIdBar } from '../components/StudentIdBar';

describe('StudentIdBar', () => {
  it('shows an error for an id that is not 17 digits', async () => {
    const onChange = vi.fn();
    render(<StudentIdBar studentId="" onChange={onChange} />);

    await userEvent.type(screen.getByLabelText(/birth registration/i), '12345');
    await userEvent.click(screen.getByRole('button', { name: /use this id/i }));

    expect(screen.getByText(/exactly 17 digits/i)).toBeInTheDocument();
    expect(onChange).not.toHaveBeenCalled();
  });

  it('accepts a valid 17 digit id', async () => {
    const onChange = vi.fn();
    render(<StudentIdBar studentId="" onChange={onChange} />);

    await userEvent.type(screen.getByLabelText(/birth registration/i), '20152691234567890');
    await userEvent.click(screen.getByRole('button', { name: /use this id/i }));

    expect(onChange).toHaveBeenCalledWith('20152691234567890');
  });
});
