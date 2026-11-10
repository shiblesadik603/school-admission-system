import { useState, type FormEvent } from 'react';
import { isValidStudentId } from '../studentId';

interface Props {
  studentId: string;
  onChange: (value: string) => void;
}

export function StudentIdBar({ studentId, onChange }: Props) {
  const [value, setValue] = useState(studentId);
  const [error, setError] = useState('');

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    const trimmed = value.trim();
    if (!isValidStudentId(trimmed)) {
      setError('Birth registration number must be exactly 17 digits.');
      return;
    }
    setError('');
    onChange(trimmed);
  }

  function handleClear() {
    setValue('');
    setError('');
    onChange('');
  }

  return (
    <form className="student-bar" onSubmit={handleSubmit}>
      <label htmlFor="student-id">Student birth registration no.</label>
      <input
        id="student-id"
        value={value}
        onChange={(e) => setValue(e.target.value)}
        placeholder="17 digit number"
        inputMode="numeric"
        maxLength={17}
      />
      <button type="submit">Use this ID</button>
      {studentId && (
        <button type="button" className="secondary" onClick={handleClear}>
          Clear
        </button>
      )}
      <span className="student-current">
        {studentId ? <>Current student: <b>{studentId}</b></> : 'No student selected'}
      </span>
      {error && <div className="field-error">{error}</div>}
    </form>
  );
}
