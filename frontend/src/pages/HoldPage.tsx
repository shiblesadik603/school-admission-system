import { useCallback, useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { api, errorMessage } from '../api';
import { ErrorBox } from '../components/ErrorBox';
import { StatusBadge } from '../components/StatusBadge';
import { useInterval, useNow } from '../hooks';
import { clockOffset, displayStatus, formatSeconds, secondsLeft } from '../time';
import type { Hold } from '../types';

export function HoldPage({ studentId }: { studentId: string }) {
  const { holdId = '' } = useParams();
  const [hold, setHold] = useState<Hold | null>(null);
  const [offset, setOffset] = useState(0);
  const [loadError, setLoadError] = useState('');
  const [actionError, setActionError] = useState('');
  const [busy, setBusy] = useState(false);
  const now = useNow();

  const loadHold = useCallback(async () => {
    try {
      const data = await api.getHold(holdId);
      setHold(data);
      setOffset(clockOffset(data, Date.now()));
      setLoadError('');
    } catch (err) {
      setLoadError(errorMessage(err));
    }
  }, [holdId]);

  useEffect(() => {
    loadHold();
  }, [loadHold]);

  useInterval(loadHold, 5000);

  if (!hold) {
    return (
      <section>
        <h2>Application</h2>
        {loadError ? <ErrorBox message={loadError} /> : <p>Loading...</p>}
        <Link to="/my">Back to my applications</Link>
      </section>
    );
  }

  const status = displayStatus(hold, now, offset);
  const left = secondsLeft(hold, now, offset);
  const isOwner = hold.studentId === studentId;

  async function handlePay() {
    setActionError('');
    setBusy(true);
    try {
      await api.confirm(holdId, studentId);
    } catch (err) {
      setActionError(errorMessage(err));
    } finally {
      setBusy(false);
      loadHold();
    }
  }

  async function handleCancel() {
    if (!window.confirm('Cancel this application? The seat will be given to someone else.')) return;
    setActionError('');
    setBusy(true);
    try {
      await api.cancel(holdId, studentId);
    } catch (err) {
      setActionError(errorMessage(err));
    } finally {
      setBusy(false);
      loadHold();
    }
  }

  return (
    <section>
      <h2>Application</h2>
      <div className="card">
        <p><b>Class:</b> {hold.className}</p>
        <p><b>Student:</b> {hold.studentId}</p>
        <p><b>Status:</b> <StatusBadge status={status} /></p>

        {status === 'HELD' && (
          <p className="countdown">
            Time left to pay: <b>{formatSeconds(left)}</b>
          </p>
        )}
        {status === 'EXPIRED' && <p className="hint">The payment time is over and the seat has been released. You can apply again if seats are available.</p>}
        {status === 'CONFIRMED' && <p className="hint">Admission fee paid. The seat is confirmed.</p>}
        {status === 'CANCELLED' && <p className="hint">This application was cancelled.</p>}

        {!isOwner && (
          <p className="hint">This application belongs to a different student. Enter that student's ID at the top to pay or cancel.</p>
        )}

        <div className="actions">
          <button onClick={handlePay} disabled={status !== 'HELD' || !isOwner || busy}>
            Pay admission fee
          </button>
          <button className="danger" onClick={handleCancel} disabled={status !== 'HELD' || !isOwner || busy}>
            Cancel
          </button>
        </div>

        <ErrorBox message={actionError} />
        <ErrorBox message={loadError} />
      </div>
      <p>
        <Link to="/my">My applications</Link> · <Link to="/">All classes</Link>
      </p>
    </section>
  );
}
