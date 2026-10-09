import { useEffect, useMemo, useState } from "react";
import {
  getActivities,
  getActivity,
  getStudents,
  getCourses,
  getTeachers,
  getSubjects,
  getTeacherSchedules,
  createActivity,
  updateActivity,
  publishActivity
} from "../services/api";
import "./Activities.css";

const initialForm = {
  teacherId: "",
  courseId: "",
  subjectId: "",
  title: "",
  description: "",
  dueAt: "",
  targetType: 1,
  targetStudentId: ""
};

function Activities() {
  const [activities, setActivities] = useState([]);
  const [teachers, setTeachers] = useState([]);
  const [courses, setCourses] = useState([]);
  const [subjects, setSubjects] = useState([]);
  const [students, setStudents] = useState([]);
  const [teacherSchedules, setTeacherSchedules] = useState([]);

  const [showForm, setShowForm] = useState(false);
  const [showReview, setShowReview] = useState(false);

  const [form, setForm] = useState(initialForm);
  const [currentDraft, setCurrentDraft] = useState(null);

  const [loading, setLoading] = useState(true);
  const [loadingFormData, setLoadingFormData] = useState(false);
  const [savingDraft, setSavingDraft] = useState(false);
  const [publishing, setPublishing] = useState(false);

  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  useEffect(() => {
    loadActivities();
  }, []);

  async function loadActivities() {
    try {
      setLoading(true);
      setError("");

      const data = await getActivities();
      setActivities(data);
    } catch (err) {
      setError(getActivityErrorMessage(err, "No se pudieron cargar las actividades."));
    } finally {
      setLoading(false);
    }
  }

  async function openCreateForm() {
    try {
      setShowForm(true);
      setShowReview(false);
      setError("");
      setSuccess("");
      setLoadingFormData(true);

      const [
        teachersData,
        coursesData,
        subjectsData,
        studentsData,
        schedulesData
      ] = await Promise.all([
        getTeachers(),
        getCourses(),
        getSubjects(),
        getStudents(),
        getTeacherSchedules()
      ]);

      setTeachers(teachersData);
      setCourses(coursesData);
      setSubjects(subjectsData);
      setStudents(studentsData);
      setTeacherSchedules(schedulesData);

      setForm(initialForm);
      setCurrentDraft(null);
    } catch (err) {
      setError(getActivityErrorMessage(err, "No se pudieron cargar los datos necesarios."));
    } finally {
      setLoadingFormData(false);
    }
  }

  async function openEditForm(activityId) {
    try {
      setShowForm(true);
      setShowReview(false);
      setError("");
      setSuccess("");
      setLoadingFormData(true);

      const [activity, teachersData, coursesData, subjectsData, studentsData, schedulesData] = await Promise.all([
        getActivity(activityId),
        getTeachers(),
        getCourses(),
        getSubjects(),
        getStudents(),
        getTeacherSchedules()
      ]);

      setTeachers(teachersData);
      setCourses(coursesData);
      setSubjects(subjectsData);
      setStudents(studentsData);
      setTeacherSchedules(schedulesData);
      setForm({
        teacherId: String(activity.teacherId),
        courseId: String(activity.courseId),
        subjectId: String(activity.subjectId),
        title: activity.title,
        description: activity.description,
        dueAt: toDateTimeLocalValue(activity.dueAt),
        targetType: Number(activity.targetType),
        targetStudentId: activity.targetStudentId ? String(activity.targetStudentId) : ""
      });
      setCurrentDraft(activity);
    } catch (err) {
      setError(getActivityErrorMessage(err, "No se pudo cargar el borrador."));
    } finally {
      setLoadingFormData(false);
    }
  }

  function closeForm() {
    const hadSavedDraft = currentDraft !== null;
    setShowForm(false);
    setShowReview(false);
    setForm(initialForm);
    setCurrentDraft(null);
    setError("");
    if (hadSavedDraft) loadActivities();
  }

  function handleChange(event) {
    const { name, value } = event.target;

    setForm((current) => ({
      ...current,
      [name]: value
    }));
  }

  function handleTeacherChange(event) {
    const teacherId = event.target.value;

    setForm((current) => ({
      ...current,
      teacherId,
      courseId: "",
      subjectId: "",
      targetStudentId: ""
    }));
  }

  function handleCourseChange(event) {
    const courseId = event.target.value;

    setForm((current) => ({
      ...current,
      courseId,
      subjectId: "",
      targetStudentId: ""
    }));
  }

  function handleTargetTypeChange(event) {
    const targetType = Number(event.target.value);

    setForm((current) => ({
      ...current,
      targetType,
      targetStudentId: ""
    }));
  }

  function getDueDatePart() {
    if (!form.dueAt) {
      return "";
    }

    return form.dueAt.slice(0, 10);
  }

  function getDueTimePart() {
    if (!form.dueAt) {
      return "";
    }

    return form.dueAt.slice(11, 16);
  }

  function handleDueDateChange(event) {
    const date = event.target.value;
    const time = getDueTimePart();

    setForm((current) => ({
      ...current,
      dueAt: date ? `${date}T${time || "00:00"}` : ""
    }));
  }

  function handleDueTimeChange(event) {
    const time = event.target.value;
    const date = getDueDatePart();

    setForm((current) => ({
      ...current,
      dueAt: date ? `${date}T${time || "00:00"}` : ""
    }));
  }

  const selectedTeacher = useMemo(() => {
    return teachers.find(
      (teacher) => teacher.id === Number(form.teacherId)
    );
  }, [teachers, form.teacherId]);

  const selectedCourse = useMemo(() => {
    return courses.find(
      (course) => course.id === Number(form.courseId)
    );
  }, [courses, form.courseId]);

  const selectedSubject = useMemo(() => {
    return subjects.find(
      (subject) => subject.id === Number(form.subjectId)
    );
  }, [subjects, form.subjectId]);

  const selectedStudent = useMemo(() => {
    return students.find(
      (student) => student.id === Number(form.targetStudentId)
    );
  }, [students, form.targetStudentId]);

  const availableCourses = useMemo(() => {
    if (!form.teacherId) {
      return [];
    }

    const teacherId = Number(form.teacherId);

    const courseIds = new Set(
      teacherSchedules
        .filter((schedule) => schedule.teacherId === teacherId)
        .map((schedule) => schedule.courseId)
    );

    return courses.filter((course) => courseIds.has(course.id));
  }, [form.teacherId, teacherSchedules, courses]);

  const availableSubjects = useMemo(() => {
    if (!form.teacherId || !form.courseId) {
      return [];
    }

    const teacherId = Number(form.teacherId);
    const courseId = Number(form.courseId);

    const subjectIds = new Set(
      teacherSchedules
        .filter(
          (schedule) =>
            schedule.teacherId === teacherId &&
            schedule.courseId === courseId
        )
        .map((schedule) => schedule.subjectId)
    );

    return subjects.filter((subject) => subjectIds.has(subject.id));
  }, [
    form.teacherId,
    form.courseId,
    teacherSchedules,
    subjects
  ]);

  const availableStudents = useMemo(() => {
    if (!form.courseId) {
      return [];
    }

    const courseId = Number(form.courseId);

    return students.filter(
      (student) => student.courseId === courseId
    );
  }, [form.courseId, students]);

  function validateForm() {
    if (!form.teacherId) {
      return "Seleccioná un docente.";
    }

    if (!form.courseId) {
      return "Seleccioná un curso.";
    }

    if (!form.subjectId) {
      return "Seleccioná una materia.";
    }

    if (!form.title.trim()) {
      return "Ingresá un título.";
    }

    if (!form.description.trim()) {
      return "Ingresá una descripción.";
    }

    if (!form.dueAt) {
      return "Ingresá una fecha y hora de entrega.";
    }

    if (form.targetType === 2 && !form.targetStudentId) {
      return "Seleccioná el alumno destinatario.";
    }

    const teacherId = Number(form.teacherId);
    const courseId = Number(form.courseId);
    const subjectId = Number(form.subjectId);

    const validAssignment = teacherSchedules.some(
      (schedule) =>
        schedule.teacherId === teacherId &&
        schedule.courseId === courseId &&
        schedule.subjectId === subjectId
    );

    if (!validAssignment) {
      return "El docente no tiene asignada esa materia para ese curso.";
    }

    if (form.targetType === 2) {
      const student = students.find(
        (item) => item.id === Number(form.targetStudentId)
      );

      if (!student || student.courseId !== courseId) {
        return "El alumno seleccionado no pertenece al curso.";
      }
    }

    return "";
  }

  async function handleContinueToReview(event) {
    event.preventDefault();

    const validationError = validateForm();

    if (validationError) {
      setError(validationError);
      return;
    }

    try {
      setSavingDraft(true);
      setError("");
      setSuccess("");

      const payload = {
        teacherId: Number(form.teacherId),
        courseId: Number(form.courseId),
        subjectId: Number(form.subjectId),
        title: form.title.trim(),
        description: form.description.trim(),
        dueAt: form.dueAt,
        targetType: Number(form.targetType),
        targetStudentId:
          Number(form.targetType) === 2
            ? Number(form.targetStudentId)
            : null
      };

      let draft;

      if (currentDraft) {
        draft = await updateActivity(currentDraft.id, payload);
      } else {
        draft = await createActivity(payload);
      }

      setCurrentDraft(draft);
      setShowReview(true);
    } catch (err) {
      setError(getActivityErrorMessage(err, "No se pudo guardar la actividad."));
    } finally {
      setSavingDraft(false);
    }
  }

  async function handlePublish() {
    if (!currentDraft) {
      setError("No hay una actividad en revisión.");
      return;
    }

    try {
      setPublishing(true);
      setError("");
      setSuccess("");

      await publishActivity(currentDraft.id);

      setShowReview(false);
      setShowForm(false);
      setCurrentDraft(null);
      setForm(initialForm);

      setSuccess("Actividad publicada correctamente.");

      await loadActivities();
    } catch (err) {
      setError(getActivityErrorMessage(err, "No se pudo enviar la actividad."));
    } finally {
      setPublishing(false);
    }
  }

  function formatDate(dateValue) {
    if (!dateValue) {
      return "-";
    }

    return new Date(dateValue).toLocaleString("es-AR", {
      dateStyle: "short",
      timeStyle: "short"
    });
  }

  function getActivityErrorMessage(error, fallback) {
    if (error instanceof TypeError) {
      return "No se pudo conectar con el servidor. Revisá la conexión e intentá nuevamente.";
    }
    if (error.status === 404) {
      return error.message || "No se encontró la actividad o alguno de los datos seleccionados.";
    }
    if (error.status === 409) {
      return error.message || "El borrador cambió de estado y ya no se puede modificar.";
    }
    if (error.status === 400) {
      return error.message || "Revisá los datos ingresados e intentá nuevamente.";
    }
    return error.message || fallback;
  }

  function toDateTimeLocalValue(value) {
    if (!value) return "";
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return "";
    return new Date(date.getTime() - date.getTimezoneOffset() * 60000)
      .toISOString()
      .slice(0, 16);
  }

  function getTargetDescription(activity) {
    if (activity.targetType === 2) {
      return activity.targetStudentName || "Alumno específico";
    }

    return "Todo el curso";
  }

  return (
    <section className="activities-page">
      {!showForm && (
        <div className="activities-header">
          <div>
            <h2>Actividades Académicas</h2>
            <p>
              Creá, asigná y gestioná las actividades de los alumnos.
            </p>
          </div>

          <button
            type="button"
            className="activities-button activities-button-primary"
            onClick={openCreateForm}
          >
            + Nueva actividad
          </button>
        </div>
      )}

      {error && (
        <div
          className="activities-alert activities-alert-error"
          role="alert"
        >
          {error}
        </div>
      )}

      {success && (
        <div
          className="activities-alert activities-alert-success"
          role="status"
        >
          {success}
        </div>
      )}

      {loading && (
        <div className="activities-loading">
          Cargando actividades...
        </div>
      )}

      {!loading && !showForm && activities.length === 0 && (
        <div className="activity-empty">
          <p>No hay actividades registradas.</p>
        </div>
      )}

      {!loading && !showForm && activities.length > 0 && (
        <div className="activities-list">
          {activities.map((activity) => (
            <article
              className="activity-card"
              key={activity.id}
            >
              <h3>{activity.title}</h3>

              <p>
                <strong>Materia:</strong>{" "}
                {activity.subjectName}
              </p>

              <p>
                <strong>Curso:</strong>{" "}
                {activity.courseName}
              </p>

              <p>
                <strong>Docente:</strong>{" "}
                {activity.teacherName}
              </p>

              <p>
                <strong>Entrega:</strong>{" "}
                {formatDate(activity.dueAt)}
              </p>

              <p>
                <strong>Destinatarios:</strong>{" "}
                {getTargetDescription(activity)}
              </p>

              <span className="activity-status">
                {activity.publishedAt
                  ? "PUBLICADA"
                  : "BORRADOR"}
              </span>
              {!activity.publishedAt && (
                <button
                  type="button"
                  className="activities-button activities-button-secondary"
                  onClick={() => openEditForm(activity.id)}
                  disabled={loadingFormData}
                >
                  Editar borrador
                </button>
              )}
            </article>
          ))}
        </div>
      )}

      {showForm && !showReview && (
        <div className="activities-panel">
          <div className="activities-panel-header">
            <h3>{currentDraft ? "Editar borrador" : "Nueva actividad"}</h3>
            <p>
              Completá los datos de la actividad y luego revisalos
              antes de enviarla.
            </p>
          </div>

          {loadingFormData ? (
            <div className="activities-loading">
              Cargando datos...
            </div>
          ) : (
            <form
              className="activities-form"
              onSubmit={handleContinueToReview}
            >
              <div className="activities-form-section">
                <h4 className="activities-form-section-title">
                  Información académica
                </h4>

                <div className="activities-form-grid">
                  <div className="activities-field">
                    <label htmlFor="teacherId">
                      Docente
                    </label>

                    <select
                      id="teacherId"
                      name="teacherId"
                      value={form.teacherId}
                      onChange={handleTeacherChange}
                    >
                      <option value="">
                        Seleccionar docente
                      </option>

                      {teachers
                        .filter(
                          (teacher) =>
                            teacher.isActive !== false
                        )
                        .map((teacher) => (
                          <option
                            key={teacher.id}
                            value={teacher.id}
                          >
                            {teacher.firstName}{" "}
                            {teacher.lastName}
                          </option>
                        ))}
                    </select>
                  </div>

                  <div className="activities-field">
                    <label htmlFor="courseId">
                      Curso
                    </label>

                    <select
                      id="courseId"
                      name="courseId"
                      value={form.courseId}
                      onChange={handleCourseChange}
                      disabled={!form.teacherId}
                    >
                      <option value="">
                        Seleccionar curso
                      </option>

                      {availableCourses.map((course) => (
                        <option
                          key={course.id}
                          value={course.id}
                        >
                          {course.name}
                        </option>
                      ))}
                    </select>
                  </div>

                  <div className="activities-field">
                    <label htmlFor="subjectId">
                      Materia
                    </label>

                    <select
                      id="subjectId"
                      name="subjectId"
                      value={form.subjectId}
                      onChange={handleChange}
                      disabled={!form.courseId}
                    >
                      <option value="">
                        Seleccionar materia
                      </option>

                      {availableSubjects.map((subject) => (
                        <option
                          key={subject.id}
                          value={subject.id}
                        >
                          {subject.name}
                        </option>
                      ))}
                    </select>
                  </div>
                </div>
              </div>

              <div className="activities-form-section">
                <h4 className="activities-form-section-title">
                  Contenido
                </h4>

                <div className="activities-form-grid">
                  <div className="activities-field activities-field-full">
                    <label htmlFor="title">
                      Título
                    </label>

                    <input
                      id="title"
                      name="title"
                      type="text"
                      value={form.title}
                      onChange={handleChange}
                      placeholder="Ej. Trabajo práctico sobre Revolución Industrial"
                    />
                  </div>

                  <div className="activities-field activities-field-full">
                    <label htmlFor="description">
                      Descripción
                    </label>

                    <textarea
                      id="description"
                      name="description"
                      value={form.description}
                      onChange={handleChange}
                      rows="5"
                      placeholder="Explicá la actividad, consignas, objetivos o indicaciones para los alumnos..."
                    />
                  </div>
                </div>
              </div>

              <div className="activities-form-section">
                <h4 className="activities-form-section-title">
                  Entrega
                </h4>

                <div className="activities-form-grid">
                  <div className="activities-field">
                    <label htmlFor="dueDate">
                      Fecha de entrega
                    </label>

                    <input
                      id="dueDate"
                      name="dueDate"
                      type="date"
                      value={getDueDatePart()}
                      onChange={handleDueDateChange}
                    />
                  </div>

                  <div className="activities-field">
                    <label htmlFor="dueTime">
                      Hora límite
                    </label>

                    <input
                      id="dueTime"
                      name="dueTime"
                      type="time"
                      value={getDueTimePart()}
                      onChange={handleDueTimeChange}
                    />
                  </div>
                </div>
              </div>

              <div className="activities-form-section">
                <h4 className="activities-form-section-title">
                  Destinatarios
                </h4>

                <div className="activities-form-grid">
                  <div className="activities-field">
                    <label htmlFor="targetType">
                      Destinatarios
                    </label>

                    <select
                      id="targetType"
                      name="targetType"
                      value={form.targetType}
                      onChange={handleTargetTypeChange}
                    >
                      <option value={1}>
                        Todo el curso
                      </option>

                      <option value={2}>
                        Un alumno específico
                      </option>
                    </select>
                  </div>

                  {Number(form.targetType) === 2 && (
                    <div className="activities-field">
                      <label htmlFor="targetStudentId">
                        Alumno
                      </label>

                      <select
                        id="targetStudentId"
                        name="targetStudentId"
                        value={form.targetStudentId}
                        onChange={handleChange}
                      >
                        <option value="">
                          Seleccionar alumno
                        </option>

                        {availableStudents.map((student) => (
                          <option
                            key={student.id}
                            value={student.id}
                          >
                            {student.firstName}{" "}
                            {student.lastName}
                          </option>
                        ))}
                      </select>
                    </div>
                  )}
                </div>
              </div>

              <div className="activities-form-actions">
                <button
                  type="button"
                  className="activities-button activities-button-secondary"
                  onClick={closeForm}
                  disabled={savingDraft}
                >
                  Cancelar
                </button>

                <button
                  type="submit"
                  className="activities-button activities-button-primary"
                  disabled={savingDraft}
                >
                  {savingDraft
                    ? "Guardando..."
                    : "Continuar a revisión"}
                </button>
              </div>
            </form>
          )}
        </div>
      )}

      {showForm && showReview && (
        <div className="activities-panel">
          <div className="activities-review">
            <div className="activities-review-header">
              <h3>Revisar actividad</h3>

              <p>
                Revisá los datos antes de enviar la actividad.
                Una vez enviada, quedará publicada para los
                destinatarios seleccionados.
              </p>
            </div>

            <div className="activities-review-card">
              <p>
                <strong>Docente:</strong>{" "}
                {selectedTeacher
                  ? `${selectedTeacher.firstName} ${selectedTeacher.lastName}`
                  : "-"}
              </p>

              <p>
                <strong>Curso:</strong>{" "}
                {selectedCourse?.name || "-"}
              </p>

              <p>
                <strong>Materia:</strong>{" "}
                {selectedSubject?.name || "-"}
              </p>

              <p>
                <strong>Título:</strong>{" "}
                {form.title}
              </p>

              <p>
                <strong>Descripción:</strong>{" "}
                {form.description}
              </p>

              <p>
                <strong>Fecha de entrega:</strong>{" "}
                {formatDate(form.dueAt)}
              </p>

              <p>
                <strong>Destinatarios:</strong>{" "}
                {Number(form.targetType) === 1
                  ? `Todo el curso (${availableStudents.length} alumnos)`
                  : selectedStudent
                    ? `${selectedStudent.firstName} ${selectedStudent.lastName}`
                    : "-"}
              </p>
            </div>

            <div className="activities-review-actions">
              <button
                type="button"
                className="activities-button activities-button-secondary"
                onClick={() => {
                  setShowReview(false);
                  setError("");
                }}
                disabled={publishing}
              >
                Volver a editar
              </button>

              <button
                type="button"
                className="activities-button activities-button-primary"
                onClick={handlePublish}
                disabled={publishing}
              >
                {publishing
                  ? "Enviando..."
                  : "Enviar actividad"}
              </button>
            </div>
          </div>
        </div>
      )}
    </section>
  );
}

export default Activities;
