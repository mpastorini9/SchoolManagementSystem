import { useEffect, useState } from "react";
import {
  getTeachers,
  createTeacher,
  updateTeacher,
  deleteTeacher
} from "../services/api";
import "./Teachers.css";

export default function Teachers() {
  const [teachers, setTeachers] = useState([]);

  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [documentNumber, setDocumentNumber] = useState("");

  const [editingTeacherId, setEditingTeacherId] = useState(null);

  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [loadError, setLoadError] = useState("");
  const [feedback, setFeedback] = useState(null);

  const loadTeachers = async () => {
    try {
      const data = await getTeachers();
      setTeachers(data);
      setLoadError("");
    } catch {
      setLoadError("No se pudieron cargar los docentes.");
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    // The initial load updates state only after the asynchronous request settles.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    loadTeachers();
  }, []);

  const clearForm = () => {
    setFirstName("");
    setLastName("");
    setDocumentNumber("");
    setEditingTeacherId(null);
  };

  const handleCreate = async () => {
    if (
      !firstName.trim() ||
      !lastName.trim() ||
      !documentNumber.trim()
    ) {
      setFeedback({
        type: "error",
        message: "Completá nombre, apellido y documento."
      });

      return;
    }

    setIsSubmitting(true);
    setFeedback(null);

    try {
      await createTeacher({
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        documentNumber: documentNumber.trim()
      });

      clearForm();

      await loadTeachers();

      setFeedback({
        type: "success",
        message: "El docente se registró correctamente."
      });
    } catch (error) {
      if (error.status === 400) {
        setFeedback({
          type: "error",
          message: "Verificá los datos ingresados."
        });
      } else if (error.status === 409) {
        setFeedback({
          type: "error",
          message: "Ya existe un docente registrado con ese documento."
        });
      } else {
        setFeedback({
          type: "error",
          message:
            "No se pudo registrar el docente. Intentá nuevamente."
        });
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleEdit = (teacher) => {
    setEditingTeacherId(teacher.id);
    setFirstName(teacher.firstName);
    setLastName(teacher.lastName);
    setDocumentNumber(teacher.documentNumber);

    setFeedback(null);

    window.scrollTo({
      top: 0,
      behavior: "smooth"
    });
  };

  const handleUpdate = async () => {
    if (
      !firstName.trim() ||
      !lastName.trim() ||
      !documentNumber.trim()
    ) {
      setFeedback({
        type: "error",
        message: "Completá nombre, apellido y documento."
      });

      return;
    }

    setIsSubmitting(true);
    setFeedback(null);

    try {
      await updateTeacher(editingTeacherId, {
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        documentNumber: documentNumber.trim()
      });

      clearForm();

      await loadTeachers();

      setFeedback({
        type: "success",
        message: "El docente se actualizó correctamente."
      });
    } catch (error) {
      if (error.status === 400) {
        setFeedback({
          type: "error",
          message: "Verificá los datos ingresados."
        });
      } else if (error.status === 404) {
        setFeedback({
          type: "error",
          message: "No se encontró el docente."
        });
      } else if (error.status === 409) {
        setFeedback({
          type: "error",
          message:
            "Ya existe otro docente registrado con ese documento."
        });
      } else {
        setFeedback({
          type: "error",
          message:
            "No se pudo actualizar el docente. Intentá nuevamente."
        });
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async (teacher) => {
    const confirmed = window.confirm(
      `¿Querés eliminar al docente ${teacher.firstName} ${teacher.lastName}?`
    );

    if (!confirmed) {
      return;
    }

    setIsSubmitting(true);
    setFeedback(null);

    try {
      await deleteTeacher(teacher.id);

      if (editingTeacherId === teacher.id) {
        clearForm();
      }

      await loadTeachers();

      setFeedback({
        type: "success",
        message: "El docente se eliminó correctamente."
      });
    } catch (error) {
      if (error.status === 404) {
        setFeedback({
          type: "error",
          message: "No se encontró el docente."
        });
      } else {
        setFeedback({
          type: "error",
          message:
            "No se pudo eliminar el docente. Intentá nuevamente."
        });
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancelEdit = () => {
    clearForm();

    setFeedback(null);
  };

  return (
    <main className="teachers-page">
      <header className="teachers-header">
        <p className="teachers-eyebrow">
          Gestión académica
        </p>

        <h2>Docentes</h2>

        <p>
          Registrá docentes y consultá el listado del personal docente.
        </p>
      </header>

      <section
        className="teachers-card"
        aria-label="Gestión de docentes"
      >
        <div className="teachers-section-header">
          <h3>
            {editingTeacherId === null
              ? "Nuevo docente"
              : "Editar docente"}
          </h3>

          <p>
            {editingTeacherId === null
              ? "Completá los datos para registrar un docente."
              : "Modificá los datos del docente seleccionado."}
          </p>
        </div>

        <div className="teacher-form">
          <div className="teacher-field">
            <label htmlFor="teacher-first-name">
              Nombre
            </label>

            <input
              id="teacher-first-name"
              placeholder="Nombre"
              value={firstName}
              onChange={(event) =>
                setFirstName(event.target.value)
              }
              disabled={isSubmitting}
            />
          </div>

          <div className="teacher-field">
            <label htmlFor="teacher-last-name">
              Apellido
            </label>

            <input
              id="teacher-last-name"
              placeholder="Apellido"
              value={lastName}
              onChange={(event) =>
                setLastName(event.target.value)
              }
              disabled={isSubmitting}
            />
          </div>

          <div className="teacher-field">
            <label htmlFor="teacher-document">
              Documento
            </label>

            <input
              id="teacher-document"
              placeholder="Documento"
              value={documentNumber}
              onChange={(event) =>
                setDocumentNumber(event.target.value)
              }
              disabled={isSubmitting}
            />
          </div>

          {editingTeacherId === null ? (
            <button
              type="button"
              onClick={handleCreate}
              disabled={isSubmitting}
            >
              {isSubmitting
                ? "Registrando..."
                : "Crear docente"}
            </button>
          ) : (
            <>
              <button
                type="button"
                onClick={handleUpdate}
                disabled={isSubmitting}
              >
                {isSubmitting
                  ? "Guardando..."
                  : "Guardar cambios"}
              </button>

              <button
                type="button"
                className="teacher-cancel-button"
                onClick={handleCancelEdit}
                disabled={isSubmitting}
              >
                Cancelar
              </button>
            </>
          )}
        </div>

        {feedback && (
          <div
            className={`teachers-feedback teachers-feedback--${feedback.type}`}
            role={
              feedback.type === "error"
                ? "alert"
                : "status"
            }
          >
            {feedback.message}
          </div>
        )}

        <section
          className="teachers-list"
          aria-labelledby="teachers-list-title"
        >
          <div className="teachers-section-header teachers-section-header--list">
            <h3 id="teachers-list-title">
              Listado de docentes
            </h3>

            <p>
              {teachers.length} registrados
            </p>
          </div>

          {isLoading && (
            <div className="teachers-state">
              Cargando docentes...
            </div>
          )}

          {loadError && (
            <div className="teachers-state teachers-state--error">
              {loadError}
            </div>
          )}

          {!isLoading &&
            !loadError &&
            teachers.length === 0 && (
              <div className="teachers-state">
                Todavía no hay docentes registrados.
              </div>
            )}

          {!isLoading &&
            !loadError &&
            teachers.length > 0 && (
              <table className="teacher-table">
                <thead>
                  <tr>
                    <th>Nombre</th>
                    <th>Apellido</th>
                    <th>Documento</th>
                    <th>Acciones</th>
                  </tr>
                </thead>

                <tbody>
                  {teachers.map((teacher) => (
                    <tr key={teacher.id}>
                      <td data-label="Nombre">
                        {teacher.firstName}
                      </td>

                      <td data-label="Apellido">
                        {teacher.lastName}
                      </td>

                      <td data-label="Documento">
                        {teacher.documentNumber}
                      </td>

                      <td
                        className="teacher-actions"
                        data-label="Acciones"
                      >
                        <button
                          type="button"
                          className="teacher-edit-button"
                          onClick={() =>
                            handleEdit(teacher)
                          }
                          disabled={isSubmitting}
                          aria-label={`Editar a ${teacher.firstName} ${teacher.lastName}`}
                        >
                          Editar
                        </button>

                        <button
                          type="button"
                          className="teacher-delete-button"
                          onClick={() =>
                            handleDelete(teacher)
                          }
                          disabled={isSubmitting}
                          aria-label={`Eliminar a ${teacher.firstName} ${teacher.lastName}`}
                        >
                          Eliminar
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
        </section>
      </section>
    </main>
  );
}
