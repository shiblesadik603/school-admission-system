import { useState } from 'react';

const STORAGE_KEY = 'studentId';

export function isValidStudentId(value: string) {
  return /^\d{17}$/.test(value);
}

function loadStudentId() {
  try {
    return localStorage.getItem(STORAGE_KEY) ?? '';
  } catch {
    return '';
  }
}

// The student id is kept in localStorage so it survives refresh / closing the browser.
export function useStudentId() {
  const [studentId, setStudentIdState] = useState(loadStudentId);

  function setStudentId(value: string) {
    setStudentIdState(value);
    try {
      if (value) localStorage.setItem(STORAGE_KEY, value);
      else localStorage.removeItem(STORAGE_KEY);
    } catch {
      // ignore (private mode etc.)
    }
  }

  return { studentId, setStudentId };
}
