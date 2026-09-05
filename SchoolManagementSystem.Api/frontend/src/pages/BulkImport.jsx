import { useState } from "react";
import { confirmImport, previewImport } from "../services/api";
import "./BulkImport.css";

const templates = {
  courses: "name\n2°A",
  teachers: "firstName,lastName,documentNumber\nAna,Pérez,12345678",
  students: "firstName,lastName,documentNumber,dateOfBirth,courseName\nLucía,Gómez,12345679,2010-05-14,2°A"
};

export default function BulkImport() {
  const [entityType, setEntityType] = useState("students");
  const [file, setFile] = useState(null);
  const [content, setContent] = useState("");
  const [preview, setPreview] = useState(null);
  const [feedback, setFeedback] = useState(null);
  const [busy, setBusy] = useState(false);

  const handleFile = (event) => {
    const selectedFile = event.target.files?.[0];
    setPreview(null);
    setFeedback(null);
    setFile(selectedFile ?? null);
    if (!selectedFile) { setContent(""); return; }
    if (!/\.(csv|txt)$/i.test(selectedFile.name)) {
      setContent("");
      setFeedback({ type: "error", message: "Seleccioná un archivo CSV o TXT." });
      return;
    }
    const reader = new FileReader();
    reader.onload = () => setContent(String(reader.result ?? ""));
    reader.onerror = () => setFeedback({ type: "error", message: "No se pudo leer el archivo." });
    reader.readAsText(selectedFile);
  };

  const handlePreview = async () => {
    if (!content) { setFeedback({ type: "error", message: "Seleccioná un archivo antes de validar." }); return; }
    setBusy(true);
    setFeedback(null);
    try {
      const data = await previewImport({ entityType, content });
      setPreview(data);
      setFeedback(data.canConfirm
        ? { type: "success", message: "El archivo es válido. Confirmá para guardar los registros." }
        : { type: "error", message: "Corregí las filas informadas antes de confirmar la importación." });
    } catch {
      setFeedback({ type: "error", message: "No se pudo validar el archivo." });
    } finally { setBusy(false); }
  };

  const handleConfirm = async () => {
    setBusy(true);
    setFeedback(null);
    try {
      const result = await confirmImport({ entityType, content });
      setFeedback({ type: "success", message: `Se importaron ${result.importedRows} registros correctamente.` });
      setPreview(null);
      setFile(null);
      setContent("");
    } catch (error) {
      setFeedback({ type: "error", message: error.status === 400 ? "El archivo dejó de ser válido. Volvé a revisar la vista previa." : "No se pudo guardar la importación." });
    } finally { setBusy(false); }
  };

  return (
    <main className="bulk-import-page">
      <header className="bulk-import-header"><p>Administración</p><h2>Importación masiva</h2><span>Subí CSV o TXT, validá cada fila y confirmá recién cuando la vista previa sea válida.</span></header>
      <section className="bulk-import-card">
        <div className="bulk-import-controls">
          <label>Tipo de datos<select value={entityType} onChange={(event) => { setEntityType(event.target.value); setPreview(null); setFeedback(null); }} disabled={busy}><option value="students">Alumnos</option><option value="teachers">Docentes</option><option value="courses">Cursos</option></select></label>
          <label>Archivo CSV o TXT<input type="file" accept=".csv,.txt,text/csv,text/plain" onChange={handleFile} disabled={busy} /></label>
          <button type="button" onClick={handlePreview} disabled={busy || !file}>{busy ? "Validando..." : "Validar y previsualizar"}</button>
        </div>
        <p className="bulk-import-template">Columnas requeridas: <code>{templates[entityType].split("\n")[0]}</code>. Ejemplo: <code>{templates[entityType].split("\n")[1]}</code></p>
      </section>
      {feedback && <p className={`bulk-import-feedback bulk-import-feedback--${feedback.type}`} role={feedback.type === "error" ? "alert" : "status"}>{feedback.message}</p>}
      {preview && <section className="bulk-import-card"><div className="bulk-import-preview-title"><h3>Vista previa</h3><span>{preview.validRows} de {preview.totalRows} filas válidas</span></div><div className="bulk-import-table-wrap"><table className="bulk-import-table"><thead><tr><th>Fila</th><th>Valores</th><th>Resultado</th></tr></thead><tbody>{preview.rows.map((row) => <tr key={row.rowNumber} className={row.isValid ? "bulk-import-row--valid" : "bulk-import-row--invalid"}><td data-label="Fila">{row.rowNumber}</td><td data-label="Valores">{row.values.join(" · ")}</td><td data-label="Resultado">{row.isValid ? "Válida" : row.message}</td></tr>)}</tbody></table></div>{preview.canConfirm && <button type="button" className="bulk-import-confirm" onClick={handleConfirm} disabled={busy}>{busy ? "Guardando..." : "Confirmar e importar"}</button>}</section>}
    </main>
  );
}
