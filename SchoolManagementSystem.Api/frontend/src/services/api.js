const API_URL = "http://localhost:5003/api"; 

export async function getStudents() {
  const res = await fetch(`${API_URL}/Student`);
  return res.json();
}
export async function createStudent(student) {
  const res = await fetch(`${API_URL}/Student`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify(student)
  });

  return res.json();
}
export async function getCourses() {
    const res = await fetch(`${API_URL}/Course`);
    return res.json();
}

export async function takeAttendance({ courseId, date, absentStudentsIds }) {
  const res = await fetch(`${API_URL}/Attendance/take-attendance`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify({ courseId, date, absentStudentsIds })
  });

  if (!res.ok) {
    const error = new Error("Attendance registration failed.");
    error.status = res.status;
    throw error;
  }
}
