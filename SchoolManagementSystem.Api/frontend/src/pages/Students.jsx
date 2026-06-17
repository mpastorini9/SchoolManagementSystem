import { useEffect, useState } from "react";
import { getStudents, createStudent } from "../services/api";

export default function Students() {
  const [students, setStudents] = useState([]);

  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");

  useEffect(() => {
    loadStudents();
  }, []);

  const loadStudents = async () => {
    const data = await getStudents();
    setStudents(data);
  };

  const handleCreate = async () => {
    const newStudent = {
      firstName,
      lastName,
      documentNumber: "000",
      dateOfBirth: "2000-01-01",
      courseId: 1
    };

    await createStudent(newStudent);

    setFirstName("");
    setLastName("");

    loadStudents();
  };

  return (
    <div>
      <h2>Alumnos</h2>

      <div>
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

        <button onClick={handleCreate}>
          Crear Alumno
        </button>
      </div>

      <hr />

      {students.map((s) => (
        <div key={s.id}>
          {s.firstName} {s.lastName}
        </div>
      ))}
    </div>
  );
}