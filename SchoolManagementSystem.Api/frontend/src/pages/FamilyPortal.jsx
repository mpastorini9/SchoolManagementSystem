import { useEffect, useMemo, useState } from "react";
import {
  downloadFamilyActivityMaterialFile,
  downloadFamilySubmissionFile,
  getFamilyActivityMaterials,
  getFamilyStudentActivities,
  getFamilyStudentAttendance,
  getFamilyStudentGrades,
  getFamilyStudentRecords,
  getFamilyStudentSchedule,
  getFamilyStudents
} from "../services/api";
import { groupActivitiesByDueDate } from "../utils/activityAgenda";
import "./FamilyPortal.css";

const sections = [
  ["summary", "Resumen académico"],
  ["attendance", "Asistencias"],
  ["activities", "Actividades y entregas"],
  ["grades", "Calificaciones"],
  ["schedule", "Materias y horarios"],
  ["records", "Legajo e historial"]
];
const attendanceLabels = { Present: "Presente", Absent: "Ausente", Late: "Tarde", Justified: "Justificada" };
const recordLabels = { Observation: "Observación", Incident: "Incidente", Recognition: "Reconocimiento" };
const weekdayLabels = ["domingo", "lunes", "martes", "miércoles", "jueves", "viernes", "sábado"];

function formatDate(value, includeTime = false) {
  if (!value) return "—";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "—";
  return new Intl.DateTimeFormat("es-UY", includeTime
    ? { dateStyle: "medium", timeStyle: "short" }
    : { dateStyle: "medium" }).format(date);
}

function activityState(activity) {
  if (activity.submission?.reviewedAt) return "Con devolución";
  if (activity.submission) return "Entregada · pendiente de revisión";
  const dueDate = activity.dueAt ? new Date(activity.dueAt) : null;
  return dueDate && Number.isFinite(dueDate.getTime()) && dueDate.getTime() < Date.now()
    ? "Pendiente · vencida"
    : "Pendiente";
}

export default function FamilyPortal() {
  const [children, setChildren] = useState([]);
  const [selectedStudentId, setSelectedStudentId] = useState("");
  const [section, setSection] = useState("summary");
  const [attendance, setAttendance] = useState([]);
  const [activities, setActivities] = useState([]);
  const [grades, setGrades] = useState([]);
  const [schedule, setSchedule] = useState([]);
  const [records, setRecords] = useState([]);
  const [materials, setMaterials] = useState({});
  const [materialErrors, setMaterialErrors] = useState({});
  const [attendanceYear, setAttendanceYear] = useState("all");
  const [loadingChildren, setLoadingChildren] = useState(true);
  const [loadingChildData, setLoadingChildData] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    let active = true;
    getFamilyStudents()
      .then((data) => {
        if (!active) return;
        setChildren(data);
        setError("");
        setLoadingChildData(data.length > 0);
        setSelectedStudentId(data.length ? String(data[0].id) : "");
      })
      .catch((loadError) => { if (active) setError(loadError.message || "No se pudieron cargar los alumnos vinculados."); })
      .finally(() => { if (active) setLoadingChildren(false); });
    return () => { active = false; };
  }, []);

  useEffect(() => {
    if (!selectedStudentId) return undefined;
    let active = true;
    const studentId = Number(selectedStudentId);
    Promise.all([
      getFamilyStudentAttendance(studentId), getFamilyStudentActivities(studentId),
      getFamilyStudentGrades(studentId), getFamilyStudentSchedule(studentId), getFamilyStudentRecords(studentId)
    ]).then(async ([attendanceData, activityData, gradeData, scheduleData, recordData]) => {
      const materialResults = await Promise.all(activityData.map(async (activity) => ({
        id: activity.id,
        result: await Promise.allSettled([getFamilyActivityMaterials(studentId, activity.id)])
      })));
      if (!active) return;
      setAttendance(attendanceData); setActivities(activityData); setGrades(gradeData);
      setSchedule(scheduleData); setRecords(recordData);
      setMaterials(Object.fromEntries(materialResults.filter(item => item.result[0].status === "fulfilled")
        .map(item => [item.id, item.result[0].value])));
      setMaterialErrors(Object.fromEntries(materialResults.filter(item => item.result[0].status === "rejected")
        .map(item => [item.id, true])));
    }).catch((loadError) => { if (active) setError(loadError.message || "No se pudo cargar la información académica del alumno."); })
      .finally(() => { if (active) setLoadingChildData(false); });
    return () => { active = false; };
  }, [selectedStudentId]);

  const selectedChild = children.find((child) => String(child.id) === selectedStudentId);
  const activityGroups = useMemo(() => groupActivitiesByDueDate(activities, (activity) => activity.dueAt), [activities]);
  const attendanceYears = useMemo(() => [...new Set(attendance.map((item) => new Date(item.date).getFullYear()))].sort((a, b) => b - a), [attendance]);
  const visibleAttendance = attendanceYear === "all" ? attendance : attendance.filter((item) => new Date(item.date).getFullYear() === Number(attendanceYear));
  const visibleAttendanceSummary = useMemo(() => visibleAttendance.reduce((summary, item) => {
    if (Object.hasOwn(summary, item.status)) summary[item.status] += 1;
    return summary;
  }, { Present: 0, Absent: 0, Late: 0, Justified: 0 }), [visibleAttendance]);
  const attendanceSummary = useMemo(() => attendance.reduce((summary, item) => {
    if (Object.hasOwn(summary, item.status)) summary[item.status] += 1;
    return summary;
  }, { Present: 0, Absent: 0, Late: 0, Justified: 0 }), [attendance]);
  const pendingCount = activities.filter((item) => !item.submission).length;
  const submittedCount = activities.filter((item) => item.submission).length;
  const reviewedCount = activities.filter((item) => item.submission?.reviewedAt).length;

  async function runDownload(download) {
    try { await download(); }
    catch (downloadError) { setError(downloadError.message || "No se pudo descargar el archivo."); }
  }

  function renderAttendance() {
    return <section className="family-portal-section">
      <div className="family-section-heading"><div><h3>Historial de asistencias</h3><p>Registros informados por el colegio.</p></div>
        {attendanceYears.length > 0 && <label className="family-filter">Período<select value={attendanceYear} onChange={(event) => setAttendanceYear(event.target.value)}>
          <option value="all">Todos los años</option>{attendanceYears.map((year) => <option key={year} value={year}>{year}</option>)}
        </select></label>}
      </div>
      {visibleAttendance.length > 0 && <div className="family-stat-grid compact">{Object.entries(visibleAttendanceSummary).map(([status, count]) => <div className="family-stat" key={status}><span>{attendanceLabels[status]}</span><strong>{count}</strong></div>)}</div>}
      {visibleAttendance.length === 0 ? <p className="family-portal-empty">{attendance.length === 0 ? "Todavía no hay asistencias registradas." : "No hay asistencias registradas para este período."}</p> : <div className="family-portal-table-wrap"><table className="family-portal-table"><thead><tr><th>Fecha</th><th>Estado</th></tr></thead><tbody>
        {visibleAttendance.map((item) => <tr key={item.id}><td>{formatDate(item.date, true)}</td><td>{attendanceLabels[item.status] ?? "Registrada"}</td></tr>)}
      </tbody></table></div>}
    </section>;
  }

  function renderActivities() {
    return <section className="family-portal-section"><h3>Actividades, entregas y materiales</h3>
      {activities.length === 0 ? <p className="family-portal-empty">No hay actividades publicadas para este alumno.</p> : <div className="family-portal-activities">
        {activityGroups.filter((group) => group.activities.length > 0).map((group) => <section className="family-agenda-group" key={group.id} aria-labelledby={`family-agenda-${group.id}`}>
          <header className="family-agenda-heading"><h4 id={`family-agenda-${group.id}`}>{group.label}</h4><span>{group.activities.length}</span></header>
          {group.activities.map((activity) => <article className="family-portal-activity" key={activity.id}>
          <p className="family-portal-activity-meta">{activity.courseName} · {activity.subjectName} · Publicada {formatDate(activity.publishedAt)}</p>
          <h4>{activity.title}</h4><p>{activity.description}</p>
          <p className="family-portal-activity-meta">Docente: {activity.teacherName} · Vence: {activity.dueAt ? formatDate(activity.dueAt, true) : "Sin fecha"}</p>
          <p className="family-status-pill">{activityState(activity)}</p>
          {activity.submission && <div className="family-submission"><strong>Entrega · {formatDate(activity.submission.submittedAt, true)}</strong>
            {activity.submission.reviewedAt && <span>Revisada · {formatDate(activity.submission.reviewedAt, true)}</span>}
            {activity.submission.value && <p>{activity.submission.value}</p>}
            {activity.submission.teacherFeedback && <p><strong>Devolución del docente:</strong> {activity.submission.teacherFeedback}</p>}
            {activity.submission.revisions.map((revision) => revision.hasFile && <button className="family-link-button" type="button" key={revision.id}
              onClick={() => runDownload(() => downloadFamilySubmissionFile(
                selectedChild.id, activity.id, activity.submission.id, revision.id, revision.fileName))}>
              Descargar entrega: {revision.fileName}{revision.isCurrent ? " (actual)" : ` (versión ${revision.version})`}
            </button>)}
          </div>}
          {materialErrors[activity.id] && <p className="family-portal-alert">No se pudieron cargar los materiales de esta actividad.</p>}
          {materials[activity.id]?.length > 0 && <div className="family-portal-materials"><h5>Materiales publicados</h5>
            {materials[activity.id].map((material) => <div className="family-portal-material" key={material.id}><span>{material.name}{material.description ? ` — ${material.description}` : ""}</span>
              {material.url ? <a href={material.url} target="_blank" rel="noreferrer">Abrir</a> : <button type="button" onClick={() => runDownload(() => downloadFamilyActivityMaterialFile(selectedChild.id, activity.id, material.id, material.fileName || material.name))}>Descargar</button>}
            </div>)}
          </div>}
          </article>)}
        </section>)}
      </div>}
    </section>;
  }

  function renderGrades() {
    return <section className="family-portal-section"><h3>Calificaciones e informes</h3>
      {grades.length === 0 ? <p className="family-portal-empty">Todavía no hay calificaciones publicadas.</p> : <div className="family-portal-table-wrap"><table className="family-portal-table"><thead><tr><th>Fecha</th><th>Materia</th><th>Evaluación</th><th>Resultado</th><th>Docente</th></tr></thead><tbody>
        {grades.map((grade) => <tr key={grade.id}><td>{formatDate(grade.evaluatedAt)}</td><td>{grade.subjectName}</td><td>{grade.evaluationTitle}</td><td><strong>{grade.value}</strong></td><td>{grade.teacherName}</td></tr>)}
      </tbody></table></div>}
      <p className="family-portal-note">Se muestran resultados individuales publicados por el colegio. No se calculan promedios ni notas finales.</p>
    </section>;
  }

  function renderSchedule() {
    const subjects = [...new Set(schedule.map((item) => item.subjectName))];
    return <section className="family-portal-section"><h3>Materias y horarios</h3>
      {subjects.length === 0 ? <p className="family-portal-empty">No hay horarios semanales registrados para el curso actual.</p> : <>
        <div className="family-subject-list">{subjects.map((subject) => <span key={subject}>{subject}</span>)}</div>
        <div className="family-portal-table-wrap"><table className="family-portal-table"><thead><tr><th>Día</th><th>Horario</th><th>Materia</th><th>Docente</th></tr></thead><tbody>
          {schedule.map((item, index) => <tr key={`${item.dayOfWeek}-${item.startTime}-${index}`}><td>{weekdayLabels[item.dayOfWeek]}</td><td>{item.startTime.slice(0, 5)}–{item.endTime.slice(0, 5)}</td><td>{item.subjectName}</td><td>{item.teacherName}</td></tr>)}
        </tbody></table></div>
      </>}
      <div className="family-portal-empty"><strong>Calendario escolar</strong><br />El sistema todavía no cuenta con un calendario institucional de eventos.</div>
      <p className="family-portal-note">Los horarios provienen de las asignaciones semanales de docentes vinculadas al curso.</p>
    </section>;
  }

  function renderRecords() {
    return <section className="family-portal-section"><h3>Legajo e historial</h3>
      {records.length === 0 ? <p className="family-portal-empty">No hay registros del legajo habilitados para consulta familiar.</p> : <div className="family-record-list">
        {records.map((record) => <article className="family-record" key={record.id}><p className="family-portal-activity-meta">{recordLabels[record.category] ?? "Registro"} · {formatDate(record.recordedAt)}</p><h4>{record.title}</h4><p>{record.description}</p></article>)}
      </div>}
      <p className="family-portal-note">Solo se muestran registros que el colegio habilitó expresamente para la familia.</p>
    </section>;
  }

  function renderSummary() {
    return <>
      <section className="family-portal-section"><div className="family-section-heading"><div><h3>Situación académica</h3><p>Información disponible para {selectedChild.firstName} {selectedChild.lastName}.</p></div></div>
        {attendance.length > 0 && <div className="family-stat-grid">{Object.entries(attendanceSummary).map(([status, count]) => <div className="family-stat" key={status}><span>{attendanceLabels[status]}</span><strong>{count}</strong></div>)}</div>}
        <div className="family-stat-grid">{[
          ["Actividades pendientes", activities.length ? pendingCount : "—"],
          ["Entregadas", activities.length ? submittedCount : "—"],
          ["Con devolución", activities.length ? reviewedCount : "—"],
          ["Calificaciones publicadas", grades.length || "—"]
        ].map(([label, value]) => <div className="family-stat" key={label}><span>{label}</span><strong>{value}</strong></div>)}</div>
        {attendance.length === 0 && <p className="family-portal-empty">Todavía no hay asistencias registradas.</p>}
      </section>
      <section className="family-portal-section"><h3>Accesos directos</h3><div className="family-shortcuts">
        {sections.slice(1).map(([id, label]) => <button type="button" key={id} onClick={() => setSection(id)}>{label}<span aria-hidden="true">→</span></button>)}
      </div></section>
      {grades.length > 0 && <section className="family-portal-section"><div className="family-section-heading"><h3>Últimos resultados publicados</h3><button className="family-link-button" type="button" onClick={() => setSection("grades")}>Ver historial</button></div>
        <div className="family-portal-table-wrap"><table className="family-portal-table"><thead><tr><th>Fecha</th><th>Materia</th><th>Evaluación</th><th>Resultado</th></tr></thead><tbody>
          {grades.slice(0, 3).map((grade) => <tr key={grade.id}><td>{formatDate(grade.evaluatedAt)}</td><td>{grade.subjectName}</td><td>{grade.evaluationTitle}</td><td><strong>{grade.value}</strong></td></tr>)}
        </tbody></table></div>
      </section>}
      {activities.length > 0 && <section className="family-portal-section"><div className="family-section-heading"><h3>Actividad reciente</h3><button className="family-link-button" type="button" onClick={() => setSection("activities")}>Ver actividades</button></div>
        {activities.slice(0, 3).map((activity) => <div className="family-summary-row" key={activity.id}><strong>{activity.title}</strong><span>{activity.subjectName} · {activityState(activity)}</span></div>)}
      </section>}
    </>;
  }

  const renderers = { summary: renderSummary, attendance: renderAttendance, activities: renderActivities, grades: renderGrades, schedule: renderSchedule, records: renderRecords };

  return <main className="family-portal">
    <header className="family-portal-header"><div><p className="family-portal-eyebrow">Familia</p><h2>Portal de familias</h2><p>La trayectoria académica de tus hijos, en un solo lugar.</p></div>
      {children.length > 1 && <label className="family-child-picker"><span>Alumno</span><select value={selectedStudentId} onChange={(event) => {
        setError("");
        setLoadingChildData(true);
        setSelectedStudentId(event.target.value);
      }}>
        {children.map((child) => <option key={child.id} value={child.id}>{child.firstName} {child.lastName}</option>)}
      </select></label>}
    </header>
    {selectedChild && <p className="family-portal-child">{selectedChild.firstName} {selectedChild.lastName} · {selectedChild.courseName}</p>}
    {!loadingChildren && selectedChild && <nav className="family-portal-nav" aria-label="Secciones académicas">{sections.map(([id, label]) => <button key={id} type="button" className={section === id ? "active" : ""} onClick={() => setSection(id)} aria-current={section === id ? "page" : undefined}>{label}</button>)}</nav>}
    {error && <p className="family-portal-alert" role="alert">{error}</p>}
    {loadingChildren && <p role="status">Cargando alumnos vinculados…</p>}
    {!loadingChildren && children.length === 0 && <p className="family-portal-empty">El colegio todavía no vinculó alumnos a esta cuenta.</p>}
    {loadingChildData && <p role="status">Cargando información académica…</p>}
    {!loadingChildData && selectedChild && !error && renderers[section]()}
  </main>;
}
