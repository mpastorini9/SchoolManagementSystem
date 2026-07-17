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
    <div className="card">

      <h2>Alumnos</h2>

      <div className="student-form">

        <input
          placeholder="Nombre"
          value={firstName}
          onChange={(e) => setFirstName(e.target.value)}
        />

        <input
          placeholder="Apellido"
          value={lastName}
          onChange={(e) => setLastName(e.target.value)}
        />

        <select
          value={courseId}
          onChange={(e) => setCourseId(Number(e.target.value))}
        >
          {courses.map((course) => (
            <option
              key={course.id}
              value={course.id}
            >
              {course.name}
            </option>
          ))}
        </select>

        <button onClick={handleCreate}>
          Crear alumno
        </button>

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

              <td>{s.firstName}</td>

              <td>{s.lastName}</td>

              <td>{s.course?.name}</td>

              <td>
                <button className="edit-btn">✏️</button>
                <button className="delete-btn">🗑️</button>
              </td>

            </tr>

          ))}

        </tbody>

      </table>

    </div>
  );
}