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

export async function getTeachers() {
  const res = await fetch(`${API_URL}/Teacher`);

  if (!res.ok) {
    throw new Error("Failed to load teachers.");
  }

  return res.json();
}

export async function createTeacher(teacher) {
  const res = await fetch(`${API_URL}/Teacher`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify(teacher)
  });

  if (!res.ok) {
    const error = new Error("Failed to create teacher.");
    error.status = res.status;
    error.message = await res.text();
    throw error;
  }

  return res.json();
}

export async function updateTeacher(id, teacher) {
  const res = await fetch(`${API_URL}/Teacher/${id}`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify(teacher)
  });

  if (!res.ok) {
    const error = new Error("Failed to update teacher.");
    error.status = res.status;
    error.message = await res.text();
    throw error;
  }

  return res.json();
}

export async function deleteTeacher(id) {
  const res = await fetch(`${API_URL}/Teacher/${id}`, {
    method: "DELETE"
  });

  if (!res.ok) {
    const error = new Error("Failed to delete teacher.");
    error.status = res.status;
    error.message = await res.text();
    throw error;
  }
}

async function requestJson(url, options) {
  const res = await fetch(url, options);

  if (!res.ok) {
    const error = new Error(await res.text());
    error.status = res.status;
    throw error;
  }

  if (res.status === 204) return undefined;
  return res.json();
}

export function getSubjects() {
  return requestJson(`${API_URL}/Subject`);
}

export function getTeacherSchedules() {
  return requestJson(`${API_URL}/TeacherAttendance/schedules`);
}

export function getTeacherAttendances() {
  return requestJson(`${API_URL}/TeacherAttendance`);
}

export function createTeacherSchedule(schedule) {
  return requestJson(`${API_URL}/TeacherAttendance/schedules`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(schedule)
  });
}

export function takeTeacherAttendance(attendance) {
  return requestJson(`${API_URL}/TeacherAttendance`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(attendance)
  });
}

export function previewImport(importData) {
  return requestJson(`${API_URL}/Import/preview`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(importData)
  });
}

export function confirmImport(importData) {
  return requestJson(`${API_URL}/Import/confirm`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(importData)
  });
}

export function getActivities({ teacherId, courseId, subjectId, includeInactive = false } = {}) {
  const params = new URLSearchParams();

  if (teacherId) params.append("teacherId", teacherId);
  if (courseId) params.append("courseId", courseId);
  if (subjectId) params.append("subjectId", subjectId);

  params.append("includeInactive", includeInactive);

  const query = params.toString();

  return requestJson(
    `${API_URL}/Activity${query ? `?${query}` : ""}`
  );
}

export function getActivity(id) {
  return requestJson(`${API_URL}/Activity/${id}`);
}

export function createActivity(activity) {
  return requestJson(`${API_URL}/Activity`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(activity)
  });
}

export function updateActivity(id, activity) {
  return requestJson(`${API_URL}/Activity/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(activity)
  });
}

export function publishActivity(id) {
  return requestJson(`${API_URL}/Activity/${id}/publish`, {
    method: "POST"
  });
}

export function getActivityStudents(id) {
  return requestJson(`${API_URL}/Activity/${id}/students`);
}

export function getActivityMaterials(id) {
  return requestJson(`${API_URL}/Activity/${id}/materials`);
}

export function addActivityMaterial(id, material) {
  return requestJson(`${API_URL}/Activity/${id}/materials`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(material)
  });
}

export function deleteActivityMaterial(activityId, materialId) {
  return requestJson(
    `${API_URL}/Activity/${activityId}/materials/${materialId}`,
    {
      method: "DELETE"
    }
  );
}