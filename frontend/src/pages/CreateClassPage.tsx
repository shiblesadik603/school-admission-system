import { useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { api, errorMessage } from '../api';
import { ErrorBox } from '../components/ErrorBox';

export function CreateClassPage() {
  const [name, setName] = useState('');
  const [seats, setSeats] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [saving, setSaving] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError('');
    setSuccess('');

    const totalSeats = Number(seats);
    if (name.trim() === '') {
      setError('Please enter a class name.');
      return;
    }
    if (!Number.isInteger(totalSeats) || totalSeats < 1) {
      setError('Total seats must be a whole number greater than 0.');
      return;
    }

    setSaving(true);
    try {
      const created = await api.createClass(name.trim(), totalSeats);
      setSuccess(`"${created.name}" created with ${created.totalSeats} seats.`);
      setName('');
      setSeats('');
    } catch (err) {
      setError(errorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <section>
      <h2>Create class</h2>
      <p className="hint">Create one class per shift, e.g. "Class 6 - Morning Shift" and "Class 6 - Day Shift".</p>

      <form className="form" onSubmit={handleSubmit}>
        <label htmlFor="class-name">Class name</label>
        <input id="class-name" value={name} onChange={(e) => setName(e.target.value)} placeholder="Class 6 - Morning Shift" />

        <label htmlFor="class-seats">Total seats</label>
        <input id="class-seats" type="number" min={1} value={seats} onChange={(e) => setSeats(e.target.value)} placeholder="120" />

        <button type="submit" disabled={saving}>
          {saving ? 'Saving...' : 'Create class'}
        </button>
      </form>

      <ErrorBox message={error} />
      {success && (
        <div className="success-box">
          {success} <Link to="/">See all classes</Link>
        </div>
      )}
    </section>
  );
}
