import { useEffect, useRef, useState } from 'react';

// Calls `callback` every `ms` milliseconds (and keeps using the latest callback).
export function useInterval(callback: () => void, ms: number) {
  const saved = useRef(callback);

  useEffect(() => {
    saved.current = callback;
  }, [callback]);

  useEffect(() => {
    const id = setInterval(() => saved.current(), ms);
    return () => clearInterval(id);
  }, [ms]);
}

// Current time that updates every second, used for countdowns.
export function useNow() {
  const [now, setNow] = useState(() => Date.now());
  useInterval(() => setNow(Date.now()), 1000);
  return now;
}
