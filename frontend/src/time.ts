import type { Hold, HoldStatus } from './types';

// The browser clock can be wrong, so we measure the difference with the server time
// that comes in every hold response and use it for the countdown.
export function clockOffset(hold: Hold, receivedAt: number) {
  return new Date(hold.serverTime).getTime() - receivedAt;
}

export function secondsLeft(hold: Hold, now: number, offset: number) {
  const ms = new Date(hold.expiresAt).getTime() - (now + offset);
  return Math.max(0, Math.ceil(ms / 1000));
}

// HELD but the time is over -> show EXPIRED right away, without waiting for the server.
export function displayStatus(hold: Hold, now: number, offset: number): HoldStatus {
  if (hold.status === 'HELD' && secondsLeft(hold, now, offset) === 0) return 'EXPIRED';
  return hold.status;
}

export function formatSeconds(total: number) {
  const m = Math.floor(total / 60);
  const s = total % 60;
  return `${m}:${s.toString().padStart(2, '0')}`;
}
