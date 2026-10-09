
import { useState } from "react";
import Students from "./pages/Students";
import Attendance from "./pages/Attendance";
import Teachers from "./pages/Teachers";
import TeacherAttendance from "./pages/TeacherAttendance";
import Activities from "./pages/Activities";
import BulkImport from "./pages/BulkImport";
import "./App.css";

function App() {

  const [page, setPage] = useState("students");

  return (
    <div className="app-container">
      <header className="app-header"><h1>Colegio José Antonio de Oro Ocampo</h1></header>
      <nav className="app-nav" aria-label="Navegación principal">
        <button onClick={() => setPage("students")}>Alumnos</button>
        <button onClick={() => setPage("attendance")}>Asistencia alumnos</button>
        <button onClick={() => setPage("teachers")}>Docentes</button>
        <button onClick={() => setPage("teacherAttendance")}>Asistencia docente</button>
        <button onClick={() => setPage("activities")}>Actividades</button>
        <button onClick={() => setPage("bulkImport")}>Importar datos</button>
      </nav>
      <div className="app-content">
        {page === "students" && <Students />}
        {page === "attendance" && <Attendance />}
        {page === "teachers" && <Teachers />}
        {page === "teacherAttendance" && <TeacherAttendance />}
        {page === "activities" && <Activities />}
        {page === "bulkImport" && <BulkImport />}
      </div>
    </div>
  );
}

export default App;
