
import { useState } from "react";
import Students from "./pages/Students";
import Attendance from "./pages/Attendance";

function App() {

  const [page, setPage] = useState("students");

  return (
    <>
      <h1>INAC - CIATA</h1>

      <button onClick={() => setPage("students")}>
        Alumnos
      </button>

      <button onClick={() => setPage("attendance")}>
        Asistencia
      </button>

      <hr />

      {page === "students" && <Students />}
      {page === "attendance" && <Attendance />}
    </>
  );
}

export default App;