import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { api, errorMessage } from '../api';
import { ErrorBox } from '../components/ErrorBox';
import { StatusBadge } from '../components/StatusBadge';
import { useInterval, useNow } from '../hooks';
import { isValidStudentId } from '../studentId';
import { clockOffset, displayStatus, formatSeconds, secondsLeft } from '../time';
import type { Hold } from '../types';

export function MyHoldsPage({ studentId }: { studentId: string }) {
  const [holds, setHolds] = useState<Hold[]>([]);
  const [offset, setOffset] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const now = useNow();
  const hasStudent = isValidStudentId(studentId);

  const loadHolds = useCallback(async () => {
    if (!hasStudent) return;
    try {
      const data = await api.getMyHolds(studentId);
      setHolds(data);
      if (data.length > 0) setOffset(clockOffset(data[0], Date.now()));
      setError('');
    } catch (err) {
      setError(errorMessage(err));
    } finally {
      setLoading(false);
    }
  }, [studentId, hasStudent]);

  useEffect(() => {
    setHolds([]);
    setLoading(true);
    loadHolds();
  }, [loadHolds]);

  useInterval(loadHolds, 5000);

  if (!hasStudent) {
    return (
      <section>
        <h2>My applications</h2>
        <p>Enter the student's 17 digit birth registration number at the top to see their applications.</p>
      </section>
    );
  }

  return (
    <section>
      <h2>My applications</h2>
      <ErrorBox message={error} />
      {loading && <p>Loading...</p>}
      {!loading && holds.length === 0 && !error && (
        <p>
          No applications yet. <Link to="/">Apply for a class</Link>.
        </p>
      )}

      {holds.length > 0 && (
        <table>
          <thead>
            <tr>
              <th>Class</th>
              <th>Status</th>
              <th>Time left</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {holds.map((h) => {
              const status = displayStatus(h, now, offset);
              return (
                <tr key={h.holdId}>
                  <td>{h.className}</td>
                  <td><StatusBadge status={status} /></td>
                  <td>{status === 'HELD' ? formatSeconds(secondsLeft(h, now, offset)) : '-'}</td>
                  <td>
                    <Link to={`/holds/${h.holdId}`}>{status === 'HELD' ? 'Pay / Cancel' : 'View'}</Link>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      )}
    </section>
  );
}
