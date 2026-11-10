import { describe, expect, it } from 'vitest';
import { displayStatus, formatSeconds, secondsLeft } from '../time';
import type { Hold } from '../types';

function makeHold(expiresAt: string, status: Hold['status'] = 'HELD'): Hold {
  return {
    holdId: 'h_1',
    classId: 'c_1',
    className: 'Class 6',
    studentId: '20152691234567890',
    status,
    expiresAt,
    createdAt: '2026-11-01T04:00:00Z',
    confirmedAt: null,
    serverTime: '2026-11-01T04:00:00Z',
  };
}

describe('countdown helpers', () => {
  const now = new Date('2026-11-01T04:00:00Z').getTime();

  it('counts seconds left until expiry', () => {
    expect(secondsLeft(makeHold('2026-11-01T04:10:00Z'), now, 0)).toBe(600);
  });

  it('uses the server clock offset', () => {
    // browser clock is 60s behind the server
    expect(secondsLeft(makeHold('2026-11-01T04:10:00Z'), now, 60_000)).toBe(540);
  });

  it('shows HELD as EXPIRED once time is over', () => {
    const hold = makeHold('2026-11-01T03:59:59Z');
    expect(displayStatus(hold, now, 0)).toBe('EXPIRED');
  });

  it('does not change CONFIRMED holds', () => {
    const hold = makeHold('2026-11-01T03:59:59Z', 'CONFIRMED');
    expect(displayStatus(hold, now, 0)).toBe('CONFIRMED');
  });

  it('formats seconds as m:ss', () => {
    expect(formatSeconds(600)).toBe('10:00');
    expect(formatSeconds(65)).toBe('1:05');
  });
});
