import { NavLink, Route, Routes } from 'react-router-dom';
import { StudentIdBar } from './components/StudentIdBar';
import { ClassesPage } from './pages/ClassesPage';
import { CreateClassPage } from './pages/CreateClassPage';
import { HoldPage } from './pages/HoldPage';
import { MyHoldsPage } from './pages/MyHoldsPage';
import { useStudentId } from './studentId';

export default function App() {
  const { studentId, setStudentId } = useStudentId();

  return (
    <div className="container">
      <header>
        <h1>School Admission</h1>
        <StudentIdBar studentId={studentId} onChange={setStudentId} />
        <nav>
          <NavLink to="/" end>Classes</NavLink>
          <NavLink to="/my">My applications</NavLink>
          <NavLink to="/classes/new">Create class (admin)</NavLink>
        </nav>
      </header>

      <main>
        <Routes>
          <Route path="/" element={<ClassesPage studentId={studentId} />} />
          <Route path="/classes/new" element={<CreateClassPage />} />
          <Route path="/holds/:holdId" element={<HoldPage studentId={studentId} />} />
          <Route path="/my" element={<MyHoldsPage studentId={studentId} />} />
          <Route path="*" element={<p>Page not found.</p>} />
        </Routes>
      </main>
    </div>
  );
}
