import { useEffect, useState } from "react";
import { getCourses, getStudents, takeAttendance } from "../services/api";
import "./Attendance.css";

export default function Attendance() {
  const [courses, setCourses] = useState([]);
  const [students, setStudents] = useState([]);
  const [selectedCourseId, setSelectedCourseId] = useState("");
  const [selectedDate, setSelectedDate] = useState("");
  const [absentStudents, setAbsentStudents] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [feedback, setFeedback] = useState(null);

  useEffect(() => {
    const loadAttendanceData = async () => {
      try {
        const [coursesData, studentsData] = await Promise.all([
          getCourses(),
          getStudents()
        ]);

        setCourses(coursesData);
        setStudents(studentsData);
      } catch {
        setLoadError("No se pudieron cargar los cursos y alumnos.");
      } finally {
        setIsLoading(false);
      }
    };

    loadAttendanceData();
  }, []);

  const selectedCourseStudents = selectedCourseId
    ? students.filter((student) => student.courseId === Number(selectedCourseId))
    : [];

  const handleCourseChange = (event) => {
    setSelectedCourseId(event.target.value);
    setAbsentStudents([]);
    setFeedback(null);
  };

  const handleDateChange = (event) => {
    setSelectedDate(event.target.value);
    setFeedback(null);
  };

  const toggleStudent = (id) => {
    if (absentStudents.includes(id)) {
      setAbsentStudents(absentStudents.filter((studentId) => studentId !== id));
    } else {
      setAbsentStudents([...absentStudents, id]);
    }
  };

  const handleSave = async () => {
    if (!selectedCourseId) {
      setFeedback({
        type: "error",
        message: "Seleccioná un curso antes de guardar la asistencia."
      });
      return;
    }

    if (!selectedDate) {
      setFeedback({
        type: "error",
        message: "Seleccioná una fecha antes de guardar la asistencia."
      });
      return;
    }

    setIsSubmitting(true);
    setFeedback(null);

    try {
      await takeAttendance({
        courseId: Number(selectedCourseId),
        date: selectedDate,
        absentStudentsIds: absentStudents
      });

      setFeedback({
        type: "success",
        message: "La asistencia se registró correctamente."
      });
    } catch (error) {
      let message = "No se pudo guardar la asistencia. Intentá nuevamente.";

      if (error.status === 400) {
        message = "No se pudo registrar la asistencia. Verificá el curso, la fecha y los alumnos seleccionados.";
      } else if (error.status === 404) {
        message = "El curso seleccionado ya no está disponible. Actualizá la pantalla y seleccioná otro curso.";
      } else if (error.status === 409) {
        message = "Ya existe una asistencia registrada para este curso y fecha. No se realizaron cambios.";
      }

      setFeedback({ type: "error", message });
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <main className="attendance-page">
      <header className="attendance-header">
        <p className="attendance-eyebrow">Asistencia diaria</p>
        <h2>Tomar asistencia</h2>
        <p>Seleccioná el curso y la fecha, luego marcá únicamente a los alumnos ausentes.</p>
      </header>

      <section className="attendance-card" aria-label="Registro de asistencia">
        <div className="attendance-fields">
          <div className="attendance-field">
            <label htmlFor="course">Curso</label>
            <select
              id="course"
              value={selectedCourseId}
              onChange={handleCourseChange}
              disabled={isLoading}
            >
              <option value="">Seleccionar curso</option>
              {courses.map((course) => (
                <option key={course.id} value={course.id}>
                  {course.name}
                </option>
              ))}
            </select>
          </div>

          <div className="attendance-field">
            <label htmlFor="attendance-date">Fecha</label>
            <input
              id="attendance-date"
              type="date"
              value={selectedDate}
              onChange={handleDateChange}
              disabled={isLoading}
            />
          </div>
        </div>

        {isLoading && <p className="attendance-state">Cargando cursos y alumnos...</p>}

        {loadError && <p className="attendance-feedback attendance-feedback--error" role="alert">{loadError}</p>}

        {!isLoading && !loadError && !selectedCourseId && (
          <p className="attendance-state">Seleccioná un curso para ver los alumnos.</p>
        )}

        {!isLoading && !loadError && selectedCourseId && selectedCourseStudents.length === 0 && (
          <p className="attendance-state">No hay alumnos asignados a este curso.</p>
        )}

        {!isLoading && !loadError && selectedCourseStudents.length > 0 && (
          <section className="attendance-roster" aria-labelledby="attendance-roster-title">
            <div className="attendance-roster-header">
              <h3 id="attendance-roster-title">Alumnos</h3>
              <p>Marcá solo las ausencias.</p>
            </div>

            <div className="attendance-student-list">
              {selectedCourseStudents.map((student) => {
                const isAbsent = absentStudents.includes(student.id);

                return (
                  <button
                    key={student.id}
                    type="button"
                    className={`attendance-student${isAbsent ? " attendance-student--absent" : ""}`}
                    onClick={() => toggleStudent(student.id)}
                    aria-pressed={isAbsent}
                  >
                    <span className="attendance-student-name">{student.firstName} {student.lastName}</span>
                    <span className="attendance-student-status">
                      {isAbsent ? "Ausente" : "Presente"}
                    </span>
                  </button>
                );
              })}
            </div>
          </section>
        )}

        <div className="attendance-actions">
          <button
            type="button"
            className="attendance-save-button"
            onClick={handleSave}
            disabled={isLoading || Boolean(loadError) || isSubmitting}
          >
            {isSubmitting ? "Guardando asistencia..." : "Guardar asistencia"}
          </button>
        </div>

        {feedback && (
          <p
            className={`attendance-feedback attendance-feedback--${feedback.type}`}
            role={feedback.type === "error" ? "alert" : "status"}
          >
            {feedback.message}
          </p>
        )}
      </section>
    </main>
  );
}
