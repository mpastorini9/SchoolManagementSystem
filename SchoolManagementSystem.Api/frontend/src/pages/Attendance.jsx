import { useEffect, useState } from "react";
import { getCourses, getStudents, takeAttendance } from "../services/api";

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
    <div>
      <h2>Tomar asistencia</h2>

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

      <label htmlFor="attendance-date">Fecha</label>
      <input
        id="attendance-date"
        type="date"
        value={selectedDate}
        onChange={handleDateChange}
        disabled={isLoading}
      />

      {isLoading && <p>Cargando cursos y alumnos...</p>}

      {loadError && <p role="alert">{loadError}</p>}

      {!isLoading && !loadError && !selectedCourseId && (
        <p>Seleccioná un curso para ver los alumnos.</p>
      )}

      {!isLoading && !loadError && selectedCourseId && selectedCourseStudents.length === 0 && (
        <p>No hay alumnos asignados a este curso.</p>
      )}

      {!isLoading && !loadError && selectedCourseStudents.map((student) => (
        <label key={student.id}>
          <span>{student.firstName} {student.lastName}</span>
          <input
            type="checkbox"
            checked={absentStudents.includes(student.id)}
            onChange={() => toggleStudent(student.id)}
          />
          Ausente
        </label>
      ))}

      <button
        type="button"
        onClick={handleSave}
        disabled={isLoading || Boolean(loadError) || isSubmitting}
      >
        {isSubmitting ? "Guardando asistencia..." : "Guardar asistencia"}
      </button>

      {feedback && (
        <p role={feedback.type === "error" ? "alert" : "status"}>
          {feedback.message}
        </p>
      )}
    </div>
  );
}
