import { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { api, errorMessage } from '../api';
import { ErrorBox } from '../components/ErrorBox';
import { useInterval } from '../hooks';
import { isValidStudentId } from '../studentId';
import type { SchoolClass } from '../types';

export function ClassesPage({ studentId }: { studentId: string }) {
  const [classes, setClasses] = useState<SchoolClass[]>([]);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState('');
  const [applyError, setApplyError] = useState('');
  const [applyingId, setApplyingId] = useState('');
  const navigate = useNavigate();

  const loadClasses = useCallback(async () => {
    try {
      setClasses(await api.getClasses());
      setLoadError('');
    } catch (err) {
      setLoadError(errorMessage(err));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadClasses();
  }, [loadClasses]);

  // keep seat numbers fresh without reloading the page
  useInterval(loadClasses, 3000);

  async function handleApply(classId: string) {
    setApplyError('');
    if (!isValidStudentId(studentId)) {
      setApplyError('Please enter the student\'s 17 digit birth registration number at the top first.');
      return;
    }

    setApplyingId(classId);
    try {
      const hold = await api.apply(classId, studentId);
      navigate(`/holds/${hold.holdId}`);
    } catch (err) {
      setApplyError(errorMessage(err));
      loadClasses();
    } finally {
      setApplyingId('');
    }
  }

  return (
    <section>
      <h2>Classes</h2>
      <p className="hint">Seat numbers update automatically every few seconds.</p>

      <ErrorBox message={applyError} />
      <ErrorBox message={loadError} />

      {loading && <p>Loading classes...</p>}

      {!loading && classes.length === 0 && !loadError && (
        <p>
          No classes yet. <Link to="/classes/new">Create a class</Link>.
        </p>
      )}

      {classes.length > 0 && (
        <table>
          <thead>
            <tr>
              <th>Class</th>
              <th>Total</th>
              <th>On hold</th>
              <th>Confirmed</th>
              <th>Available</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {classes.map((c) => (
              <tr key={c.id}>
                <td>{c.name}</td>
                <td>{c.totalSeats}</td>
                <td>{c.heldSeats}</td>
                <td>{c.confirmedSeats}</td>
                <td className={c.availableSeats === 0 ? 'full' : ''}>{c.availableSeats}</td>
                <td>
                  <button
                    onClick={() => handleApply(c.id)}
                    disabled={c.availableSeats === 0 || applyingId !== ''}
                  >
                    {applyingId === c.id ? 'Applying...' : c.availableSeats === 0 ? 'Full' : 'Apply'}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  );
}
