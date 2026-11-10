import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { ApiError, api } from '../api';
import { ClassesPage } from '../pages/ClassesPage';

const classes = [
  { id: 'c_1', name: 'Class 6 - Morning Shift', totalSeats: 120, heldSeats: 15, confirmedSeats: 80, availableSeats: 25 },
  { id: 'c_2', name: 'Class 7 - Day Shift', totalSeats: 50, heldSeats: 10, confirmedSeats: 40, availableSeats: 0 },
];

function renderPage(studentId = '20152691234567890') {
  return render(
    <MemoryRouter>
      <ClassesPage studentId={studentId} />
    </MemoryRouter>,
  );
}

describe('ClassesPage', () => {
  afterEach(() => vi.restoreAllMocks());

  it('lists classes with seat numbers and disables full classes', async () => {
    vi.spyOn(api, 'getClasses').mockResolvedValue(classes);
    renderPage();

    expect(await screen.findByText('Class 6 - Morning Shift')).toBeInTheDocument();
    expect(screen.getByText('25')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Full' })).toBeDisabled();
    expect(screen.getByRole('button', { name: 'Apply' })).toBeEnabled();
  });

  it('shows a readable message when no seat is left', async () => {
    vi.spyOn(api, 'getClasses').mockResolvedValue(classes);
    vi.spyOn(api, 'apply').mockRejectedValue(
      new ApiError(409, 'NO_SEATS_AVAILABLE', 'Sorry, there are no seats available in this class right now.'),
    );
    renderPage();

    await userEvent.click(await screen.findByRole('button', { name: 'Apply' }));

    expect(await screen.findByRole('alert')).toHaveTextContent(/no seats available/i);
  });

  it('shows a message when the server cannot be reached', async () => {
    vi.spyOn(api, 'getClasses').mockRejectedValue(new ApiError(0, 'NETWORK_ERROR', 'Cannot reach the server.'));
    renderPage();

    expect(await screen.findByRole('alert')).toHaveTextContent(/cannot reach the server/i);
  });
});
