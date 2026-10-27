import { useCallback, useEffect, useState } from "react";
import {
  downloadActivityMaterialFile,
  downloadStudentSubmissionFile,
  getActivities,
  getActivityMaterials,
  getStudentSubmissionRevisions,
  getStudentSubmissions,
  submitStudentActivity
} from "../services/api";
import { groupActivitiesByDueDate } from "../utils/activityAgenda";
import "./StudentActivities.css";

function formatDate(value) {
  const date = new Date(value);
  if (!value || Number.isNaN(date.getTime())) return "—";
  return new Intl.DateTimeFormat("es-UY", { dateStyle: "medium", timeStyle: "short" }).format(date);
}

function getSubmissionStatus(submission, dueAt) {
  if (!submission) {
    const dueDate = dueAt ? new Date(dueAt) : null;
    if (dueDate && Number.isFinite(dueDate.getTime()) && dueDate.getTime() < Date.now()) {
      return { label: "Pendiente · vencida", className: " is-overdue" };
    }
    return { label: "Pendiente", className: "" };
  }
  if (submission.receivedAt && submission.teacherObservation) {
    return { label: "Con devolución", className: " has-feedback" };
  }
  return { label: "Esperando corrección", className: " is-submitted" };
}

function StudentActivities() {
  const [activities, setActivities] = useState([]);
  const [submissions, setSubmissions] = useState([]);
  const [materials, setMaterials] = useState({});
  const [revisions, setRevisions] = useState({});
  const [expandedHistory, setExpandedHistory] = useState({});
  const [drafts, setDrafts] = useState({});
  const [loading, setLoading] = useState(true);
  const [submittingId, setSubmittingId] = useState(null);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const fetchActivityData = useCallback(async () => {
    const [activityData, submissionData] = await Promise.all([getActivities(), getStudentSubmissions()]);
    const materialEntries = await Promise.all(activityData.map(async (activity) => {
      try { return [activity.id, await getActivityMaterials(activity.id)]; }
      catch { return [activity.id, []]; }
    }));
    return { activityData, submissionData, materials: Object.fromEntries(materialEntries) };
  }, []);

  const applyActivityData = useCallback(({ activityData, submissionData, materials: activityMaterials }) => {
    setActivities(activityData);
    setSubmissions(submissionData);
    setMaterials(activityMaterials);
  }, []);

  const load = useCallback(async () => {
    try {
      applyActivityData(await fetchActivityData());
    } catch (loadError) {
      setError(loadError.message || "No se pudieron cargar tus actividades.");
    } finally {
      setLoading(false);
    }
  }, [applyActivityData, fetchActivityData]);

  const refresh = useCallback(async () => {
    setLoading(true);
    setError("");
    await load();
  }, [load]);

  useEffect(() => {
    let active = true;
    fetchActivityData()
      .then((data) => { if (active) applyActivityData(data); })
      .catch((loadError) => { if (active) setError(loadError.message || "No se pudieron cargar tus actividades."); })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, [applyActivityData, fetchActivityData]);

  const submissionByActivity = Object.fromEntries(submissions.map((item) => [item.activityId, item]));
  const activityGroups = groupActivitiesByDueDate(activities, (activity) => activity.extendedDueAt ?? activity.dueAt);

  function updateDraft(activityId, field, value) {
    setDrafts((current) => ({ ...current, [activityId]: { ...current[activityId], [field]: value } }));
  }

  async function handleSubmit(event, activity) {
    event.preventDefault();
    const draft = drafts[activity.id] ?? {};
    if (!draft.value?.trim() && !draft.file) {
      setError("Escribí una respuesta o seleccioná un archivo para entregar.");
      return;
    }
    setSubmittingId(activity.id);
    setError("");
    setSuccess("");
    try {
      await submitStudentActivity(activity.id, draft);
      setDrafts((current) => ({ ...current, [activity.id]: { value: "", file: null } }));
      setSuccess(`La entrega de “${activity.title}” quedó registrada.`);
      await refresh();
    } catch (submitError) {
      setError(submitError.message || "No se pudo registrar la entrega.");
    } finally {
      setSubmittingId(null);
    }
  }

  async function toggleHistory(submissionId) {
    const isOpen = expandedHistory[submissionId];
    setExpandedHistory((current) => ({ ...current, [submissionId]: !isOpen }));
    if (!isOpen && !revisions[submissionId]) {
      try {
        const data = await getStudentSubmissionRevisions(submissionId);
        setRevisions((current) => ({ ...current, [submissionId]: data }));
      } catch (historyError) {
        setError(historyError.message || "No se pudo cargar el historial de entregas.");
      }
    }
  }

  if (loading) return <main className="student-activities"><p role="status">Cargando tus actividades…</p></main>;

  return (
    <main className="student-activities">
      <header className="student-activities-header">
        <div>
          <p className="student-activities-eyebrow">Espacio del alumno</p>
          <h2>Mis actividades</h2>
          <p>Consultá las consignas, entregá tus trabajos y revisá las devoluciones.</p>
        </div>
        <button className="student-activities-refresh" type="button" onClick={refresh}>Actualizar</button>
      </header>

      {error && <p className="student-activities-alert student-activities-alert--error" role="alert">{error}</p>}
      {success && <p className="student-activities-alert student-activities-alert--success" role="status">{success}</p>}

      {activities.length === 0 ? (
        <div className="student-activities-empty">No tenés actividades publicadas por el momento.</div>
      ) : (
        <section className="student-activities-agenda" aria-label="Agenda de actividades">
          {activityGroups.filter((group) => group.activities.length > 0).map((group) => (
            <section className="student-activities-agenda-group" key={group.id} aria-labelledby={`student-agenda-${group.id}`}>
              <header className="student-activities-agenda-heading">
                <h3 id={`student-agenda-${group.id}`}>{group.label}</h3>
                <span>{group.activities.length}</span>
              </header>
              <div className="student-activities-list">
          {group.activities.map((activity) => {
            const submission = submissionByActivity[activity.id];
            const dueAt = activity.extendedDueAt ?? activity.dueAt;
            const status = getSubmissionStatus(submission, dueAt);
            const draft = drafts[activity.id] ?? {};
            return (
              <article className="student-activity-card" key={activity.id}>
                <div className="student-activity-heading">
                  <div>
                    <p className="student-activity-course">{activity.courseName} · {activity.subjectName}</p>
                    <h3>{activity.title}</h3>
                  </div>
                  <span className={`student-activity-badge${status.className}`}>
                    {status.label}
                  </span>
                </div>
                <p className="student-activity-description">{activity.description}</p>
                <p className="student-activity-meta">Docente: {activity.teacherName} · Vence: {dueAt ? formatDate(dueAt) : "Sin fecha"}</p>

                {materials[activity.id]?.length > 0 && (
                  <section className="student-activity-materials" aria-label="Materiales de la actividad">
                    <h4>Materiales</h4>
                    {materials[activity.id].map((material) => (
                      <div className="student-activity-material" key={material.id}>
                        <span>{material.name}{material.description ? ` — ${material.description}` : ""}</span>
                        {material.url ? (
                          <a href={material.url} target="_blank" rel="noreferrer">Abrir</a>
                        ) : (
                          <button type="button" onClick={() => downloadActivityMaterialFile(activity.id, material.id, material.fileName || material.name)}>Descargar</button>
                        )}
                      </div>
                    ))}
                  </section>
                )}

                {submission && (
                  <section className="student-submission-review" aria-label="Estado de la entrega">
                    <p><strong>Última entrega:</strong> {formatDate(submission.submittedAt)}</p>
                    {submission.currentRevision?.value && <p><strong>Respuesta:</strong> {submission.currentRevision.value}</p>}
                    {submission.currentRevision?.fileName && <p><strong>Archivo:</strong> {submission.currentRevision.fileName}</p>}
                    {submission.teacherObservation ? <p><strong>Devolución docente:</strong> {submission.teacherObservation}</p> : <p className="student-activity-muted">Todavía no hay una devolución del docente.</p>}
                    <button className="student-activity-history-button" type="button" onClick={() => toggleHistory(submission.id)}>
                      {expandedHistory[submission.id] ? "Ocultar historial" : "Ver historial de revisiones"}
                    </button>
                    {expandedHistory[submission.id] && (
                      <ol className="student-revision-list">
                        {(revisions[submission.id] ?? []).map((revision) => (
                          <li key={revision.id}>
                            <span>Versión {revision.version} · {formatDate(revision.submittedAt)}{revision.isCurrent ? " · actual" : ""}</span>
                            {revision.value && <p>{revision.value}</p>}
                            {revision.fileName && <button type="button" onClick={() => downloadStudentSubmissionFile(submission.id, revision.id, revision.fileName)}>Descargar {revision.fileName}</button>}
                          </li>
                        ))}
                      </ol>
                    )}
                  </section>
                )}

                <form className="student-submission-form" onSubmit={(event) => handleSubmit(event, activity)}>
                  <label htmlFor={`submission-value-${activity.id}`}>{submission ? "Nueva versión" : "Tu respuesta"}</label>
                  <textarea id={`submission-value-${activity.id}`} value={draft.value ?? ""} onChange={(event) => updateDraft(activity.id, "value", event.target.value)} placeholder="Escribí tu respuesta…" rows="4" disabled={submittingId === activity.id} />
                  <label htmlFor={`submission-file-${activity.id}`}>Adjuntar archivo (opcional)</label>
                  <input id={`submission-file-${activity.id}`} type="file" onChange={(event) => updateDraft(activity.id, "file", event.target.files?.[0] ?? null)} disabled={submittingId === activity.id} />
                  <button type="submit" disabled={submittingId === activity.id}>
                    {submittingId === activity.id ? "Enviando…" : submission ? "Enviar nueva versión" : "Entregar actividad"}
                  </button>
                </form>
              </article>
            );
          })}
              </div>
            </section>
          ))}
        </section>
      )}
    </main>
  );
}

export default StudentActivities;
