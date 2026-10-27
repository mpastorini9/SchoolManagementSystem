import { useEffect, useState } from "react";
import { userManager } from "./auth/oidc";
import Students from "./pages/Students";
import Attendance from "./pages/Attendance";
import Teachers from "./pages/Teachers";
import TeacherAttendance from "./pages/TeacherAttendance";
import Activities from "./pages/Activities";
import BulkImport from "./pages/BulkImport";
import StudentActivities from "./pages/StudentActivities";
import FamilyPortal from "./pages/FamilyPortal";
import "./App.css";

let signInCallbackPromise;

const navigation = [
  { id: "students", label: "Alumnos", group: "Institución", icon: "students", roles: ["SchoolAdministrator"], mobile: true },
  { id: "attendance", label: "Asistencia de alumnos", group: "Institución", icon: "attendance", roles: ["SchoolAdministrator"], mobile: true },
  { id: "teachers", label: "Docentes", group: "Institución", icon: "teachers", roles: ["SchoolAdministrator"], mobile: true },
  { id: "teacherAttendance", label: "Horarios y asistencia docente", group: "Gestión académica", icon: "schedule", roles: ["SchoolAdministrator", "Teacher"] },
  { id: "activities", label: "Actividades", group: "Gestión académica", icon: "activities", roles: ["SchoolAdministrator", "Teacher"], mobile: true },
  { id: "studentActivities", label: "Mis actividades", group: "Mi espacio", icon: "activities", roles: ["Student"], mobile: true },
  { id: "familyPortal", label: "Portal de familias", group: "Mi espacio", icon: "family", roles: ["Family"], mobile: true },
  { id: "bulkImport", label: "Importar datos", group: "Administración", icon: "import", roles: ["SchoolAdministrator"] }
];

const roleLabels = {
  SchoolAdministrator: "Administración escolar",
  Teacher: "Docente",
  Student: "Alumno",
  Family: "Familia"
};

function getSafeOidcError(error) {
  const type = error instanceof Error ? error.name : typeof error;
  const message = error instanceof Error ? error.message : String(error ?? "Unknown error");
  const safeMessage = message
    .replace(/\b(?:access_token|refresh_token|id_token|client_secret|password|code_verifier|authorization|state|code)(\s*[=:]\s*)[^&\s,;]+/gi, "$1[redacted]")
    .replace(/\beyJ[A-Za-z0-9_-]{20,}\.[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]+\b/g, "[redacted token]")
    .replace(/https?:\/\/[^\s"'<>]+/gi, (value) => {
      try {
        const url = new URL(value);
        return `${url.origin}${url.pathname}`;
      } catch {
        return "[redacted URL]";
      }
    });

  return { type, message: safeMessage };
}

function AppIcon({ name, size = 20 }) {
  const paths = {
    students: <><path d="M16 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" /><circle cx="10" cy="7" r="4" /><path d="M20 21v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75" /></>,
    attendance: <><path d="M8 2v4M16 2v4M3 10h18" /><rect x="3" y="4" width="18" height="18" rx="3" /><path d="m8 15 2.2 2.2L16 12" /></>,
    teachers: <><path d="M12 3 2.5 8 12 13l9.5-5L12 3Z" /><path d="M6 10v5c3.3 2.8 8.7 2.8 12 0v-5M21.5 8v6" /></>,
    schedule: <><rect x="3" y="4" width="18" height="17" rx="3" /><path d="M8 2v4M16 2v4M3 10h18M8 14h3M8 17h6" /></>,
    activities: <><path d="M8 3h8l4 4v14H4V3h4Z" /><path d="M14 3v5h6M8 13h8M8 17h6" /></>,
    family: <><path d="M3 10.5 12 3l9 7.5V21H3v-10.5Z" /><path d="M9 21v-6h6v6M7.5 10h.01M16.5 10h.01" /></>,
    import: <><path d="M12 15V3m0 0L7 8m5-5 5 5" /><path d="M4 14v5a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-5" /></>,
    logout: <><path d="M10 17l5-5-5-5M15 12H3" /><path d="M12 3h6a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-6" /></>,
    menu: <><path d="M4 6h16M4 12h16M4 18h16" /></>,
    more: <><circle cx="5" cy="12" r="1" /><circle cx="12" cy="12" r="1" /><circle cx="19" cy="12" r="1" /></>
  };

  return <svg aria-hidden="true" width={size} height={size} viewBox="0 0 24 24" fill="none"
    stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">{paths[name] ?? paths.activities}</svg>;
}

function App() {
  const [page, setPage] = useState("activities");
  const [user, setUser] = useState(null);
  const [ready, setReady] = useState(false);
  const [authError, setAuthError] = useState("");
  const [signInPending, setSignInPending] = useState(false);
  const [mobileMoreOpen, setMobileMoreOpen] = useState(false);

  const signIn = async () => {
    if (signInPending) return;
    setAuthError("");
    setSignInPending(true);
    let stage = "discovery";
    try {
      await userManager.metadataService.getAuthorizationEndpoint();
      console.info("[OIDC] Discovery completed");
      stage = "authorization request creation or browser navigation";
      console.info("[OIDC] Starting authorization redirect");
      await userManager.signinRedirect();
    } catch (error) {
      const details = getSafeOidcError(error);
      console.error("[OIDC] Sign-in start failed", { stage, ...details });
      setAuthError("No se pudo completar el inicio de sesi\u00f3n. Revis\u00e1 la consola del navegador para ver el detalle t\u00e9cnico sin datos de autenticaci\u00f3n.");
      setSignInPending(false);
    }
  };
  const signOut = async () => {
    setAuthError("");
    try { await userManager.signoutRedirect(); }
    catch { setAuthError("No se pudo cerrar la sesión."); }
  };

  useEffect(() => {
    let active = true;
    (async () => {
      let stage = "session restoration";
      try {
        const query = new URLSearchParams(window.location.search);
        const callback = query.has("code") || query.has("error");
        if (callback) stage = query.has("error") ? "authorization error response" : "callback processing";
        if (callback && !signInCallbackPromise)
          signInCallbackPromise = userManager.signinRedirectCallback();
        const current = callback ? await signInCallbackPromise : await userManager.getUser();
        if (callback) console.info("[OIDC] Callback processed", { authenticated: Boolean(current && !current.expired) });
        if (callback) window.history.replaceState({}, document.title, window.location.pathname);
        if (active) {
          const authenticatedUser = current && !current.expired ? current : null;
          setUser(authenticatedUser);
          const authenticatedRoles = [].concat(authenticatedUser?.profile?.role ?? []);
          if (authenticatedRoles.includes("Student")) setPage("studentActivities");
          if (authenticatedRoles.includes("Family")) setPage("familyPortal");
        }
      } catch (error) {
        const details = getSafeOidcError(error);
        console.error("[OIDC] Sign-in callback failed", { stage, ...details });
        if (active) setAuthError("No se pudo completar la autenticación. Revisá la consola del navegador para ver el detalle técnico sin datos de autenticación.");
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
  const visibleNavigation = navigation.filter((item) => item.roles.some((role) => roles.includes(role)));
  const navGroups = [...new Set(visibleNavigation.map((item) => item.group))];
  const activeItem = visibleNavigation.find((item) => item.id === page) ?? visibleNavigation[0];
  const userName = user?.profile?.name || user?.profile?.email || "Usuario";
  const userInitials = userName.split(/[\s@.]+/).filter(Boolean).slice(0, 2).map((part) => part[0]).join("").toUpperCase();
  const userRole = roles.map((role) => roleLabels[role]).find(Boolean) ?? "Usuario";
  const mobileNavigation = visibleNavigation.filter((item) => item.mobile);
  const mobilePrimary = mobileNavigation.slice(0, 4);
  const mobileSecondary = visibleNavigation.filter((item) => !mobilePrimary.includes(item));

  function navigate(id) {
    setPage(id);
    setMobileMoreOpen(false);
  }

  if (!ready) return <main className="app-loading" role="status"><span className="app-loading-mark" />Cargando tu espacio…</main>;
  if (!user) return (
    <main className="login-screen">
      <section className="login-card" aria-labelledby="login-title">
        <div className="login-brand"><span className="login-brand-mark"><AppIcon name="family" size={24} /></span><span>School Management System</span></div>
        <p className="login-eyebrow">Plataforma escolar</p>
        <h1 id="login-title">Todo lo importante de la escuela, en un solo lugar.</h1>
        <p className="login-description">Al continuar, se abrirá el formulario seguro de la institución para ingresar tu correo y contraseña.</p>
        {authError && <p className="login-error" role="alert">{authError}</p>}
        <button className="login-button" onClick={signIn} disabled={signInPending} aria-busy={signInPending}>
          {signInPending ? "Conectando…" : "Iniciar sesión"}
        </button>
        <p className="login-security-note">Acceso seguro con tu cuenta institucional.</p>
      </section>
    </main>
  );

  return (
    <div className="app-shell">
      <aside className="app-sidebar" aria-label="Panel de navegación">
        <div className="app-brand">
          <span className="app-brand-mark"><AppIcon name="family" size={23} /></span>
          <span className="app-brand-copy"><strong>School Management</strong><small>Plataforma escolar</small></span>
        </div>
        <div className="app-sidebar-caption">MENÚ PRINCIPAL</div>
        <nav className="app-nav" aria-label="Navegación principal">
          {navGroups.map((group) => <div className="app-nav-group" key={group}>
            <p className="app-nav-label">{group}</p>
            {visibleNavigation.filter((item) => item.group === group).map((item) => <button type="button" key={item.id}
              className={`app-nav-item${activeItem?.id === item.id ? " is-active" : ""}`}
              aria-current={activeItem?.id === item.id ? "page" : undefined} onClick={() => navigate(item.id)}>
              <AppIcon name={item.icon} /><span>{item.label}</span>
            </button>)}
          </div>)}
        </nav>
        <div className="app-sidebar-footer"><span className="app-sidebar-dot" />Sesión institucional activa</div>
      </aside>

      <div className="app-workspace">
        <header className="app-topbar">
          <div className="app-topbar-brand"><span className="app-brand-mark"><AppIcon name="family" size={21} /></span><strong>School Management</strong></div>
          <div className="app-breadcrumb"><span>{activeItem?.group}</span><span aria-hidden="true">/</span><strong>{activeItem?.label}</strong></div>
          <div className="app-account">
            <div className="app-account-avatar" aria-hidden="true">{userInitials || "U"}</div>
            <div className="app-account-copy"><strong>{userName}</strong><span>{userRole}</span></div>
            <button className="app-signout" onClick={signOut} aria-label="Cerrar sesión" title="Cerrar sesión"><AppIcon name="logout" size={19} /><span>Salir</span></button>
          </div>
        </header>

        {authError && <p className="app-auth-alert" role="alert">{authError}</p>}
        <div className="app-content" key={activeItem?.id}>
          {activeItem?.id === "students" && <Students />}
          {activeItem?.id === "attendance" && <Attendance />}
          {activeItem?.id === "teachers" && <Teachers />}
          {activeItem?.id === "teacherAttendance" && <TeacherAttendance />}
          {activeItem?.id === "activities" && <Activities />}
          {activeItem?.id === "studentActivities" && <StudentActivities />}
          {activeItem?.id === "familyPortal" && <FamilyPortal />}
          {activeItem?.id === "bulkImport" && <BulkImport />}
        </div>
      </div>

      <nav className="app-mobile-nav" aria-label="Navegación principal móvil">
        {mobilePrimary.map((item) => <button type="button" key={item.id} className={activeItem?.id === item.id ? "is-active" : ""}
          aria-current={activeItem?.id === item.id ? "page" : undefined} onClick={() => navigate(item.id)}>
          <AppIcon name={item.icon} size={21} /><span>{item.label}</span>
        </button>)}
        {mobileSecondary.length > 0 && <button type="button" className={mobileMoreOpen || mobileSecondary.some((item) => item.id === activeItem?.id) ? "is-active" : ""} aria-expanded={mobileMoreOpen}
          aria-controls={mobileMoreOpen ? "app-mobile-more" : undefined} onClick={() => setMobileMoreOpen((open) => !open)}>
          <AppIcon name="more" size={21} /><span>Más</span>
        </button>}
        {mobileMoreOpen && mobileSecondary.length > 0 && <div className="app-mobile-more" id="app-mobile-more">
          {mobileSecondary.map((item) => <button type="button" key={item.id} className={activeItem?.id === item.id ? "is-active" : ""}
            aria-current={activeItem?.id === item.id ? "page" : undefined} onClick={() => navigate(item.id)}>
            <AppIcon name={item.icon} size={19} /><span>{item.label}</span>
          </button>)}
        </div>}
      </nav>
    </div>
  );
}

export default App;
