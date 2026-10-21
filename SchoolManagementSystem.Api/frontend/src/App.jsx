import { useEffect, useState } from "react";
import { userManager } from "./auth/oidc";
import Students from "./pages/Students";
import Attendance from "./pages/Attendance";
import Teachers from "./pages/Teachers";
import TeacherAttendance from "./pages/TeacherAttendance";
import Activities from "./pages/Activities";
import BulkImport from "./pages/BulkImport";
import "./App.css";

let signInCallbackPromise;

function App() {
  const [page, setPage] = useState("activities");
  const [user, setUser] = useState(null);
  const [ready, setReady] = useState(false);
  const [authError, setAuthError] = useState("");

  const signIn = async () => {
    setAuthError("");
    try { await userManager.signinRedirect(); }
    catch { setAuthError("Authentication could not be started."); }
  };

  const signOut = async () => {
    setAuthError("");
    try { await userManager.signoutRedirect(); }
    catch { setAuthError("The identity provider could not end the session."); }
  };

  useEffect(() => {
    let active = true;
    (async () => {
      try {
        const query = new URLSearchParams(window.location.search);
        const callback = query.has("code") || query.has("error");
        if (callback && !signInCallbackPromise)
          signInCallbackPromise = userManager.signinRedirectCallback();
        const current = callback ? await signInCallbackPromise : await userManager.getUser();
        if (callback) window.history.replaceState({}, document.title, window.location.pathname);
        if (active) setUser(current && !current.expired ? current : null);
      } catch (error) {
        if (active) setAuthError(error.message || "No se pudo iniciar sesión.");
      } finally {
        const query = new URLSearchParams(window.location.search);
        if (query.has("error") || query.has("code"))
          window.history.replaceState({}, document.title, window.location.pathname);
        signInCallbackPromise = undefined;
        if (active) setReady(true);
      }
    })();
    return () => { active = false; };
  }, []);

  const roles = [].concat(user?.profile?.role ?? []);
  const isAdmin = roles.includes("SchoolAdministrator");
  const isTeacher = roles.includes("Teacher");
  const isStudent = roles.includes("Student");

  if (!ready) return <main className="app-container">Cargando sesión…</main>;
  if (!user) return (
    <main className="app-container">
      <h1>School Management System</h1>
      {authError && <p role="alert">{authError}</p>}
      <button onClick={signIn}>Iniciar sesión</button>
    </main>
  );

  return (
    <div className="app-container">
      <header className="app-header">
        <h1>School Management System</h1>
        <span>{user.profile.name || user.profile.email}</span>
        <button onClick={signOut}>Cerrar sesión</button>
      </header>
      {authError && <p role="alert">{authError}</p>}
      <nav className="app-nav" aria-label="Navegación principal">
        {isAdmin && <button onClick={() => setPage("students")}>Alumnos</button>}
        {isAdmin && <button onClick={() => setPage("attendance")}>Asistencia alumnos</button>}
        {isAdmin && <button onClick={() => setPage("teachers")}>Docentes</button>}
        {isAdmin && <button onClick={() => setPage("teacherAttendance")}>Asistencia docente</button>}
        {(isAdmin || isTeacher) && <button onClick={() => setPage("activities")}>Actividades</button>}
        {isAdmin && <button onClick={() => setPage("bulkImport")}>Importar datos</button>}
      </nav>
      <div className="app-content">
        {isAdmin && page === "students" && <Students />}
        {isAdmin && page === "attendance" && <Attendance />}
        {isAdmin && page === "teachers" && <Teachers />}
        {isAdmin && page === "teacherAttendance" && <TeacherAttendance />}
        {(isAdmin || isTeacher) && page === "activities" && <Activities />}
        {isAdmin && page === "bulkImport" && <BulkImport />}
        {isStudent && <p>Tu sesión de alumno está activa. Las entregas usan la identidad autenticada de esta sesión.</p>}
      </div>
    </div>
  );
}

export default App;
