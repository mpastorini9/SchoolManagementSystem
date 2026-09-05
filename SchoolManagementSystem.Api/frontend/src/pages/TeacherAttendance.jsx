import { useEffect, useState } from "react";
import {
  createTeacherSchedule,
  getCourses,
  getSubjects,
  getTeacherAttendances,
  getTeacherSchedules,
  getTeachers,
  takeTeacherAttendance
} from "../services/api";
import "./TeacherAttendance.css";

const days = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];
const dayLabels = {
  Monday: "Lunes", Tuesday: "Martes", Wednesday: "Miércoles", Thursday: "Jueves",
  Friday: "Viernes", Saturday: "Sábado", Sunday: "Domingo"
};

export default function TeacherAttendance() {
  const [teachers, setTeachers] = useState([]);
  const [courses, setCourses] = useState([]);
  const [subjects, setSubjects] = useState([]);
  const [schedules, setSchedules] = useState([]);
  const [attendances, setAttendances] = useState([]);
  const [scheduleForm, setScheduleForm] = useState({ teacherId: "", courseId: "", subjectId: "", dayOfWeek: "Monday", startTime: "08:00", endTime: "09:00" });
  const [attendanceForm, setAttendanceForm] = useState({ teacherScheduleId: "", date: "", status: "Present" });
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [feedback, setFeedback] = useState(null);

  const loadData = async () => {
    const [teachersData, coursesData, subjectsData, schedulesData, attendancesData] = await Promise.all([
      getTeachers(), getCourses(), getSubjects(), getTeacherSchedules(), getTeacherAttendances()
    ]);
    setTeachers(teachersData);
    setCourses(coursesData);
    setSubjects(subjectsData);
    setSchedules(schedulesData);
    setAttendances(attendancesData);
  };

  useEffect(() => {
    const load = async () => {
      try {
        await loadData();
      } catch {
        setFeedback({ type: "error", message: "No se pudo cargar la información de asistencia docente." });
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

  const updateSchedule = (event) => setScheduleForm({ ...scheduleForm, [event.target.name]: event.target.value });
  const updateAttendance = (event) => setAttendanceForm({ ...attendanceForm, [event.target.name]: event.target.value });

  const handleCreateSchedule = async () => {
    if (!scheduleForm.teacherId || !scheduleForm.courseId || !scheduleForm.subjectId) {
      setFeedback({ type: "error", message: "Seleccioná docente, curso y materia." });
      return;
    }
    setSubmitting(true);
    setFeedback(null);
    try {
      await createTeacherSchedule({
        ...scheduleForm,
        teacherId: Number(scheduleForm.teacherId),
        courseId: Number(scheduleForm.courseId),
        subjectId: Number(scheduleForm.subjectId),
        startTime: `${scheduleForm.startTime}:00`,
        endTime: `${scheduleForm.endTime}:00`
      });
      await loadData();
      setFeedback({ type: "success", message: "La actividad docente se creó correctamente." });
    } catch (error) {
      setFeedback({ type: "error", message: error.status === 409 ? "Esa actividad ya está registrada." : "No se pudo crear la actividad docente." });
    } finally {
      setSubmitting(false);
    }
  };

  const handleTakeAttendance = async () => {
    if (!attendanceForm.teacherScheduleId || !attendanceForm.date) {
      setFeedback({ type: "error", message: "Seleccioná una actividad y una fecha." });
      return;
    }
    setSubmitting(true);
    setFeedback(null);
    try {
      await takeTeacherAttendance({ ...attendanceForm, teacherScheduleId: Number(attendanceForm.teacherScheduleId), date: attendanceForm.date });
      await loadData();
      setFeedback({ type: "success", message: "La asistencia docente se registró correctamente." });
    } catch (error) {
      const message = error.status === 409
        ? "Ya existe asistencia para esa actividad y fecha."
        : error.status === 400
          ? "La fecha debe coincidir con el día programado de la actividad."
          : "No se pudo registrar la asistencia docente.";
      setFeedback({ type: "error", message });
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <main className="teacher-attendance-page">
      <header className="teacher-attendance-header">
        <p>Gestión académica</p>
        <h2>Asistencia docente</h2>
        <span>Definí las actividades de clase y registrá la presencia de cada docente.</span>
      </header>

      {feedback && <p className={`teacher-attendance-feedback teacher-attendance-feedback--${feedback.type}`} role={feedback.type === "error" ? "alert" : "status"}>{feedback.message}</p>}

      <section className="teacher-attendance-card">
        <h3>Nueva actividad docente</h3>
        <div className="teacher-attendance-form">
          <select name="teacherId" value={scheduleForm.teacherId} onChange={updateSchedule} disabled={loading || submitting}><option value="">Docente</option>{teachers.map((teacher) => <option key={teacher.id} value={teacher.id}>{teacher.firstName} {teacher.lastName}</option>)}</select>
          <select name="courseId" value={scheduleForm.courseId} onChange={updateSchedule} disabled={loading || submitting}><option value="">Curso</option>{courses.map((course) => <option key={course.id} value={course.id}>{course.name}</option>)}</select>
          <select name="subjectId" value={scheduleForm.subjectId} onChange={updateSchedule} disabled={loading || submitting}><option value="">Materia</option>{subjects.map((subject) => <option key={subject.id} value={subject.id}>{subject.name}</option>)}</select>
          <select name="dayOfWeek" value={scheduleForm.dayOfWeek} onChange={updateSchedule} disabled={loading || submitting}>{days.map((day) => <option key={day} value={day}>{dayLabels[day]}</option>)}</select>
          <input name="startTime" type="time" value={scheduleForm.startTime} onChange={updateSchedule} disabled={loading || submitting} aria-label="Hora de inicio" />
          <input name="endTime" type="time" value={scheduleForm.endTime} onChange={updateSchedule} disabled={loading || submitting} aria-label="Hora de fin" />
          <button type="button" onClick={handleCreateSchedule} disabled={loading || submitting}>Guardar actividad</button>
        </div>
        {!loading && teachers.length === 0 && <p className="teacher-attendance-state">Primero registrá un docente para poder crear una actividad.</p>}
      </section>

      <section className="teacher-attendance-card">
        <h3>Registrar asistencia</h3>
        <div className="teacher-attendance-form teacher-attendance-form--attendance">
          <select name="teacherScheduleId" value={attendanceForm.teacherScheduleId} onChange={updateAttendance} disabled={loading || submitting}><option value="">Actividad programada</option>{schedules.map((schedule) => <option key={schedule.id} value={schedule.id}>{dayLabels[schedule.dayOfWeek]} · {schedule.teacherName} · {schedule.courseName} · {schedule.subjectName} ({schedule.hoursTaught} h)</option>)}</select>
          <input name="date" type="date" value={attendanceForm.date} onChange={updateAttendance} disabled={loading || submitting} />
          <select name="status" value={attendanceForm.status} onChange={updateAttendance} disabled={loading || submitting}><option value="Present">Presente</option><option value="Absent">Ausente</option></select>
          <button type="button" onClick={handleTakeAttendance} disabled={loading || submitting}>Guardar asistencia</button>
        </div>
      </section>

      <section className="teacher-attendance-card">
        <h3>Registro reciente</h3>
        {loading ? <p className="teacher-attendance-state">Cargando información...</p> : attendances.length === 0 ? <p className="teacher-attendance-state">Todavía no hay asistencias docentes registradas.</p> : <div className="teacher-attendance-table-wrap"><table className="teacher-attendance-table"><thead><tr><th>Fecha</th><th>Docente</th><th>Curso</th><th>Materia</th><th>Horas</th><th>Estado</th></tr></thead><tbody>{attendances.map((attendance) => <tr key={attendance.id}><td data-label="Fecha">{attendance.date.slice(0, 10)}</td><td data-label="Docente">{attendance.teacherName}</td><td data-label="Curso">{attendance.courseName}</td><td data-label="Materia">{attendance.subjectName}</td><td data-label="Horas">{attendance.hoursTaught}</td><td data-label="Estado">{attendance.status === "Present" ? "Presente" : "Ausente"}</td></tr>)}</tbody></table></div>}
      </section>
    </main>
  );
}
