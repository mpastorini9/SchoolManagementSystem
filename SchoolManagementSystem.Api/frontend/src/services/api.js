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