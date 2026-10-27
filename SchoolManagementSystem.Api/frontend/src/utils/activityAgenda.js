const agendaGroups = [
  { id: "upcoming", label: "Próximas" },
  { id: "overdue", label: "Vencidas" },
  { id: "undated", label: "Sin fecha" }
];

export function groupActivitiesByDueDate(activities, getDueAt, now = Date.now()) {
  const grouped = Object.fromEntries(agendaGroups.map(({ id }) => [id, []]));

  for (const activity of activities) {
    const dueAt = getDueAt(activity);
    const dueTime = dueAt ? new Date(dueAt).getTime() : Number.NaN;
    const groupId = Number.isFinite(dueTime)
      ? dueTime < now ? "overdue" : "upcoming"
      : "undated";

    grouped[groupId].push({ activity, dueTime });
  }

  return agendaGroups.map(({ id, label }) => ({
    id,
    label,
    activities: grouped[id]
      .sort((left, right) => left.dueTime - right.dueTime)
      .map(({ activity }) => activity)
  }));
}
