import { useState } from "react";

export default function Attendance() {
  const students = [
    { id: 1, name: "Juan Perez" },
    { id: 2, name: "Pedro Gomez" },
    { id: 3, name: "Lucia Lopez" },
    { id: 4, name: "Maria Gimenez" }
  ];

  const [absentStudents, setAbsentStudents] = useState([]);

  const toggleStudent = (id) => {
    if (absentStudents.includes(id)) {
      setAbsentStudents(absentStudents.filter(x => x !== id));
    } else {
      setAbsentStudents([...absentStudents, id]);
    }
  };

  return (
    <div>

      <h2>Tomar asistencia</h2>

      <select>
        <option>2°D</option>
        <option>2°C</option>
        <option>3°B</option>
      </select>

      <br /><br />

      {students.map(student => (

        <div
          key={student.id}
          style={{
            display: "flex",
            justifyContent: "space-between",
            width: "350px",
            marginBottom: "10px"
          }}
        >

          <span>{student.name}</span>

          <input
            type="checkbox"
            checked={absentStudents.includes(student.id)}
            onChange={() => toggleStudent(student.id)}
          />

        </div>

      ))}

      <br />

      <button>
        Guardar asistencia
      </button>

    </div>
  );
}