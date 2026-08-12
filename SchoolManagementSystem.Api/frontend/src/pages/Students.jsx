import { useEffect, useState } from "react";
import { getStudents, createStudent, getCourses } from "../services/api";
import "./Students.css";

export default function Students() {
  // Lista de alumnos
  const [students, setStudents] = useState([]);

  // Lista de cursos
  const [courses, setCourses] = useState([]);

  // Datos del formulario
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [courseId, setCourseId] = useState("");

  // Se ejecuta una sola vez al abrir la pantalla
  useEffect(() => {
    loadStudents();
    loadCourses();
  }, []);

  // Obtiene los alumnos
  const loadStudents = async () => {
    const data = await getStudents();
    setStudents(data);
  };

  // Obtiene los cursos
  const loadCourses = async () => {
    const data = await getCourses();

    setCourses(data);

    // Selecciona automáticamente el primer curso
    if (data.length > 0) {
      setCourseId(data[0].id);
    }

    console.log(data);
  };

  // Crear alumno
  const handleCreate = async () => {
    if (!firstName || !lastName) return;

    const newStudent = {
      firstName,
      lastName,
      documentNumber: "000",
      dateOfBirth: "2000-01-01",
      courseId
    };

    await createStudent(newStudent);

    setFirstName("");
    setLastName("");

    if (courses.length > 0) {
      setCourseId(courses[0].id);
    }

    loadStudents();
  };

  return (
    <main className="students-page">
      <header className="students-header">
        <p className="students-eyebrow">Gestión académica</p>
        <h2>Alumnos</h2>
        <p>Registrá alumnos y consultá el listado asignado a cada curso.</p>
      </header>

      <section className="students-card" aria-label="Gestión de alumnos">
        <div className="students-section-header">
          <h3>Nuevo alumno</h3>
          <p>Completá los datos para registrarlo en un curso.</p>
        </div>

        <div className="student-form">
          <div className="student-field">
            <label htmlFor="student-first-name">Nombre</label>
            <input
              id="student-first-name"
              placeholder="Nombre"
              value={firstName}
              onChange={(e) => setFirstName(e.target.value)}
            />
          </div>

          <div className="student-field">
            <label htmlFor="student-last-name">Apellido</label>
            <input
              id="student-last-name"
              placeholder="Apellido"
              value={lastName}
              onChange={(e) => setLastName(e.target.value)}
            />
          </div>

          <div className="student-field">
            <label htmlFor="student-course">Curso</label>
            <select
              id="student-course"
              value={courseId}
              onChange={(e) => setCourseId(Number(e.target.value))}
            >
              {courses.map((course) => (
                <option key={course.id} value={course.id}>
                  {course.name}
                </option>
              ))}
            </select>
          </div>

          <button type="button" onClick={handleCreate}>
            Crear alumno
          </button>
        </div>

        <section className="students-list" aria-labelledby="students-list-title">
          <div className="students-section-header students-section-header--list">
            <h3 id="students-list-title">Listado de alumnos</h3>
            <p>{students.length} registrados</p>
          </div>

          <table className="student-table">
            <thead>
              <tr>
                <th>Nombre</th>
                <th>Apellido</th>
                <th>Curso</th>
                <th>Acciones</th>
              </tr>
            </thead>

            <tbody>
              {students.map((s) => (
                <tr key={s.id}>
                  <td data-label="Nombre">{s.firstName}</td>
                  <td data-label="Apellido">{s.lastName}</td>
                  <td data-label="Curso">{s.course?.name}</td>
                  <td className="student-actions" data-label="Acciones">
                    <button
                      type="button"
                      className="edit-btn"
                      aria-label={`Editar a ${s.firstName} ${s.lastName}`}
                    >
                      Editar
                    </button>
                    <button
                      type="button"
                      className="delete-btn"
                      aria-label={`Eliminar a ${s.firstName} ${s.lastName}`}
                    >
                      Eliminar
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </section>
      </section>
    </main>
  );
}
